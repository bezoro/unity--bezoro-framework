using System;
using System.Collections.Generic;
using System.Linq;
using Bezoro.Unity.Core.Editor.Common.Helpers;
using Bezoro.Unity.Core.Scenes;
using Bezoro.Unity.Core.ScriptableObjects;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

namespace Bezoro.Unity.Core.Editor.EditorWindows
{
	/// <summary>
	///     Edits shared runtime GameSettings assets from a UI Toolkit window.
	/// </summary>
	public class GameSettingsWindow : EditorWindow
	{
		private bool    _playFromFirstBuildScene;
		private Camera? _defaultMainCamera;

		private GameSettings? _settings;

		private VolumeProfile?            _defaultGlobalProfile;
		public event Action?              AnySettingsChanged;
		public static GameSettingsWindow? Instance { get; private set; }

		#region Unity Lifecycle

		private void OnDestroy()
		{
			if (Instance == this) Instance = null;
		}

		private void OnBecameVisible()
		{
			Instance = this;
			Validate_Settings();
			Load_Settings();
			Create_UI();
		}

		#endregion

		[MenuItem(
			"Bezoro/Runtime/Game Settings"
		)]
		public static void Show_Window() =>
			Get_Window();

		public void Load_Settings()
		{
			if (_settings == null)
				return;

			_playFromFirstBuildScene = _settings.EmulateBuild;
			_defaultMainCamera       = _settings.MainCamera;
			_defaultGlobalProfile    = _settings.DefaultGlobalProfile;
		}

		private static VisualElement CreateSeparatorElement()
		{
			var separator = new VisualElement
			{
				style =
				{
					height          = 1,
					backgroundColor = Color.gray,
					flexGrow        = 1,
					marginTop       = 5,
					marginBottom    = 2
				}
			};

			return separator;
		}

		private static void Get_Window()
		{
			var window = GetWindow<GameSettingsWindow>(
				"Game Settings"
			);

			window.minSize = new(
				Length.Auto().value,
				Length.Auto().value
			);
		}

		private bool CheckIfBuildScenesMatch()
		{
			Debug.Log(
				"Called"
			);

			if (_settings == null)
				return false;

			var currentScenes = (_settings.SceneCollections ?? Array.Empty<SceneCollection>())
								.SelectMany(sc => (IEnumerable<SceneReference>?)sc.Scenes ??
												  Enumerable.Empty<SceneReference>()
								)
								.Where(scene => scene != null)
								.ToArray();

			string[] buildScenes = EditorBuildSettings.scenes.Select(bs => bs.path
			).ToArray();

			return currentScenes.Length == buildScenes.Length &&
				   !currentScenes.Where((scene, index) => scene.Path != buildScenes[index]
				   ).Any();
		}

		private VisualElement Initialize_Root_Element()
		{
			var root = new VisualElement
			{
				style = { paddingLeft = 10, paddingRight = 10, paddingTop = 10, paddingBottom = 10 }
			};

			rootVisualElement.Add(
				root
			);

			return root;
		}

		private void Add_Emulate_Build_Toggle(VisualElement root)
		{
			var emulateBuildToggle = new Toggle(
				"Emulate Build"
			)
			{
				value = _playFromFirstBuildScene
			};

			emulateBuildToggle.RegisterValueChangedCallback(evt =>
				{
					_playFromFirstBuildScene = evt.newValue;
					OnSettingsChanged();
				}
			);

			root.Add(
				emulateBuildToggle
			);
		}

		private void Add_Fundamental_Services_Label(VisualElement root)
		{
			var container = new VisualElement
			{
				style =
				{
					backgroundColor = new Color(
						0.18f,
						0.18f,
						0.18f,
						1f
					),
					paddingTop = 10,

					marginBottom = 5,

					borderTopLeftRadius  = 5,
					borderTopRightRadius = 5
				}
			};

			var fundamentalServicesLabel = new Label(
				"Fundamental Services"
			)
			{
				style =
				{
					alignSelf = Align.Center,
					fontSize  = 14, unityFontStyleAndWeight = FontStyle.Bold
				}
			};

			var separator = CreateSeparatorElement();

			container.Add(
				fundamentalServicesLabel
			);

			container.Add(
				separator
			);

			root.Add(
				container
			);
		}

