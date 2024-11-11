using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Eflatun.SceneReference;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Bezoro.Framework.Unity.Utilities
{
	public static class SceneManagerUtils
	{
		public static event Action<float> LoadingProgressChanged;
		public static event Action<string> SceneLoadStarted;
		public static event Action<string> SceneLoadCompleted;

		public static async UniTask LoadScene
			(SceneReference sceneReference, LoadSceneMode mode = LoadSceneMode.Single)
		{
			SceneLoadStarted?.Invoke(sceneReference.Path);

			var operation = SceneManager.LoadSceneAsync(sceneReference.Path, mode);

			if (operation == null) { return; }

			operation.allowSceneActivation = true;

			while (!operation.isDone)
			{
				LoadingProgressChanged?.Invoke(operation.progress);
				await UniTask.Yield();
			}

			SceneLoadCompleted?.Invoke(sceneReference.Path);
		}

		public static async UniTask UnloadScene(string path)
		{
			if (!IsSceneLoaded(path))
			{
				BDebug.LogWarning($"Cannot unload scene: {path} is not loaded");

				return;
			}

			var operation = SceneManager.UnloadSceneAsync(path);

			if (operation == null) { return; }

			while (!operation.isDone) { await UniTask.Yield(); }
		}

		public static async UniTask UnloadScene(SceneReference sceneReference)
		{
			await UnloadScene(sceneReference.Path);
		}

		public static bool IsSceneLoaded(string path)
		{
			return SceneManager.GetSceneByPath(path).isLoaded;
		}

		public static bool IsSceneLoaded(SceneReference sceneReference)
		{
			return SceneManager.GetSceneByPath(sceneReference.Path).isLoaded;
		}

		public static void SetActiveScene(SceneReference sceneReference)
		{
			var scene = SceneManager.GetSceneByPath(sceneReference.Path);

			if (scene.isLoaded)
			{
				SceneManager.SetActiveScene(scene);
			}
			else
			{
				Debug.LogWarning($"Cannot set active scene: {sceneReference.Path} is not loaded");
			}
		}

		public static int GetLoadedSceneCount()
		{
			return SceneManager.sceneCount;
		}

		public static Scene[] GetAllLoadedScenes()
		{
			var countLoaded = SceneManager.sceneCount;
			var loadedScenes = new Scene[countLoaded];

			for ( var i = 0 ; i < countLoaded ; i++ )
			{
				loadedScenes[i] = SceneManager.GetSceneAt(i);
			}

			return loadedScenes;
		}

		public static bool IsSceneReferenceLoaded(SceneReference sceneReference)
		{
			return AreScenesLoaded(new[] { sceneReference });
		}

		public static bool AreScenesLoaded
			(IEnumerable<SceneReference> sceneReferences, LoadCheckMode mode = LoadCheckMode.Any)
		{
			var loadedScenes = GetAllLoadedScenes();

			switch ( mode )
			{
				case LoadCheckMode.Any:
					return sceneReferences.Any(reference => loadedScenes.Any(scene => scene.path == reference.Path));

				case LoadCheckMode.All:
					return sceneReferences.All(reference => loadedScenes.Any(scene => scene.path == reference.Path));

				case LoadCheckMode.ExceptAny:
					return loadedScenes.Any(scene => sceneReferences.All(reference => scene.path != reference.Path));

				case LoadCheckMode.ExceptAll:
					return loadedScenes.All(scene => sceneReferences.All(reference => scene.path != reference.Path));

				default:
					return false;
			}
		}

		public enum LoadCheckMode
		{
			Any,
			All,
			ExceptAny,
			ExceptAll
		}

		public static async UniTask LoadSceneWithLoadingScreen
			(Scene currentScene, SceneReference targetScene, SceneReference loadingScene)
		{
			// Load loading screen additively
			await LoadScene(loadingScene, LoadSceneMode.Additive);

			// Unload previous scene
			await UnloadScene(currentScene.path);

			// Load target scene additively
			await LoadScene(targetScene, LoadSceneMode.Additive);

			// Unload loading screen
			await UnloadScene(loadingScene);
		}
	}
}
