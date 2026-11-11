using System.Collections;
using Bezoro.Unity.Core.AsyncLoading;
using Bezoro.Unity.Core.MonoBehaviours;
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