		private void Add_Game_Initializer_Field(VisualElement root)
		{
			var container = new VisualElement
			{
				tooltip =
					"Asset used to initialize the game's core systems, such as the DI and Event Bus Systems during Unity's various initialization phases.",
				style =
				{
					paddingTop    = 10, paddingBottom = 10,
					flexDirection = FlexDirection.Row
				}
			};

			var gameInitializerField = new ObjectField(
				"Game Initializer"
			)
			{
				objectType = typeof(GameInitializationConfig),
				style      = { flexGrow = 1 }
			};

			var inspectButton = new Button(() =>
				{
					InspectorWindowUtilities.Open_Inspector_Window(
						gameInitializerField,
						InspectorWindowType.Auxiliary
					);
				}
			)
			{
				text = "Inspect",
				style =
				{
					marginLeft = 5
				}
			};

			container.Add(
				gameInitializerField
			);

			container.Add(
				inspectButton
			);

			root.Add(
				container
			);
		}

		private void Add_Game_Settings_Field(VisualElement root)
		{
			var container = new VisualElement
			{
				style =
				{
					flexDirection = FlexDirection.Row,
					paddingTop    = 5,
					paddingBottom = 5
				}
			};

			var gameSettingsField = new ObjectField(
				"Game Settings"
			)
			{
				value       = _settings,
				objectType  = typeof(GameSettings),
				style       = { flexGrow = 1 },
				enabledSelf = false
			};

			var inspectButton = new Button(() =>
				{
					InspectorWindowUtilities.Open_Inspector_Window(
						gameSettingsField,
						InspectorWindowType.Auxiliary
					);
				}
			)
			{
				text = "Inspect",
				style =
				{
					marginLeft = 5
				}
			};

			container.Add(
				gameSettingsField
			);

			container.Add(
				inspectButton
			);

			root.Add(
				container
			);
		}

		private void Add_Gameplay_Label(VisualElement root)
		{
			var container = new VisualElement
			{
				style =
				{
					backgroundColor = new Color(
						0.18f,
						0.18f,
						0.18f,
						1f
					),
					paddingTop = 10,

					marginBottom = 5,

					borderTopLeftRadius  = 5,
					borderTopRightRadius = 5
				}
			};

			var gameplayLabel = new Label(
				"Gameplay"
			)
			{
				style =
				{
					alignSelf = Align.Center,
					fontSize  = 14, unityFontStyleAndWeight = FontStyle.Bold
				}
			};

			var separator = CreateSeparatorElement();
			container.Add(
				gameplayLabel
			);

			container.Add(
				separator
			);

			root.Add(
				container
			);
		}

		private void Add_Load_Settings_Button(VisualElement root)
		{
			var loadSettingsButton = new Button(
				Load_Settings
			)
			{
				text = "Load Settings"
			};

			root.Add(
				loadSettingsButton
			);
		}

		private void Add_Main_Camera_Field(VisualElement root)
		{
			var container = new VisualElement
			{
				style =
				{
					paddingBottom = 10
				}
			};

			var cameraField = new ObjectField(
				"Main Camera"
			)
			{
				objectType = typeof(Camera),
				value      = _defaultMainCamera
			};

			cameraField.RegisterValueChangedCallback(evt =>
				{
					_defaultMainCamera = evt.newValue as Camera;
					OnSettingsChanged();
				}
			);

			container.Add(
				cameraField
			);

			root.Add(
				container
			);
		}

