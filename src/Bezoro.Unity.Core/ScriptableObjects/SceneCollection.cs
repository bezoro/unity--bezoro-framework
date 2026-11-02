using System;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bezoro.Core.Extensions;
using Bezoro.Logging.Types;
using Bezoro.Unity.Core.Loading;
using Bezoro.Unity.Core.Scenes;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Logger = Bezoro.Logging.Logger;
using SceneManager = Bezoro.Unity.Core.Scenes.SceneManager;


namespace Bezoro.Unity.Core.ScriptableObjects
{
	[CreateAssetMenu(
		fileName = "SC_NewSceneCollection",
		menuName = "Bezoro/Runtime/Scene Collection",
		order = 0
	)]
	/// <summary>
	/// Stores a loadable set of scenes with loading-screen and load-mode options.
	/// </summary>
	public sealed class SceneCollection : ScriptableObject, ISerializationCallbackReceiver
	{
		[Tooltip(
			"The name of the collection."
		)]
		[SerializeField] private new string? name;

		[HideInInspector]
		[SerializeField] private List<SceneReference>? scenes = new();

		[Tooltip(
			"The scenes to load when the collection is loaded."
		)]
		[SerializeField] private List<SceneCollectionEntry> sceneEntries = new();

		[SerializeField] private bool useLoadingScreen = true;

		[Tooltip(
			"The loading screen to use when loading the collection."
		)]
		[SerializeField] private SceneReference? loadScreenOverride;

		[Tooltip(
			"Determines if the collection should be loaded additively or replace the current scene(s)"
		)]
		[SerializeField] private LoadSceneMode loadMode = LoadSceneMode.Single;

		public IReadOnlyList<SceneCollectionEntry> Entries => sceneEntries;
		public IReadOnlyList<SceneReference> Scenes
		{
			get
			{
				EnsureEntriesMigrated();
				return sceneEntries
					   .Where(entry => entry?.Scene != null)
					   .Select(entry => entry.Scene!)
					   .ToArray();
			}
		}

		public string? Name => name;

		public async UniTask Load()
		{
			EnsureEntriesMigrated();
			var plan = SceneCollectionLoadPlan.Create(sceneEntries);
			if (plan.ProviderScenePaths.Count == 0 && plan.RemainingScenePaths.Count == 0)
				throw new InvalidOperationException($"{nameof(SceneCollection)} '{name}' contains no valid scenes.");

			if (useLoadingScreen)
				await LoadingScreenManager.LoadScenesWithLoadingScreen(
					plan.ProviderScenePaths,
					plan.RemainingScenePaths,
					loadMode,
					loadScreenOverride?.Path
				);
			else
				await LoadWithoutLoadingScreen(plan);

			if (loadMode == LoadSceneMode.Single)
				SceneManager.SetActiveScene(plan.ActiveScenePath);

			Logger.Log(
				"Loading complete!",
				contextObject: this,
				category: LogCategory.Loading
			);
		}

		public void AddScene(Scene scene) => AddScene(
			new SceneReference(
				scene
			)
		);

		public void AddScene(SceneReference scene) =>
			sceneEntries.Add(new(scene));

		public void RemoveScene(SceneReference scene)
		{
			int index = sceneEntries.FindIndex(entry => entry.Scene == scene);
			if (index >= 0)
				sceneEntries.RemoveAt(index);
		}

		public void SetScenes(List<SceneReference> newScenes)
		{
			if (newScenes == null)
				throw new ArgumentNullException(nameof(newScenes));

			sceneEntries = newScenes
						   .Where(scene => scene != null)
						   .Select(scene => new SceneCollectionEntry(scene))
						   .ToList();
			SyncLegacyScenes();
		}

		internal void SetEntries(IEnumerable<SceneCollectionEntry> entries)
		{
			if (entries == null)
				throw new ArgumentNullException(nameof(entries));

			sceneEntries = entries.Where(entry => entry != null).ToList();
			SyncLegacyScenes();
		}

		void ISerializationCallbackReceiver.OnBeforeSerialize()
		{
			EnsureEntriesMigrated();
			SyncLegacyScenes();
		}

		void ISerializationCallbackReceiver.OnAfterDeserialize() =>
			EnsureEntriesMigrated();

		private void EnsureEntriesMigrated()
		{
			sceneEntries ??= new();
			if (sceneEntries.Count > 0 || scenes == null || scenes.Count == 0)
				return;

			sceneEntries = scenes
						   .Where(scene => scene != null)
						   .Select(scene => new SceneCollectionEntry(scene))
						   .ToList();
		}

		private async UniTask LoadWithoutLoadingScreen(SceneCollectionLoadPlan plan)
		{
			for (var i = 0; i < plan.ProviderScenePaths.Count; i++)
			{
				var mode = i == 0 ? loadMode : LoadSceneMode.Additive;
				await SceneLoader.Load(plan.ProviderScenePaths[i], mode);
			}

			if (plan.RemainingScenePaths.Count == 0)
				return;

			var remainingMode = plan.ProviderScenePaths.Count > 0
									? LoadSceneMode.Additive
									: loadMode;
			await SceneLoader.Load(plan.RemainingScenePaths, remainingMode);
		}

		private void SyncLegacyScenes()
		{
			scenes = sceneEntries
					 .Where(entry => entry?.Scene != null)
					 .Select(entry => entry.Scene!)
					 .ToList();
		}

