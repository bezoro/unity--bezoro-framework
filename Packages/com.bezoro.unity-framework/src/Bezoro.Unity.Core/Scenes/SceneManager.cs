using System.Collections.Generic;
using System.Linq;
using Bezoro.Core.Extensions;
using Bezoro.Logging.Types;
using Bezoro.Unity.Core.Common.Enums;
using Bezoro.Unity.Core.Loading;
using UnityEngine;
using UnityEngine.SceneManagement;
using Logger = Bezoro.Logging.Logger;
using Object = UnityEngine.Object;

namespace Bezoro.Unity.Core.Scenes;

/// <summary>
///     Provides functionality to manage scenes within the Unity environment, including loading, unloading,
///     checking scene load states, and handling scene transitions.
/// </summary>
[DefaultExecutionOrder(
	ExecutionOrder.Fundamental
)]
public static class SceneManager
{
	public static int SceneCountInBuildSettings =>
		UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings;

	public static bool IsLoadingScreen(string path) =>
		path == LoadingScreenManager.LoadingScreenPath;

	public static bool IsSceneLoaded(SceneReference scene) => IsSceneLoaded(
		scene.Path
	);

	public static bool IsSceneLoaded(string path) =>
		SceneLoader.LoadedScenePaths.Contains(
			path
		);

	public static HashSet<string> GetAllLoadedScenePaths()
	{
		var loadedScenePaths = new HashSet<string>();

		for (var i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
		{
			var scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(
				i
			);

			loadedScenePaths.Add(
				scene.path
			);
		}

		return loadedScenePaths;
	}

	public static IEnumerable<Scene> GetAllLoadedScenes()
	{
		var loadedScenes = new List<Scene>();
		for (var i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
		{
			loadedScenes.Add(
				UnityEngine.SceneManagement.SceneManager.GetSceneAt(
					i
				)
			);
		}

		return loadedScenes;
	}

	public static int GetLoadedSceneCount() => UnityEngine.SceneManagement.SceneManager.sceneCount;

	public static Scene FindSceneByName(string sceneName)
	{
		for (var i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
		{
			var scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(
				i
			);

			if (scene.name == sceneName) return scene;
		}

		return default;
	}

	public static Scene GetActiveScene() => UnityEngine.SceneManagement.SceneManager.GetActiveScene();

	public static Scene GetLoadedSceneAt(int buildIndex)
	{
		if (buildIndex < 0 || buildIndex >= UnityEngine.SceneManagement.SceneManager.sceneCount)
			return default;

		return UnityEngine.SceneManagement.SceneManager.GetSceneAt(
			buildIndex
		);
	}

	public static Scene GetSceneByPath(string scenePath) =>
		UnityEngine.SceneManagement.SceneManager.GetSceneByPath(
			scenePath
		);

	public static string GetLoadingScreenPath() =>
		LoadingScreenManager.LoadingScreenPath;

	public static T[] GetObjectsOfTypeInScene<T>(Scene scene)
		where T : Object
	{
		var           objects         = new List<T>();
		GameObject[]? rootGameObjects = scene.GetRootGameObjects();

		foreach (var rootGO in rootGameObjects)
		{
			// Search in the entire hierarchy of each root object
			var asyncComponents = rootGO.GetComponentsInChildren<T>(
				true
			);

			objects.AddRange(
				asyncComponents
			);
		}

		return objects.ToArray();
	}

	public static void SetActiveScene(SceneReference sceneReference) => SetActiveScene(
		sceneReference.Path
	);

	public static void SetActiveScene(Scene scene) => SetActiveScene(
		scene.path
	);

	public static void SetActiveScene(string path)
	{
		if (!IsSceneLoaded(
				path
			))
		{
			Logger.LogError(
				$"[{nameof(SceneManager).Bold()}] Cannot set active scene: {path.Italic()} is not loaded"
			);

			return;
		}

		var scene = UnityEngine.SceneManagement.SceneManager.GetSceneByPath(
			path
		);

		UnityEngine.SceneManagement.SceneManager.SetActiveScene(
			scene
		);

		Logger.Log(
			$"Set active scene: {path.Bold()}",
			contextObject: nameof(SceneManager),
			category: LogCategory.Loading
		);
	}
}
