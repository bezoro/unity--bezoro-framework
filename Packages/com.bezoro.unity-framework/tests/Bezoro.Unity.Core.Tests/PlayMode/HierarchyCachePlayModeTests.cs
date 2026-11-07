using System.Collections;
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
		public IEnumerator RebuildCache_InPlayMode_RestoresTemporaryChildrenAndComponents()
		{
			Assert.IsTrue(Application.isPlaying);
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
