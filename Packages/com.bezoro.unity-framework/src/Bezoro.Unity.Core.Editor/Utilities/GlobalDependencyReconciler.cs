using System;
using System.Linq;
using Bezoro.Unity.Core.Editor.Utilities;
using Bezoro.Unity.Core.ScriptableObjects;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Bezoro.Unity.Core.Editor.Dependencies
{
	public static class GlobalDependencyReconcilerModel
	{
		public static bool CanMutateScene(
			bool applicationIsPlaying,
			bool editorIsPlaying,
			bool editorIsPlayingOrWillChangePlaymode) =>
			!applicationIsPlaying &&
			!editorIsPlaying &&
			!editorIsPlayingOrWillChangePlaymode;
	}

	[InitializeOnLoad]
	public static class GlobalDependencyReconciler
	{
		private static bool _isReconciling;
		private static bool _isScheduled;
		private static bool _isUndoRedoInProgress;

		static GlobalDependencyReconciler()
		{
			EditorApplication.hierarchyChanged += Schedule;
			EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
			EditorSceneManager.sceneOpened += (_, _) => Schedule();
			EditorSceneManager.sceneSaved += _ => Schedule();
			Undo.undoRedoPerformed += OnUndoRedoPerformed;
		}

		[MenuItem("Bezoro/Runtime/Reconcile Global Dependencies")]
		public static void ReconcileLoadedScenes()
		{
			if (!CanMutate())
				return;

			_isReconciling = true;
			try
			{
				var collections = FindAssets<SceneCollection>();
				var config      = FindInitializationConfig();

				for (var i = 0; i < SceneManager.sceneCount; i++)
				{
					var scene = SceneManager.GetSceneAt(i);
					if (!scene.IsValid() || !scene.isLoaded || string.IsNullOrEmpty(scene.path))
						continue;

					var result = GlobalDependencyProvisioner.ReconcileScene(
						scene,
						collections,
						config
					);

					foreach (var diagnostic in result.Diagnostics)
					{
						if (diagnostic.Severity == GlobalDependencyDiagnosticSeverity.Error)
							Debug.LogError(diagnostic.Message, diagnostic.Context);
						else
							Debug.LogWarning(diagnostic.Message, diagnostic.Context);
					}
				}
			}
			finally
			{
				_isReconciling = false;
			}
		}

		[MenuItem("Bezoro/Runtime/Migrate SceneCollection Entries")]
		public static void MigrateSceneCollections()
		{
			if (!CanMutate())
				return;

			foreach (var collection in FindAssets<SceneCollection>())
			{
				Undo.RecordObject(collection, "Migrate SceneCollection Entries");
				((ISerializationCallbackReceiver)collection).OnAfterDeserialize();
				EditorUtility.SetDirty(collection);
			}

			AssetDatabase.SaveAssets();
		}

		private static bool CanMutate() =>
			!_isReconciling &&
			!InternalEditorUtility.inBatchMode &&
			GlobalDependencyReconcilerModel.CanMutateScene(
				UnityEngine.Application.isPlaying,
				EditorApplication.isPlaying,
				EditorApplication.isPlayingOrWillChangePlaymode
			) &&
			!EditorApplication.isCompiling &&
			!EditorApplication.isUpdating &&
			!BuildPipeline.isBuildingPlayer &&
			!BuildEventHandler.IsBuildInProgress &&
			!GlobalDependencyBuildValidator.IsValidating &&
			!AssetDatabase.IsAssetImportWorkerProcess() &&
			PrefabStageUtility.GetCurrentPrefabStage() == null;

		private static T[] FindAssets<T>()
			where T : UnityEngine.Object =>
			AssetDatabase.FindAssets($"t:{typeof(T).Name}")
						 .Select(AssetDatabase.GUIDToAssetPath)
						 .Select(AssetDatabase.LoadAssetAtPath<T>)
						 .Where(asset => asset != null)
						 .ToArray();

		private static GameInitializationConfig? FindInitializationConfig()
		{
			var settings = GameSettings.Instance;
			if (settings?.InitializationConfig != null)
				return settings.InitializationConfig;

			return FindAssets<GameInitializationConfig>().FirstOrDefault();
		}

		private static void Schedule()
		{
			if (_isScheduled ||
				_isReconciling ||
				_isUndoRedoInProgress ||
				!GlobalDependencyReconcilerModel.CanMutateScene(
					UnityEngine.Application.isPlaying,
					EditorApplication.isPlaying,
					EditorApplication.isPlayingOrWillChangePlaymode
				))
				return;

			_isScheduled = true;
			EditorApplication.delayCall += RunScheduled;
		}

		private static void OnUndoRedoPerformed()
		{
			CancelScheduled();
			_isUndoRedoInProgress = true;
			EditorApplication.delayCall -= EndUndoRedoSuppression;
			EditorApplication.delayCall += EndUndoRedoSuppression;
		}

		private static void EndUndoRedoSuppression() =>
			_isUndoRedoInProgress = false;

		private static void OnPlayModeStateChanged(PlayModeStateChange state)
		{
			if (state == PlayModeStateChange.EnteredEditMode)
			{
				Schedule();
				return;
			}

			CancelScheduled();
		}

		private static void CancelScheduled()
		{
			if (!_isScheduled)
				return;

			EditorApplication.delayCall -= RunScheduled;
			_isScheduled = false;
		}

		private static void RunScheduled()
		{
			_isScheduled = false;

			// A delay-call invocation may already have captured this callback before Undo canceled it.
			if (_isUndoRedoInProgress || !CanMutate())
				return;

			ReconcileLoadedScenes();
		}
	}
}
