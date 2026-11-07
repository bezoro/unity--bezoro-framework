#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Bezoro.Unity.Core.AsyncLoading;
using Bezoro.Unity.Core.MonoBehaviours;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Bezoro.Unity.Core.Tests.EditMode
{
	[TestFixture]
	public sealed class HierarchyCacheEditorTests
	{
		private Scene _preview;
		private GameObject _root = null!;
		private HierarchyCache _cache = null!;
		private GameObject? _prefabContents;
		private string? _folder;
		private Scene _originalActive;
		private Scene[] _originalScenes = Array.Empty<Scene>();
		private bool[] _originalDirty = Array.Empty<bool>();
		private GameObject[][] _originalRoots = Array.Empty<GameObject[]>();
		private Object[] _originalSelection = Array.Empty<Object>();
		private Object? _originalSelected;

		[SetUp]
		public void SetUp()
		{
			Assert.IsFalse(UnityEngine.Application.isPlaying);
			_originalActive = SceneManager.GetActiveScene();
			_originalScenes = Enumerable.Range(0, SceneManager.sceneCount).Select(SceneManager.GetSceneAt).ToArray();
			_originalDirty = _originalScenes.Select(scene => scene.isDirty).ToArray();
			_originalRoots = _originalScenes.Select(scene => scene.isLoaded ? scene.GetRootGameObjects() : Array.Empty<GameObject>()).ToArray();
			_originalSelection = Selection.objects;
			_originalSelected = Selection.activeObject;
			_preview = EditorSceneManager.NewPreviewScene();
			_root = CreateOwned("Editor cache root");
			_cache = _root.AddComponent<HierarchyCache>();
		}

		[TearDown]
		public void TearDown()
		{
			var failures = new List<Exception>();
			void Clean(Action action)
			{
				try { action(); }
				catch (Exception error) { failures.Add(error); }
			}
			Clean(() =>
			{
				if (_prefabContents != null)
					PrefabUtility.UnloadPrefabContents(_prefabContents);
			});
			Clean(() =>
			{
				if (_preview.IsValid())
					EditorSceneManager.ClosePreviewScene(_preview);
			});
			Clean(() =>
			{
				if (_folder != null && AssetDatabase.IsValidFolder(_folder))
					Assert.IsTrue(AssetDatabase.DeleteAsset(_folder));
			});
			Clean(() =>
			{
				if (SceneManager.GetActiveScene() != _originalActive)
					SceneManager.SetActiveScene(_originalActive);
				Assert.That(SceneManager.GetActiveScene(), Is.EqualTo(_originalActive));
			});
			Clean(() =>
			{
				Assert.That(Enumerable.Range(0, SceneManager.sceneCount).Select(SceneManager.GetSceneAt), Is.EqualTo(_originalScenes));
				for (int i = 0; i < _originalScenes.Length; i++)
				{
					Assert.That(_originalScenes[i].isDirty, Is.EqualTo(_originalDirty[i]), "A test changed an original scene's dirty flag.");
					if (_originalScenes[i].isLoaded)
						Assert.That(_originalScenes[i].GetRootGameObjects(), Is.EqualTo(_originalRoots[i]));
				}
				Assert.That(Selection.objects, Is.EqualTo(_originalSelection));
				Assert.That(Selection.activeObject, Is.SameAs(_originalSelected));
			});
			_prefabContents = null;
			_folder = null;
			if (failures.Count > 0)
				throw new AggregateException("Owned editor cache test cleanup failed.", failures);
		}

		[TestCase(HideFlags.DontSaveInEditor)]
		[TestCase(HideFlags.DontSaveInBuild)]
		public void RebuildCache_ExcludesTransientSubtreesAndComponents_WhilePublicSnapshotsRemainComplete(HideFlags excluded)
		{
			var authored = CreateOwned("Hidden authored child", _root.transform, HideFlags.HideInHierarchy | HideFlags.HideInInspector);
			var transient = CreateOwned("Transient parent", _root.transform, excluded);
			var nested = CreateOwned("Ordinary child below transient parent", transient.transform);
			var temporaryLight = authored.AddComponent<Light>();
			temporaryLight.hideFlags = excluded;

			_cache.RebuildCache();

			Assert.That(GroupTargets(_cache), Is.EqualTo(new Object[] { _root, authored }), "Hidden authored objects remain, while the entire transient subtree is omitted.");
			CollectionAssert.DoesNotContain(ComponentTargets(_cache), temporaryLight);
			CollectionAssert.DoesNotContain(GroupTargets(_cache), nested);
			Assert.That(new GameObjectState(authored).ComponentStates.Select(state => state.Target), Does.Contain(temporaryLight),
				"The public snapshot constructor must retain its complete runtime contract.");
			Assert.That(new GameObjectState(transient).Target, Is.SameAs(transient));
		}

		[Test]
		public void RebuildCache_TracksAuthoredActivationAndEnabledState()
		{
			var authored = CreateOwned("Authored light", _root.transform);
			var light = authored.AddComponent<Light>();
			_cache.RebuildCache();
			string before = CacheSnapshot(_cache);
			light.enabled = false;
			authored.SetActive(false);
			_cache.RebuildCache();
			Assert.That(CacheSnapshot(_cache), Is.Not.EqualTo(before));
			using var serialized = new SerializedObject(_cache);
			var group = serialized.FindProperty("gameObjectStatesCache").GetArrayElementAtIndex(1);
			Assert.IsFalse(group.FindPropertyRelative("wasInitiallyActive").boolValue);
			var components = group.FindPropertyRelative("componentStates");
			var entry = Enumerable.Range(0, components.arraySize).Select(components.GetArrayElementAtIndex)
				.Single(item => item.FindPropertyRelative("target").objectReferenceValue == light);
			Assert.IsFalse(entry.FindPropertyRelative("wasInitiallyEnabled").boolValue);
		}

		[Test]
		public void RebuildCache_StartDisabledMembershipIgnoresExcludedAncestorsAndPreservesExistingOrder()
		{
			var first = CreateOwned("First authored target", _root.transform);
			var second = CreateOwned("Second authored target", _root.transform);
			var outside = CreateOwned("Excluded outside ancestor", flags: HideFlags.DontSaveInEditor);
			var excludedTarget = CreateOwned("Ordinary target below excluded ancestor", outside.transform);
			var disabled = _root.AddComponent<StartDisabled>();
			SetDisabledTargets(disabled, first, excludedTarget, second);
			_cache.RebuildCache();
			var initial = DisabledTargets(_cache);
			CollectionAssert.AreEquivalent(new[] { first, second }, initial);
			string before = CacheSnapshot(_cache);

			SetDisabledTargets(disabled, second, first, excludedTarget);
			_cache.RebuildCache();

			Assert.That(DisabledTargets(_cache), Is.EqualTo(initial), "Equivalent membership must preserve the serialized order.");
			Assert.That(CacheSnapshot(_cache), Is.EqualTo(before));
		}

		[UnityTest]
		public IEnumerator SavedPrefabReloadAndInstanceRebuild_PreserveIdentitiesOverridesAndDirtyState()
		{
			CreateOwned("Authored child", _root.transform);
			var asyncGameObject = _root.AddComponent<AsyncGameObject>();
			using (var serialized = new SerializedObject(asyncGameObject))
			{
				serialized.FindProperty("hierarchyCacheReference").objectReferenceValue = _cache;
				serialized.ApplyModifiedPropertiesWithoutUndo();
			}
			_cache.RebuildCache();
			_folder = "Assets/OwnedHierarchyCacheTests_" + Guid.NewGuid().ToString("N");
			Assert.That(AssetDatabase.CreateFolder("Assets", _folder.Substring("Assets/".Length)), Is.Not.Empty);
			string path = _folder + "/Cache.prefab";
			var asset = PrefabUtility.SaveAsPrefabAsset(_root, path, out bool saved);
			Assert.IsTrue(saved);
			Assert.IsTrue(asset != null);
			var identities = PersistentIdentities(asset!);
			string guid = AssetDatabase.AssetPathToGUID(path);

			_prefabContents = PrefabUtility.LoadPrefabContents(path);
			var loadedCache = _prefabContents.GetComponent<HierarchyCache>();
			CreateOwned("Temporary render child", _prefabContents.transform, HideFlags.DontSaveInEditor, _prefabContents.scene);
			var helper = _prefabContents.AddComponent<Light>();
			helper.hideFlags = HideFlags.DontSaveInEditor;
			string cacheBefore = CacheSnapshot(loadedCache);
			loadedCache.RebuildCache();
			Assert.That(CacheSnapshot(loadedCache), Is.EqualTo(cacheBefore));
			PrefabUtility.SaveAsPrefabAsset(_prefabContents, path, out saved);
			Assert.IsTrue(saved);
			PrefabUtility.UnloadPrefabContents(_prefabContents);
			_prefabContents = null;
			asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
			Assert.That(AssetDatabase.AssetPathToGUID(path), Is.EqualTo(guid));
			Assert.That(PersistentIdentities(asset), Is.EqualTo(identities));
			Assert.That(asset.GetComponentsInChildren<Transform>(true).Select(node => node.name), Does.Not.Contain("Temporary render child"));
			Assert.That(asset.GetComponents<Light>(), Is.Empty);

			var instance = (GameObject)PrefabUtility.InstantiatePrefab(asset, _preview);
			var instanceCache = instance.GetComponent<HierarchyCache>();
			CreateOwned("Temporary instance child", instance.transform, HideFlags.DontSaveInEditor);
			cacheBefore = CacheSnapshot(instanceCache);
			string[] overridesBefore = OverrideSnapshot(instance);
			bool dirtyBefore = EditorUtility.IsDirty(instanceCache);
			bool sceneDirtyBefore = _preview.isDirty;
			instanceCache.RebuildCache();
			yield return null; // Let native hierarchy callbacks run before checking the same live instance again.
			instanceCache.RebuildCache();
			Assert.That(CacheSnapshot(instanceCache), Is.EqualTo(cacheBefore));
			Assert.That(OverrideSnapshot(instance), Is.EqualTo(overridesBefore));
			Assert.That(EditorUtility.IsDirty(instanceCache), Is.EqualTo(dirtyBefore));
			Assert.That(_preview.isDirty, Is.EqualTo(sceneDirtyBefore));
		}

		private GameObject CreateOwned(string name, Transform? parent = null, HideFlags flags = HideFlags.None, Scene destination = default)
		{
			var go = EditorUtility.CreateGameObjectWithHideFlags(name, HideFlags.HideAndDontSave);
			SceneManager.MoveGameObjectToScene(go, destination.IsValid() ? destination : _preview);
			if (parent != null)
				go.transform.SetParent(parent, false);
			go.hideFlags = flags;
			return go;
		}

		private static Object[] GroupTargets(HierarchyCache cache)
		{
			using var serialized = new SerializedObject(cache);
			var groups = serialized.FindProperty("gameObjectStatesCache");
			return Enumerable.Range(0, groups.arraySize).Select(i => groups.GetArrayElementAtIndex(i).FindPropertyRelative("target").objectReferenceValue).ToArray();
		}

		private static Object[] ComponentTargets(HierarchyCache cache)
		{
			using var serialized = new SerializedObject(cache);
			var groups = serialized.FindProperty("gameObjectStatesCache");
			return Enumerable.Range(0, groups.arraySize).SelectMany(i =>
			{
				var states = groups.GetArrayElementAtIndex(i).FindPropertyRelative("componentStates");
				return Enumerable.Range(0, states.arraySize).Select(j => states.GetArrayElementAtIndex(j).FindPropertyRelative("target").objectReferenceValue);
			}).ToArray();
		}

		private static Object[] DisabledTargets(HierarchyCache cache)
		{
			using var serialized = new SerializedObject(cache);
			var items = serialized.FindProperty("startDisabledObjects");
			return Enumerable.Range(0, items.arraySize).Select(i => items.GetArrayElementAtIndex(i).objectReferenceValue).ToArray();
		}

		private static void SetDisabledTargets(StartDisabled disabled, params GameObject[] targets)
		{
			using var serialized = new SerializedObject(disabled);
			var items = serialized.FindProperty("gameObjects");
			items.arraySize = targets.Length;
			for (int i = 0; i < targets.Length; i++)
				items.GetArrayElementAtIndex(i).objectReferenceValue = targets[i];
			serialized.ApplyModifiedPropertiesWithoutUndo();
		}

		private static string ObjectId(Object? target) => target == null ? "null" : target.GetEntityId().ToString();

		private static string CacheSnapshot(HierarchyCache cache)
		{
			using var serialized = new SerializedObject(cache);
			var groups = serialized.FindProperty("gameObjectStatesCache");
			var values = new List<string>();
			for (int i = 0; i < groups.arraySize; i++)
			{
				var group = groups.GetArrayElementAtIndex(i);
				values.Add(ObjectId(group.FindPropertyRelative("target").objectReferenceValue) + ":" + group.FindPropertyRelative("wasInitiallyActive").boolValue);
				var states = group.FindPropertyRelative("componentStates");
				for (int j = 0; j < states.arraySize; j++)
				{
					var state = states.GetArrayElementAtIndex(j);
					values.Add(ObjectId(state.FindPropertyRelative("target").objectReferenceValue) + ":" + state.FindPropertyRelative("wasInitiallyEnabled").boolValue);
				}
			}
			values.AddRange(DisabledTargets(cache).Select(ObjectId));
			return string.Join("\n", values);
		}

		private static string[] PersistentIdentities(GameObject asset)
		{
			return asset.GetComponentsInChildren<Transform>(true)
				.SelectMany(node => new Object[] { node.gameObject }.Concat(node.GetComponents<Component>().Cast<Object>()))
				.Select(target =>
				{
					Assert.IsTrue(AssetDatabase.TryGetGUIDAndLocalFileIdentifier(target, out string guid, out long id));
					return guid + ":" + id + ":" + target.GetType().FullName;
				})
				.OrderBy(value => value, StringComparer.Ordinal).ToArray();
		}

		private static string[] OverrideSnapshot(GameObject instance) => (PrefabUtility.GetPropertyModifications(instance) ?? Array.Empty<PropertyModification>())
			.Select(item => ObjectId(item.target) + ":" + item.propertyPath + ":" + item.value + ":" + ObjectId(item.objectReference)).OrderBy(value => value, StringComparer.Ordinal).ToArray();
	}
}
