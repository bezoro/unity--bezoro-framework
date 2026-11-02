using System;
using System.Collections.Generic;
using System.Linq;
using Bezoro.Unity.Core.Common.Enums;
using Bezoro.Unity.Core.Scenes;
using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Bezoro.Unity.Core.ScriptableObjects
{
	/// <summary>
	///     Stores global runtime settings and dispatches initialization phases.
	/// </summary>
	[DefaultExecutionOrder(ExecutionOrder.Fundamental)]
	[CreateAssetMenu(
		fileName = "GameSettings",
		menuName = "Bezoro/Runtime/Game Settings",
		order = -1
	)]
	public sealed class GameSettings : ScriptableSingleton<GameSettings>
	{
		[SerializeField] private GameInitializationConfig? gameInitializationConfig;

		[Tooltip("Scenes to add to the build")]
		[SerializeField] private SceneCollection[]? sceneCollections;

		[Tooltip("Default Main Camera")]
		[SerializeField] private Camera? mainCamera;

		[Tooltip("Default Loading Screen")]
		[SerializeField] private SceneReference? loadingScreenReference;

		[Tooltip("Default Post-Processing Profile")]
		[SerializeField] private VolumeProfile? defaultGlobalProfile;

		[Tooltip("Forces starting scenes to be loaded in editor mode")]
		private bool _emulateBuild;

		public static event Action? AnySettingsChanged;

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
		private static void Initialize_AfterAssembliesLoaded() =>
			Instance?.gameInitializationConfig?.AfterAssemblyLoadPhase();

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
		private static void Initialize_AfterSceneLoad() =>
			Instance?.gameInitializationConfig?.AfterSceneLoadPhase();

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void Initialize_BeforeSceneLoad() =>
			Instance?.gameInitializationConfig?.BeforeSceneLoadPhase();

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
		private static void Initialize_BeforeSplashScreen() =>
			Instance?.gameInitializationConfig?.BeforeSplashScreenPhase();

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void Initialize_Subsystems() =>
			Instance?.gameInitializationConfig?.SubsystemRegistrationPhase();

		public static string SceneCollectionsName => nameof(sceneCollections);

		public GameInitializationConfig? InitializationConfig => gameInitializationConfig;

		[Tooltip("Forces starting scenes to be loaded in editor mode")]
		public bool EmulateBuild
		{
#if UNITY_EDITOR
			get
			{
				bool value = EditorPrefs.GetBool(
					EMULATE_BUILD_KEY,
					false
				);

				_emulateBuild = value;
				return value;
			}
			set
			{
				EditorPrefs.SetBool(
					EMULATE_BUILD_KEY,
					value
				);

				_emulateBuild = value;
				NotifySettingsChanged();
			}
#else
			get => false;
			set { }
#endif
		}


		public Camera? MainCamera
		{
			get => mainCamera;
			set
			{
				mainCamera = value;
				NotifySettingsChanged();
			}
		}

		public SceneCollection[]? SceneCollections
		{
			get => sceneCollections;
			set
			{
				sceneCollections = value?.Distinct().ToArray() ?? Array.Empty<SceneCollection>();
#if UNITY_EDITOR
				OverwriteBuildScenes();
#endif
				NotifySettingsChanged();
			}
		}

		public SceneReference? LoadingScreen
		{
			get => loadingScreenReference;
			set
			{
				loadingScreenReference = value;
				NotifySettingsChanged();
			}
		}

		public VolumeProfile? DefaultGlobalProfile
		{
			get => defaultGlobalProfile;
			set
			{
				defaultGlobalProfile = value;
				NotifySettingsChanged();
			}
		}

		#region Unity Lifecycle

		protected override void OnValidate()
		{
			base.OnValidate();
			NotifySettingsChanged();
		}

		#endregion

		private static void NotifySettingsChanged() =>
			AnySettingsChanged?.Invoke();
#if UNITY_EDITOR
		private const string EMULATE_BUILD_KEY = "GameSettings.EmulateBuild";

		public void OverwriteBuildScenes()
		{
			var buildScenes = new List<EditorBuildSettingsScene>();
			buildScenes.Clear();

			if (SceneCollections != null)
				foreach (var collection in SceneCollections)
				{
					if (collection == null) continue;
					if (collection.Scenes == null) continue;

					foreach (var scene in collection.Scenes)
					{
						if (scene == null) continue;

						buildScenes.Add(
							new(
								scene.Path,
								true
							)
						);
					}
				}

			EditorBuildSettings.scenes = buildScenes.ToArray();
		}
#endif
	}
}