		private void Add_Scene_Collections_Field(VisualElement root)
		{
			if (_settings == null)
				return;

			var container = new VisualElement
			{
				style =
				{
					paddingBottom = 10
				}
			};

			//TODO: Turn into a reusable method for creating property fields more easily
			var sceneCollectionsField = new PropertyField
			{
				label       = "Scene Collections",
				bindingPath = GameSettings.SceneCollectionsName,
				style =
				{
					fontSize                = 14,
					unityFontStyleAndWeight = FontStyle.Bold,
					flexGrow                = 1
				}
			};

			var serializedObject = new SerializedObject(
				_settings
			);

			sceneCollectionsField.Bind(
				serializedObject
			);

			var sceneCollectionsProperty =
				serializedObject.FindProperty(
					GameSettings.SceneCollectionsName
				);

			int previousArraySize = sceneCollectionsProperty.arraySize;

			sceneCollectionsField.TrackPropertyValue(
				sceneCollectionsProperty,
				property =>
				{
					// Check if array size increased (new element added)
					if (property.arraySize > previousArraySize)
					{
						// Clear the newly added element only if it's the same as the one above it
						var newElement = property.GetArrayElementAtIndex(
							property.arraySize - 1
						);

						// Check if there's a previous element to compare with
						if (property.arraySize > 1)
						{
							var previousElement = property.GetArrayElementAtIndex(
								property.arraySize - 2
							);

							// Only clear if the new element has the same value as the previous one
							if (newElement.objectReferenceValue == previousElement.objectReferenceValue &&
								newElement.objectReferenceValue != null)
							{
								newElement.objectReferenceValue = default;
								serializedObject.ApplyModifiedProperties();
							}
						}
						else
						{
							// If this is the first element and it has a value, clear it
							if (newElement.objectReferenceValue != null)
							{
								newElement.objectReferenceValue = default;
								serializedObject.ApplyModifiedProperties();
							}
						}
					}

					previousArraySize = property.arraySize;
					_settings.OverwriteBuildScenes();
				}
			);

			container.Add(
				sceneCollectionsField
			);

			AddOverwriteBuildScenesButton(
				container
			);

			root.Add(
				container
			);
		}

		private void Add_Scene_Collections_Label(VisualElement root)
		{
			var container = new VisualElement
			{
				style =
				{
					backgroundColor = new Color(
						0.18f,
						0.18f,
						0.18f,
						1f
					),
					paddingTop = 10,

					marginBottom = 5,

					borderTopLeftRadius  = 5,
					borderTopRightRadius = 5
				}
			};

			var sceneCollectionsLabel = new Label(
				"Scenes"
			)
			{
				style =
				{
					alignSelf = Align.Center,
					fontSize  = 14, unityFontStyleAndWeight = FontStyle.Bold
				}
			};

			var separator = CreateSeparatorElement();
			container.Add(
				sceneCollectionsLabel
			);

			container.Add(
				separator
			);

			root.Add(
				container
			);
		}

		private void AddOverwriteBuildScenesButton(VisualElement root)
		{
			if (_settings == null)
				return;

			var overwriteBuildScenesButton = new Button(
				_settings.OverwriteBuildScenes
			)
			{
				text = "Overwrite Build Scenes"
			};

			root.Add(
				overwriteBuildScenesButton
			);
		}

		private void Create_UI()
		{
			rootVisualElement.Clear();

			if (_settings == null)
				return;

			var root = Initialize_Root_Element();

			Add_Game_Settings_Field(
				root
			);

			Add_Fundamental_Services_Label(
				root
			);

			Add_Game_Initializer_Field(
				root
			);

			// Add_TickManager_Field( root );
			// Add_AI_Manager_Field( root );
			// Add_Audio_Manager_Field( root );
			// Add_Input_Manager_Field( root );
			// Add_UI_Manager_Field( root );
			// Add_Scene_Manager_Field( root );

			Add_Scene_Collections_Label(
				root
			);

			Add_Scene_Collections_Field(
				root
			);

			Add_Gameplay_Label(
				root
			);

			Add_Main_Camera_Field(
				root
			);

			// TODO: Move to another window
			Add_Emulate_Build_Toggle(
				root
			);

			Add_Load_Settings_Button(
				root
			);
		}

		private void OnSettingsChanged()
		{
			if (_settings == null)
			{
				Debug.LogError(
					$"{nameof(GameSettingsWindow)} cannot save changes because no Game Settings asset is loaded."
				);

				return;
			}

			Undo.RecordObject(_settings, "Game Settings Changed");

			_settings.EmulateBuild         = _playFromFirstBuildScene;
			_settings.MainCamera           = _defaultMainCamera;
			_settings.DefaultGlobalProfile = _defaultGlobalProfile;

			EditorUtility.SetDirty(_settings);
			AssetDatabase.SaveAssets();
			AnySettingsChanged?.Invoke();
		}

		private void Validate_Settings()
		{
			if (GameSettings.Instance == null)
			{
				EditorUtility.DisplayDialog(
					"Warning",
					"GameSettings instance is null. Please ensure you have created a GameSettings asset.",
					"OK"
				);

				return;
			}

			_settings = GameSettings.Instance;
		}
	}
}
