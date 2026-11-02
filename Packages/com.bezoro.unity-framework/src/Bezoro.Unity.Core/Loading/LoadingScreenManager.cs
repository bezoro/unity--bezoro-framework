using System;
using System.Collections.Generic;
using System.Linq;
using Bezoro.Core.Extensions;
using Bezoro.Unity.Core.MonoBehaviours;
using Bezoro.Unity.Core.ScriptableObjects;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using SceneManager = Bezoro.Unity.Core.Scenes.SceneManager;

namespace Bezoro.Unity.Core.Loading;

/// <summary>
///     Shows a loading screen while loading one or more scenes and updates progress tracking.
/// </summary>
public static class LoadingScreenManager
{
	private const float WEIGHT = .25f;

	private static string? _loadingScreenPath;

	public static event Action? LoadStarted;

	public static Scene LoadingScreenScene => SceneManager.GetSceneByPath(LoadingScreenPath);

	public static string LoadingScreenPath
	{
		get => string.IsNullOrEmpty(_loadingScreenPath)
				   ? GameSettings.Instance?.LoadingScreen?.Path ?? string.Empty
				   : _loadingScreenPath;

		private set => _loadingScreenPath = value;
	}

	public static SceneLoadHandle Hide() => SceneLoader.Unload(LoadingScreenPath);

	public static SceneLoadHandle Show() =>
		SceneLoader.Load(LoadingScreenPath, LoadSceneMode.Additive);

	public static async UniTask LoadScenesWithLoadingScreen(
		ICollection<string> scenePaths,
		LoadSceneMode       mode,
		string?             loadingScreenPath) =>
		await LoadScenesWithLoadingScreen(
			Array.Empty<string>(),
			scenePaths.ToArray(),
			mode,
			loadingScreenPath
		);

	public static async UniTask LoadScenesWithLoadingScreen(
		IReadOnlyList<string> providerScenePaths,
		IReadOnlyList<string> remainingScenePaths,
		LoadSceneMode         mode,
		string?               loadingScreenPath)
	{
		if (providerScenePaths == null)
			throw new ArgumentNullException(nameof(providerScenePaths));

		if (remainingScenePaths == null)
			throw new ArgumentNullException(nameof(remainingScenePaths));

		if (providerScenePaths.Count == 0 && remainingScenePaths.Count == 0)
			throw new ArgumentException("At least one scene path is required.");

		string fallbackLoadingScreenPath = GameSettings.Instance?.LoadingScreen?.Path ?? string.Empty;
		LoadingScreenPath = string.IsNullOrEmpty(loadingScreenPath) ? fallbackLoadingScreenPath : loadingScreenPath;

		if (string.IsNullOrEmpty(LoadingScreenPath))
			throw new InvalidOperationException("No loading screen scene path is configured.");

		LoadStarted?.SafeInvoke();

		if (LoadingProgressService.TryGetInstance(out var progressService))
			await LoadWithProgressTracking(
				providerScenePaths,
				remainingScenePaths,
				mode,
				LoadingScreenPath,
				progressService
			);
		else
			await LoadWithoutProgressTracking(
				providerScenePaths,
				remainingScenePaths,
				mode,
				LoadingScreenPath
			);
	}

	public static void ResetState() => LoadingScreenPath = string.Empty;

	private static async UniTask LoadWithoutProgressTracking(
		IReadOnlyList<string> providerScenePaths,
		IReadOnlyList<string> remainingScenePaths,
		LoadSceneMode         mode,
		string?               loadingScreenPath)
	{
		if (string.IsNullOrEmpty(loadingScreenPath))
			throw new ArgumentException("A loading screen scene path is required.", nameof(loadingScreenPath));

		await Show();

		if (mode == LoadSceneMode.Single)
			await SceneLoader.UnloadAll(new[] { loadingScreenPath });

		await LoadScenePlan(providerScenePaths, remainingScenePaths);
		Hide().Task.Forget();
	}

	private static async UniTask LoadWithProgressTracking(
		IReadOnlyList<string>   providerScenePaths,
		IReadOnlyList<string>   remainingScenePaths,
		LoadSceneMode           mode,
		string?                 loadingScreenPath,
		LoadingProgressService  progressService)
	{
		if (string.IsNullOrEmpty(loadingScreenPath))
			throw new ArgumentException("A loading screen scene path is required.", nameof(loadingScreenPath));

		if (!progressService.IsLoading)
		{
			string? firstScenePath = providerScenePaths.FirstOrDefault() ??
									 remainingScenePaths.FirstOrDefault();
			progressService.BeginOperation(
				string.IsNullOrEmpty(firstScenePath)
					? "Scene Loading"
					: $"Scene Loading - {firstScenePath}"
			);
		}

		var stage = progressService.BeginStage("Load Scenes With LoadingScreen", WEIGHT);

		var showTracker = stage.CreateTracker("Show Loading Screen");

		var unloadAllTracker = stage.CreateTracker("Unload All");

		var showHandle = Show();
		showTracker.BindToVariable(() => showHandle.Progress);

		await showHandle;

		if (mode == LoadSceneMode.Single)
		{
			var unloadAllHandle = SceneLoader.UnloadAll(new[] { loadingScreenPath });

			unloadAllTracker.BindToVariable(() => unloadAllHandle.Progress);

			await unloadAllHandle;
		}

		var unloadOnLoaded = Object.FindAnyObjectByType<UnloadSceneOnLoadingComplete>();

		for (var i = 0; i < providerScenePaths.Count; i++)
		{
			var providerTracker = stage.CreateTracker($"Load Provider Scene {i + 1}");
			var providerHandle  = SceneLoader.Load(providerScenePaths[i], LoadSceneMode.Additive);
			providerTracker.BindToVariable(() => providerHandle.Progress);
			await providerHandle;
		}

		if (remainingScenePaths.Count > 0)
		{
			var loadTracker = stage.CreateTracker("Load Scenes");
			var loadHandle  = SceneLoader.Load(remainingScenePaths, LoadSceneMode.Additive);
			loadTracker.BindToVariable(() => loadHandle.Progress);
			await loadHandle;
		}

		stage.Complete();

		if (unloadOnLoaded != null) return;

		await UniTask.WaitWhile(() => progressService.IsLoading);
		Hide().Task.Forget();
	}

	private static async UniTask LoadScenePlan(
		IReadOnlyList<string> providerScenePaths,
		IReadOnlyList<string> remainingScenePaths)
	{
		for (var i = 0; i < providerScenePaths.Count; i++)
			await SceneLoader.Load(providerScenePaths[i], LoadSceneMode.Additive);

		if (remainingScenePaths.Count > 0)
			await SceneLoader.Load(remainingScenePaths, LoadSceneMode.Additive);
	}
}
