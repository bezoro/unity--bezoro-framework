using System.Collections;
using Bezoro.Unity.Core.AsyncLoading;
using Bezoro.Unity.Core.MonoBehaviours;
using Bezoro.Unity.Core.Loading;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Bezoro.Unity.Core.Tests.PlayMode
{
	[TestFixture]
	public sealed class HierarchyCachePlayModeTests
	{
		private GameObject _root;

		[TearDown]
		public void TearDown()
		{
			if (_root != null)
				Object.DestroyImmediate(_root);
		}

		[UnityTest]
		public IEnumerator LoadAsync_PreCancelled_DoesNotMarkLoadedAndCanRetry()
		{
			_root = new GameObject("Cancelled async hierarchy");
			_root.SetActive(false);
			var asyncObject = _root.AddComponent<AsyncGameObject>();
			bool cancelled = false;
			yield return asyncObject.LoadAsync(new System.Threading.CancellationToken(true))
				.SuppressCancellationThrow().ToCoroutine(result => cancelled = result);
			Assert.IsTrue(cancelled);
			Assert.IsFalse(asyncObject.IsLoaded);
			yield return asyncObject.LoadAsync(default).ToCoroutine();
			Assert.IsTrue(asyncObject.IsLoaded);
		}

		[UnityTest]
		public IEnumerator LoadAsync_CancelAfterFirstGroup_KeepsRemainingHierarchyRetryable()
		{
			_root = new GameObject("Interrupted hierarchy");
			_root.SetActive(false);
			var child = new GameObject("Group still to be enabled");
			child.transform.SetParent(_root.transform);
			var renderer = child.AddComponent<MeshRenderer>();
			var asyncObject = _root.AddComponent<AsyncGameObject>();
			var cache = _root.GetComponent<HierarchyCache>();
			cache.RebuildCache();
			cache.DisableGroupsCache();
			using var progress = new TestGlobalProvider<LoadingProgressService>("Interrupted progress");
			var service = progress.Instance;
			using var cancellation = new System.Threading.CancellationTokenSource();
			service.ProgressChanged += value =>
			{
				if (value > 0 && value < 1) cancellation.Cancel();
			};
			service.BeginOperation("Interrupt hierarchy");
			bool cancelled = false;
			yield return asyncObject.LoadAsync(cancellation.Token).SuppressCancellationThrow()
				.ToCoroutine(result => cancelled = result);
			Assert.IsTrue(cancelled);
			Assert.IsFalse(asyncObject.IsLoaded);
			Assert.IsFalse(renderer.enabled);
			yield return asyncObject.LoadAsync(default).ToCoroutine();
			Assert.IsTrue(asyncObject.IsLoaded);
			Assert.IsTrue(renderer.enabled);
		}

		[UnityTest]
		public IEnumerator EnableGroupsCacheAsync_RepeatedActivationCompletesBothProgressOperations()
		{
			_root = new GameObject("Two hierarchy activations");
			var cache = _root.AddComponent<HierarchyCache>();
			cache.RebuildCache();
			using var progress = new TestGlobalProvider<LoadingProgressService>("Cache progress service");
			var service = progress.Instance;
			for (int i = 0; i < 2; i++)
			{
				service.BeginOperation("Restore " + i);
				yield return cache.EnableGroupsCacheAsync().ToCoroutine();
				float deadline = Time.realtimeSinceStartup + 5f;
				while (service.IsLoading && Time.realtimeSinceStartup < deadline) yield return null;
				Assert.IsFalse(service.IsLoading, "Activation " + i + " left an unfinished stage.");
			}
		}

		[UnityTest]
		public IEnumerator RebuildCache_RestoresEnabledNativePropertiesAndRetainsInitiallyDisabledComponents()
		{
			_root = new GameObject("Native enabled hierarchy cache");
			var cache = _root.AddComponent<HierarchyCache>();
			var enabledRenderer = _root.AddComponent<MeshRenderer>();
			var disabledCollider = _root.AddComponent<BoxCollider>();
			disabledCollider.enabled = false;
			var child = new GameObject("Mixed native component states");
			child.transform.SetParent(_root.transform);
			var disabledRenderer = child.AddComponent<MeshRenderer>();
			disabledRenderer.enabled = false;
			var enabledCollider = child.AddComponent<BoxCollider>();
			var other = new GameObject("Other enabled properties");
			other.transform.SetParent(_root.transform);
			var enabledCollider2D = other.AddComponent<BoxCollider2D>();
			var disabledLod = other.AddComponent<LODGroup>();
			disabledLod.enabled = false;

			cache.RebuildCache();
			cache.DisableGroupsCache();

			Assert.IsFalse(_root.activeSelf);
			Assert.IsFalse(child.activeSelf);
			Assert.IsFalse(other.activeSelf);
			Assert.IsFalse(enabledRenderer.enabled);
			Assert.IsFalse(disabledRenderer.enabled);
			Assert.IsFalse(enabledCollider.enabled);
			Assert.IsFalse(disabledCollider.enabled);
			Assert.IsFalse(enabledCollider2D.enabled);
			Assert.IsFalse(disabledLod.enabled);

			yield return cache.EnableGroupsCacheAsync().ToCoroutine();

			Assert.IsTrue(_root.activeSelf);
			Assert.IsTrue(child.activeSelf);
			Assert.IsTrue(other.activeSelf);
			Assert.IsTrue(enabledRenderer.enabled);
			Assert.IsFalse(disabledRenderer.enabled);
			Assert.IsTrue(enabledCollider.enabled);
			Assert.IsFalse(disabledCollider.enabled);
			Assert.IsTrue(enabledCollider2D.enabled);
			Assert.IsFalse(disabledLod.enabled);
		}

		[UnityTest]
		public IEnumerator ActivateAsync_WithoutRespectingInitiallyDisabled_EnablesNativeProperties()
		{
			_root = new GameObject("Explicit native component activation");
			var renderer = _root.AddComponent<MeshRenderer>();
			var collider = _root.AddComponent<BoxCollider>();
			var lod = _root.AddComponent<LODGroup>();
			renderer.enabled = false;
			collider.enabled = false;
			lod.enabled = false;
			var state = new GameObjectState(_root);
			state.DeactivateAll();

			yield return state.ActivateAsync(respectInitiallyDisabled: false).ToCoroutine();

			Assert.IsTrue(_root.activeSelf);
			Assert.IsTrue(renderer.enabled);
			Assert.IsTrue(collider.enabled);
			Assert.IsTrue(lod.enabled);
		}

		[UnityTest]
		public IEnumerator RebuildCache_InPlayMode_RestoresTemporaryChildrenAndComponents()
		{
			Assert.IsTrue(UnityEngine.Application.isPlaying);
			_root = new GameObject("Runtime hierarchy cache");
			var cache = _root.AddComponent<HierarchyCache>();
			var child = new GameObject("Runtime helper");
			child.transform.SetParent(_root.transform);
			child.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
			var light = child.AddComponent<Light>();
			light.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;

			cache.RebuildCache();
			cache.DisableGroupsCache();

			Assert.IsFalse(child.activeSelf);
			Assert.IsFalse(light.enabled);

			yield return cache.EnableGroupsCacheAsync().ToCoroutine();

			Assert.IsTrue(child.activeSelf);
			Assert.IsTrue(light.enabled);
		}
	}
}