		private void SetAllLoadingModes()
		{
			foreach (var scene in Scenes)
			{
				scene?.SetLoadSceneMode(
					loadMode
				);
			}
		}

#if UNITY_EDITOR
		// Called by the native inspector only after a user edits the corresponding fields.
		// Importing or loading an existing asset must not rewrite its scene entries.
		internal void ApplyInspectorChanges(bool sceneEntriesChanged, bool loadModeChanged)
		{
			if (sceneEntriesChanged)
				ValidateScenes();

			if (loadModeChanged)
				SetAllLoadingModes();
		}

		[ContextMenu("Open Scenes")]
		private void EditorOpenScenes()
		{
			var currentScenes = Scenes;
			if (currentScenes.Count == 0)
				return;

			switch (loadMode)
			{
				case LoadSceneMode.Single:
					if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
					{
						if (currentScenes[0] == null) return;

						EditorSceneManager.OpenScene(
							currentScenes[0].Path
						);

						for (var i = 1; i < currentScenes.Count; i++)
						{
							if (currentScenes[i] == null) continue;

							EditorSceneManager.OpenScene(
								currentScenes[i].Path,
								OpenSceneMode.Additive
							);
						}
					}

					break;

				case LoadSceneMode.Additive:
					foreach (var scene in currentScenes)
					{
						if (scene == null) continue;

						EditorSceneManager.OpenScene(
							scene.Path,
							OpenSceneMode.Additive
						);
					}

					break;

				default:
					Logger.LogError(
						$"Invalid load mode: {loadMode}"
					);

					break;
			}
		}

		[ContextMenu("Create New Scene")]
		private void CreateNewScene()
		{
			string assetPath = AssetDatabase.GetAssetPath(
				this
			);

			string? folderPath = Path.GetDirectoryName(
				assetPath
			);

			if (string.IsNullOrEmpty(folderPath))
			{
				Logger.LogError(
					$"Could not determine a folder for scene collection asset '{assetPath}'."
				);

				return;
			}

			// Generate unique scene name with _00x suffix
			var baseSceneName = "SCN_NewScene";
			string scenePath = GenerateUniqueScenePath(
				folderPath,
				baseSceneName
			);

			// Make sure the path uses forward slashes for Unity
			scenePath = scenePath.Replace(
				"\\",
				"/"
			);

			// Create new scene additively (doesn't replace current scene)
			var newScene = EditorSceneManager.NewScene(
				NewSceneSetup.EmptyScene,
				NewSceneMode.Additive
			);

			// Save the scene
			bool saved = EditorSceneManager.SaveScene(
				newScene,
				scenePath
			);

			if (saved)
			{
				Logger.Log(
					$"Successfully created new scene: {scenePath}"
				);

				// Load the scene asset from the saved path
				var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(
					scenePath
				);

				var sceneReference = new SceneReference(
					newScene,
					sceneAsset
				);

				// Close the newly created scene so it doesn't stay open
				EditorSceneManager.CloseScene(
					newScene,
					true
				);

				// Refresh the AssetDatabase to show the new scene
				AssetDatabase.Refresh();

				// Add the scene reference to the collection
				AddScene(sceneReference);

				// Mark the ScriptableObject as dirty so changes are saved
				EditorUtility.SetDirty(
					this
				);
			}
			else
			{
				Logger.LogError(
					$"Failed to save scene at {scenePath}"
				);
			}
		}

		[ContextMenu("Add to Game Settings")]
		private void AddToGameSettings()
		{
			if (GameSettings.Instance == null) return;

			var sceneCollections = GameSettings.Instance.SceneCollections.ToList();

			if (sceneCollections.Contains(
					this
				))
			{
				Logger.LogWarning(
					$"Scene collection '{name}' is already in GameSettings."
				);

				return;
			}

			sceneCollections.Add(
				this
			);

			GameSettings.Instance.SceneCollections = sceneCollections.ToArray();
		}

		[ContextMenu("Remove from Game Settings")]
		private void RemoveFromGameSettings()
		{
			if (GameSettings.Instance == null) return;

			var sceneCollections = GameSettings.Instance.SceneCollections.ToList();
			sceneCollections.Remove(
				this
			);

			GameSettings.Instance.SceneCollections = sceneCollections.ToArray();
		}

		private string GenerateUniqueScenePath(string folderPath, string baseSceneName)
		{
			string scenePath = Path.Combine(
				folderPath,
				$"{baseSceneName}_001.unity"
			);

			var counter = 1;

			while (File.Exists(
					   scenePath
				   ))
			{
				counter++;
				scenePath = Path.Combine(
					folderPath,
					$"{baseSceneName}_{counter:D3}.unity"
				);
			}

			return scenePath;
		}

		private void ValidateScenes()
		{
			var duplicates = Scenes.GroupBy(scene => scene.Path
								   )
								   .Where(g => g.Count() > 1
								   )
								   .Select(g => g.Key
								   );

			if (!duplicates.Any()) return;

			Logger.LogError(
				$"Duplicate scenes detected in {name}. Duplicates will be removed."
			);

			sceneEntries = sceneEntries
						   .GroupBy(entry => entry.Scene?.Path)
						   .Select(group => group.First())
						   .ToList();
			SyncLegacyScenes();
		}

#endif
	}
}
