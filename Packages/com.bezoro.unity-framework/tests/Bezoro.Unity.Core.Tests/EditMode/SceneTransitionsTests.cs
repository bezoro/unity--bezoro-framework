using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Bezoro.Unity.Core.Dependencies;
using Bezoro.Unity.Core.Loading;
using Bezoro.Unity.Core.MonoBehaviours;
using Bezoro.Unity.Core.Scenes;
using Bezoro.Unity.Core.ScriptableObjects;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using NativeScenes = UnityEngine.SceneManagement.SceneManager;

namespace Bezoro.Unity.Core.Tests.EditMode
{
	[TestFixture]
	public sealed class SceneTransitionsTests
	{
		private const string FixtureDirectory = "Packages/com.bezoro.unity-framework/tests/Bezoro.Unity.Core.Tests/EditMode/SceneLoadingFixtures";

		private const string First = FixtureDirectory + "/First.unity";
		private const string Second = FixtureDirectory + "/Second.unity";
		private const string Screen = FixtureDirectory + "/Screen.unity";
		private const string SettingsKey = "Bezoro.Tests.SceneTransitions.BuildSettings";

		[Serializable]
		private sealed class BuildSceneSnapshot
		{
			public string[] paths;
			public bool[] enabled;
		}

		[UnitySetUp]
		public IEnumerator EnterTestPlayer()
		{
			if (!UnityEngine.Application.isPlaying && string.IsNullOrEmpty(SessionState.GetString(SettingsKey, "")))
			{
				var scenes = EditorBuildSettings.scenes;
				SessionState.SetString(SettingsKey, JsonUtility.ToJson(new BuildSceneSnapshot
				{
					paths = scenes.Select(scene => scene.path).ToArray(),
					enabled = scenes.Select(scene => scene.enabled).ToArray()
				}));
				EditorBuildSettings.scenes = scenes.Where(scene => scene.path != First && scene.path != Second && scene.path != Screen)
					.Concat(new[] { new EditorBuildSettingsScene(First, true), new EditorBuildSettingsScene(Second, true), new EditorBuildSettingsScene(Screen, true) })
					.ToArray();
			}
			yield return new EnterPlayMode();
		}

		[UnityTearDown]
		public IEnumerator ExitTestPlayer()
		{
			if (UnityEngine.Application.isPlaying) yield return new ExitPlayMode();
			string json = SessionState.GetString(SettingsKey, "");
			if (string.IsNullOrEmpty(json)) yield break;
			var original = JsonUtility.FromJson<BuildSceneSnapshot>(json);
			EditorBuildSettings.scenes = original.paths.Select((path, i) =>
				new EditorBuildSettingsScene(path, original.enabled[i])).ToArray();
			SessionState.EraseString(SettingsKey);
		}

		[UnityTest]
		public IEnumerator SceneTransitions_DeferredOwnersAndSingleCollectionRespectNativeCompletion()
		{
			string first = First;
			string second = Second;
			var collection = ScriptableObject.CreateInstance<SceneCollection>();
			SceneLoadHandle owner = null;
			int unloaded = 0;
			void CountUnload(string path) { if (path == first) unloaded++; }
			try
			{
				owner = SceneLoader.Load(first, LoadSceneMode.Additive, allowSceneActivation: false);
				var observer = SceneLoader.Load(first, LoadSceneMode.Additive);
				observer.Cancel();
				var queued = SceneLoader.Load(second, LoadSceneMode.Additive);
				yield return Until(() => owner.IsReadyForActivation);
				Assert.IsFalse(queued.IsDone);
				owner.Activate();
				yield return Until(() => owner.IsDone && queued.IsDone);
				Assert.IsTrue(observer.IsCancelled);

				SceneLoader.SceneUnloadCompleted += CountUnload;
				using (var cancellation = new CancellationTokenSource())
				{
					var unload = SceneLoader.Unload(first, cancellation.Token);
					cancellation.Cancel();
					yield return Until(() => unloaded == 1 && unload.IsDoneOrCancelled);
					Assert.IsTrue(unload.IsCancelled);
				}

				owner = SceneLoader.Load(first, LoadSceneMode.Additive, allowSceneActivation: false);
				yield return Until(() => owner.IsReadyForActivation);
				owner.Cancel();
				yield return Until(() => NativeScenes.GetSceneByPath(first).isLoaded);
				var existing = SceneLoader.Load(first, LoadSceneMode.Additive);
				yield return Until(() => existing.IsDone);

				collection.SetScenes(new List<SceneReference> { new(first), new(second) });
				yield return collection.Load(false, LoadSceneMode.Single, null).ToCoroutine();
				Assert.IsTrue(NativeScenes.GetSceneByPath(first).isLoaded);
				Assert.IsTrue(NativeScenes.GetSceneByPath(second).isLoaded);
				Assert.AreEqual(first, NativeScenes.GetActiveScene().path);
			}
			finally
			{
				owner?.Activate();
				SceneLoader.SceneUnloadCompleted -= CountUnload;
				UnityEngine.Object.DestroyImmediate(collection);
			}
		}

		[UnityTest]
		public IEnumerator Boot_WithProviderInLastEntry_LoadsProviderBeforeGameplayAndSelectsFirstScene()
		{
			var initial = SceneLoader.Load(First, LoadSceneMode.Single);
			yield return Until(() => initial.IsDone);
			var collection = ScriptableObject.CreateInstance<SceneCollection>();
			var dependency = ScriptableObject.CreateInstance<GlobalDependencyDefinition>();
			var bootObject = new GameObject("Test boot");
			var loaded = new List<string>();
			bool finished = false;
			void Record(Scene scene) => loaded.Add(scene.path);
			void OnBooted() => finished = true;
			try
			{
				collection.SetEntries(new[]
				{
					new SceneCollectionEntry(new SceneReference(First)),
					new SceneCollectionEntry(new SceneReference(Second), dependency)
				});
				bootObject.SetActive(false);
				var boot = bootObject.AddComponent<BootSceneLoader>();
				var serialized = new SerializedObject(boot);
				serialized.FindProperty("initialScene").objectReferenceValue = collection;
				var screen = serialized.FindProperty("loadingScreenScene");
				screen.FindPropertyRelative("sceneAsset").objectReferenceValue = AssetDatabase.LoadAssetAtPath<SceneAsset>(Screen);
				screen.FindPropertyRelative("scenePath").stringValue = Screen;
				serialized.ApplyModifiedPropertiesWithoutUndo();
				SceneLoader.SceneLoadCompleted += Record;
				BootSceneLoader.BootSceneLoaded += OnBooted;
				bootObject.SetActive(true);
				yield return Until(() => finished);
				Assert.That(loaded.IndexOf(Second), Is.GreaterThanOrEqualTo(0));
				Assert.That(loaded.IndexOf(First), Is.GreaterThan(loaded.IndexOf(Second)));
				Assert.AreEqual(First, NativeScenes.GetActiveScene().path);
			}
			finally
			{
				BootSceneLoader.BootSceneLoaded -= OnBooted;
				SceneLoader.SceneLoadCompleted -= Record;
				if (bootObject != null) UnityEngine.Object.DestroyImmediate(bootObject);
				UnityEngine.Object.DestroyImmediate(collection);
				UnityEngine.Object.DestroyImmediate(dependency);
			}
		}

		private static IEnumerator Until(Func<bool> condition)
		{
			float deadline = Time.realtimeSinceStartup + 15f;
			while (!condition() && Time.realtimeSinceStartup < deadline) yield return null;
			Assert.IsTrue(condition(), "The expected scene operation did not complete within 15 seconds.");
		}
	}
}
