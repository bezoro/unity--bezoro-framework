using System;
using System.Linq;
using Bezoro.Unity.Core.Common.Enums;
using Bezoro.Unity.Core.Loading;
using Bezoro.Unity.Core.Scenes;
using Bezoro.Unity.Core.ScriptableObjects;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using SceneManager = Bezoro.Unity.Core.Scenes.SceneManager;

namespace Bezoro.Unity.Core.MonoBehaviours
{
	[DefaultExecutionOrder(
		ExecutionOrder.Late
	)]
	/// <summary>
	/// Loads the configured initial scene collection after the boot scene starts.
	/// </summary>
	public class BootSceneLoader : MonoBehaviour
	{
		private readonly BootSceneLoaderModel _model = new();

		[Tooltip(
			"The scene that should actually start the game"
		)]
		[SerializeField] private SceneCollection? initialScene;

		[Tooltip(
			"Optional custom loading-screen scene, leave empty to use the global default"
		)]
		[SerializeField] private SceneReference? loadingScreenScene;

		public static event Action? BootSceneLoaded;

		#region Unity Lifecycle

		private void Start() => LoadInitialSceneAsync().Forget();

		#endregion

		private string[]? GetPathsToLoad()
		{
			if (initialScene != null)
			{
				var sceneRefs = initialScene.Scenes?.Where(r => r != null
				).ToArray();

				var decision = _model.DecidePathsToLoad(
					true,
					sceneRefs?.Paths().ToArray(),
					SceneManager.GetActiveScene().buildIndex,
					SceneManager.SceneCountInBuildSettings
				);

				if (decision.Status == BootSceneLoaderPathStatus.EmptySceneCollection)
				{
					Debug.LogError(
						$"{nameof(BootSceneLoader)}: SceneCollection `{initialScene.name}` doesn't contain any valid scenes."
					);

					return null;
				}

				return decision.Paths;
			}

			var fallbackDecision = _model.DecidePathsToLoad(
				false,
				null,
				SceneManager.GetActiveScene().buildIndex,
				SceneManager.SceneCountInBuildSettings
			);

			if (!fallbackDecision.NextBuildIndex.HasValue)
				return null;

			return new[]
			{
				SceneUtility.GetScenePathByBuildIndex(
					fallbackDecision.NextBuildIndex.Value
				)
			};
		}

		private async UniTaskVoid LoadInitialSceneAsync()
		{
			await UniTask.Yield();

			string[]? pathsToLoad = GetPathsToLoad();

			if (pathsToLoad is null || pathsToLoad.Length == 0)
			{
				Debug.LogError(
					$"{nameof(BootSceneLoader)}: nothing to load."
				);

				return;
			}

			await LoadingScreenManager.LoadScenesWithLoadingScreen(
				pathsToLoad,
				LoadSceneMode.Single,
				loadingScreenScene!
			);

			BootSceneLoaded?.Invoke();
		}
	}

	public enum BootSceneLoaderPathStatus
	{
		UseSceneCollection,
		EmptySceneCollection,
		UseNextBuildScene,
		NoNextBuildScene
	}

	public readonly struct BootSceneLoaderPathDecision
	{
		public BootSceneLoaderPathDecision(
			BootSceneLoaderPathStatus status,
			string[]?                 paths,
			int?                      nextBuildIndex)
		{
			Status         = status;
			Paths          = paths;
			NextBuildIndex = nextBuildIndex;
		}

		public BootSceneLoaderPathStatus Status { get; }
		public string[]? Paths { get; }
		public int? NextBuildIndex { get; }
	}

	public sealed class BootSceneLoaderModel
	{
		public BootSceneLoaderPathDecision DecidePathsToLoad(
			bool      hasInitialSceneCollection,
			string[]? collectionPaths,
			int       activeBuildIndex,
			int       sceneCountInBuildSettings)
		{
			if (hasInitialSceneCollection)
			{
				if (collectionPaths is not { Length: > 0 })
					return new(BootSceneLoaderPathStatus.EmptySceneCollection, null, null);

				return new(BootSceneLoaderPathStatus.UseSceneCollection, collectionPaths, null);
			}

			int nextIndex = activeBuildIndex + 1;
			if (nextIndex >= sceneCountInBuildSettings)
				return new(BootSceneLoaderPathStatus.NoNextBuildScene, null, null);

			return new(BootSceneLoaderPathStatus.UseNextBuildScene, null, nextIndex);
		}
	}
}
