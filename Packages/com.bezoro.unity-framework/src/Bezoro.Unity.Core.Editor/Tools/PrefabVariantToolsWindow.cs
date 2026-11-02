using System;
using System.IO;
using Bezoro.Core.Extensions;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Bezoro.Unity.Core.Editor.Tools
{
	/// <summary>
	///     Converts regular prefabs into variants of a selected base prefab.
	/// </summary>
	public class PrefabVariantToolsWindow : EditorWindow
	{
		public const string TITLE = "Prefab Variant Tools";

		public void ConvertOriginalToBaseVariant()
		{
			Debug.Log(
				"Starting conversion of original to base variant..."
			);

			GameObject? instance = null;

			try
			{
				ThrowIfInputsAreNull(
					originalPrefab,
					basePrefab
				);

				ThrowIfPrefabAlreadyVariant(
					originalPrefab
				);

				string originalPrefabPath = AssetDatabase.GetAssetPath(
					originalPrefab
				);

				if (string.IsNullOrEmpty(
						originalPrefabPath
					))
					throw new InvalidOperationException(
						"Failed to get the original prefab path."
					);

				// Prepare output path
				string directoryName = Path.GetDirectoryName(
					originalPrefabPath
				);

				if (!VerifyDirectoryStructure(
						directoryName
					))
					throw new InvalidOperationException(
						"Output directory structure is not valid."
					);

				// Create instance of base prefab
				instance = CreateBasePrefabInstance();

				if (instance == null)
					throw new InvalidOperationException(
						"Failed to create base prefab instance."
					);

				// Copy components from original prefab
				if (!CopyComponentsToInstance(
						instance
					))
					throw new InvalidOperationException(
						"Failed to copy components to base instance."
					);

				// Save the modified prefab instance over the original prefab
				Undo.RecordObject(
					originalPrefab,
					"Convert to Base Variant"
				);

				if (!SaveVariant(
						instance,
						originalPrefabPath,
						out var originalVariant
					))
					throw new InvalidOperationException(
						"Failed to save the converted prefab."
					);

				// Finalize the conversion
				FinalizePrefabCreation(
					originalVariant
				);

				originalPrefab = originalVariant;

				Debug.Log(
					"Original prefab successfully converted to base variant!"
				);
			}
			catch (Exception e)
			{
				HandleException(
					e
				);
			}
			finally
			{
				CleanupInstance(
					instance
				);
			}
		}

		public void CreatePrefabVariant()
		{
			Undo.SetCurrentGroupName(
				"Create Prefab Variant"
			);

			int group = Undo.GetCurrentGroup();
			Debug.Log(
				"Starting prefab variant creation process..."
			);

			GameObject? instance = null;

			try
			{
				// Validate inputs
				if (originalPrefab == null)
					throw new ArgumentNullException(
						nameof(originalPrefab),
						"Original prefab is required."
					);

				if (basePrefab == null)
					throw new ArgumentNullException(
						nameof(basePrefab),
						"Base prefab is required."
					);

				// Validate and prepare output path
				if (!ValidateInput())
				{
					Debug.LogError(
						"Input validation failed. Cannot proceed with prefab variant creation."
					);

					return;
				}

				string outputPath = PrepareOutputPath();
				Debug.Log(
					$"Using output path: {outputPath}"
				);

				// Verify directory structure and permissions
				string outputDirectory = Path.GetDirectoryName(
					outputPath
				);

				if (!VerifyDirectoryStructure(
						outputDirectory
					)) return;

				// Create instance of base prefab
				instance = CreateBasePrefabInstance();

				if (instance == null)
				{
					Debug.LogError(
						"Failed to create base prefab instance."
					);

					return;
				}

				// Copy components from original
				if (!CopyComponentsToInstance(
						instance
					))
				{
					Debug.LogError(
						"Failed to copy components from original prefab."
					);

					CleanupInstance(
						instance
					);

					return;
				}

				// Save the prefab variant
				if (!SaveVariant(
						instance,
						outputPath,
						out var variant,
						"Create Prefab Variant"
					))
				{
					Debug.LogError(
						"Failed to save prefab variant."
					);

					CleanupInstance(
						instance
					);

					return;
				}

				// Finalize the process
				FinalizePrefabCreation(
					variant
				);

				Debug.Log(
					"Prefab variant creation completed successfully!"
				);
			}
			catch (Exception e)
			{
				HandleException(
					e
				);
			}
			finally
			{
				// Ensure instance is always cleaned up
				CleanupInstance(
					instance
				);

				Undo.CollapseUndoOperations(
					group
				);
			}
		}

		private void OnGUI()
		{
			DrawPrefabVariantCreationSteps();
			DrawPrefabFields();
			DrawOutputFields();
			DrawButtons();

			GUI.enabled = true;
		}

		private static bool InputsAreNull(params GameObject[] gos) =>
			gos == null ||
			gos.Length == 0 ||
			Array.Exists(
				gos,
				prefab => prefab == null
			);

		private static bool IsPrefabVariant(GameObject prefab)
		{
			if (prefab == null) return false;

			return PrefabUtility.GetPrefabAssetType(
					   prefab
				   ) ==
				   PrefabAssetType.Variant;
		}

		private static void DrawPrefabVariantCreationSteps()
		{
			EditorGUILayout.Space();

			EditorGUILayout.HelpBox(
				"1. Select the original prefab you want to turn into a variant\n" +
				"2. Select the base prefab that will be the parent of the variant\n" +
				"3. (Optional) Choose an output folder and custom name\n" +
				"4. Click 'Create Prefab Variant' to generate the variant",
				MessageType.Info
			);
		}

		private static void HandleException(Exception e)
		{
			Debug.LogError(
				$"Error creating prefab variant: {e.Message}\n{e.StackTrace}"
			);

			EditorUtility.DisplayDialog(
				"Error",
				"An error occurred while creating the prefab variant. Check the console for details.",
				"OK"
			);
		}

		[MenuItem(
			"Bezoro/Tools/" + TITLE
		)]
		private static void OpenWindow() =>
			GetWindow<PrefabVariantToolsWindow>(
				TITLE
			).Show();

		private bool CopyComponentsToInstance(GameObject instance)
		{
			Debug.Log(
				"Copying components from original prefab..."
			);

			GameObject? originalInstance = null;

			try
			{
				originalInstance = (GameObject)PrefabUtility.InstantiatePrefab(
					originalPrefab
				);

				if (originalInstance == null)
				{
					Debug.LogError(
						"Failed to instantiate original prefab for component copying"
					);

					return false;
				}

				Undo.RegisterCreatedObjectUndo(
					originalInstance,
					"Create Original Instance For Copying"
				);

				var components = originalInstance.GetComponents<Component>();

				foreach (var component in components)
				{
					if (component != null)
					{
						ComponentUtility.CopyComponent(
							component
						);

						ComponentUtility.PasteComponentAsNew(
							instance
						);
					}
				}

				Debug.Log(
					$"Successfully copied {components.Length} components"
				);

				return true;
			}
			catch (Exception ex)
			{
				Debug.LogError(
					$"Error copying components: {ex.Message}"
				);

				return false;
			}
			finally
			{
				if (originalInstance != null)
					Undo.DestroyObjectImmediate(
						originalInstance
					);
			}
		}

		private bool SaveVariant(
			GameObject     instance,
			string         outputPath,
			out GameObject variant,
			string?        undoMessage = null)
		{
			variant = null!;
			Debug.Log(
				$"Attempting to save prefab variant at: {outputPath}"
			);

			try
			{
				// Ensure consistent path separators and get directory
				outputPath = outputPath.Replace(
					"\\",
					"/"
				);

				string? directory = Path.GetDirectoryName(
					outputPath
				);

				// Double-check directory exists and is writable
				if (string.IsNullOrEmpty(directory))
					throw new InvalidOperationException(
						"Failed to resolve the prefab output directory."
					);

				if (!Directory.Exists(
						directory
					))
					Directory.CreateDirectory(
						directory
					);

				// Force Unity to release any locks on the asset database
				EditorUtility.UnloadUnusedAssetsImmediate();
				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();

				// If a file already exists at the path, delete it first
				if (File.Exists(
						outputPath
					))
				{
					Debug.Log(
						$"Removing existing file at {outputPath}"
					);

					AssetDatabase.DeleteAsset(
						outputPath
					);

					AssetDatabase.Refresh();
				}

				// Set up the base prefab connection before saving
				Debug.Log(
					"Setting up prefab variant connection..."
				);

				PrefabUtility.SetPropertyModifications(
					instance,
					new PropertyModification[0]
				);

				PrefabUtility.RecordPrefabInstancePropertyModifications(
					instance
				);

				// Important: Link the instance to the base prefab
				var baseSource = PrefabUtility.GetCorrespondingObjectFromSource(
					basePrefab
				);

				if (baseSource == null) baseSource = basePrefab;

				PrefabUtility.SaveAsPrefabAssetAndConnect(
					instance,
					outputPath,
					InteractionMode.AutomatedAction
				);

				// Load the created variant
				variant = AssetDatabase.LoadAssetAtPath<GameObject>(
					outputPath
				);

				if (variant == null)
				{
					Debug.LogError(
						$"Failed to save prefab variant at path: {outputPath}"
					);

					return false;
				}

				if (!string.IsNullOrEmpty(
						undoMessage
					))
					Undo.RegisterCreatedObjectUndo(
						variant,
						undoMessage
					);

				// Verify the variant relationship
				var prefabType = PrefabUtility.GetPrefabAssetType(
					variant
				);

				var baseObject = PrefabUtility.GetCorrespondingObjectFromSource(
					variant
				);

				if (baseObject == null || prefabType != PrefabAssetType.Variant)
				{
					Debug.LogError(
						"Created prefab is not a proper variant!"
					);

					AssetDatabase.DeleteAsset(
						outputPath
					);

					return false;
				}

				// Ensure the variant is properly saved and registered
				EditorUtility.SetDirty(
					variant
				);

				PrefabUtility.SavePrefabAsset(
					variant
				);

				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();

				Debug.Log(
					$"Successfully saved prefab variant to: {outputPath}"
				);

				return true;
			}
			catch (Exception ex)
			{
				Debug.LogError(
					$"Error saving prefab variant: {ex.Message}\nStack trace: {ex.StackTrace}"
				);

				return false;
			}
		}

		private bool ValidateInput()
		{
			if (originalPrefab == null || basePrefab == null)
			{
				EditorUtility.DisplayDialog(
					"Error",
					"Please assign both prefabs before creating a variant.",
					"OK"
				);

				return false;
			}

			if (string.IsNullOrEmpty(
					outputFolder
				))
			{
				outputFolder = Path.GetDirectoryName(
					AssetDatabase.GetAssetPath(
						originalPrefab
					)
				);
			}
			else if (!Directory.Exists(
						 outputFolder
					 ))
			{
				EditorUtility.DisplayDialog(
					"Error",
					"Output folder does not exist!",
					"OK"
				);

				return false;
			}

			return true;
		}

		private bool VerifyDirectoryStructure(string? directory)
		{
			try
			{
				if (string.IsNullOrEmpty(directory))
					throw new InvalidOperationException(
						"Output directory is empty."
					);

				if (!Directory.Exists(
						directory
					))
				{
					Directory.CreateDirectory(
						directory
					);

					Debug.Log(
						$"Created directory: {directory}"
					);
				}

				// Verify write permissions with a test file
				string testPath = Path.Combine(
					directory,
					$"test_{Guid.NewGuid()}.tmp"
				);

				try
				{
					using (File.Create(
							   testPath,
							   1,
							   FileOptions.DeleteOnClose
						   )) { }

					return true;
				}
				catch (Exception ex)
				{
					Debug.LogError(
						$"Failed to verify write permissions: {ex.Message}"
					);

					EditorUtility.DisplayDialog(
						"Permission Error",
						"Cannot write to the output directory. Please check folder permissions.",
						"OK"
					);

					return false;
				}
			}
			catch (Exception ex)
			{
				Debug.LogError(
					$"Failed to verify/create directory structure: {ex.Message}"
				);

				EditorUtility.DisplayDialog(
					"Directory Error",
					"Failed to verify or create the output directory structure.",
					"OK"
				);

				return false;
			}
		}

		private GameObject? CreateBasePrefabInstance()
		{
			Debug.Log(
				"Creating base prefab instance..."
			);

			// Create instance and ensure it's connected to the base prefab
			var instance = (GameObject)PrefabUtility.InstantiatePrefab(
				basePrefab
			);

			if (instance != null)
			{
				instance.name = $"{instance.name}_Variant";

				// Ensure the instance is properly set up as a variant candidate
				PrefabUtility.SetPropertyModifications(
					instance,
					new PropertyModification[0]
				);

				PrefabUtility.RecordPrefabInstancePropertyModifications(
					instance
				);

				// Register for undo
				Undo.RegisterCreatedObjectUndo(
					instance,
					"Create Prefab Variant Instance"
				);

				// Verify the connection to the base prefab
				var baseObject = PrefabUtility.GetCorrespondingObjectFromSource(
					instance
				);

				if (baseObject == basePrefab)
					Debug.Log(
						"Base prefab instance created successfully with proper variant setup"
					);
				else
					Debug.LogWarning(
						"Base prefab connection might not be properly set up"
					);
			}

			return instance!;
		}

		private string PrepareOutputPath()
		{
			string finalVariantName =
				string.IsNullOrEmpty(
					variantName
				)
					? originalPrefab.name + "_Variant"
					: variantName;

			return Path.Combine(
				outputFolder,
				finalVariantName + ".prefab"
			).Replace(
				'\\',
				'/'
			);
		}

		private void CleanupInstance(GameObject? instance)
		{
			if (instance != null)
			{
				Undo.DestroyObjectImmediate(
					instance
				);

				Debug.Log(
					"Cleaned up temporary instance"
				);
			}
		}

		private void DrawButtons()
		{
			EditorGUILayout.Space();
			GUI.enabled = originalPrefab != null && basePrefab != null;

			if (GUILayout.Button(
					"Create Prefab Variant",
					GUILayout.Height(
						30
					)
				))
				CreatePrefabVariant();

			GUI.enabled = true;

			EditorGUILayout.Space();

			GUI.enabled = originalPrefab != null &&
						  basePrefab != null &&
						  !IsPrefabVariant(
							  originalPrefab
						  );

			if (GUILayout.Button(
					"Convert to Base Variant",
					GUILayout.Height(
						30
					)
				))
				ConvertOriginalToBaseVariant();
		}

		private void DrawInstructions()
		{
			EditorGUILayout.Space(
				10
			);

			EditorGUILayout.HelpBox(
				"1. Select the original prefab you want to turn into a variant\n" +
				"2. Select the base prefab that will be the parent of the variant\n" +
				"3. (Optional) Choose an output folder and custom name\n" +
				"4. Click 'Create Prefab Variant' to generate the variant",
				MessageType.Info
			);
		}

		private void DrawOutputFields()
		{
			EditorGUILayout.BeginVertical(
				"box"
			);

			EditorGUILayout.LabelField(
				"Output Settings".Underline(),
				new GUIStyle(
					EditorStyles.boldLabel
				) { richText = true }
			);

			outputFolder = EditorGUILayout.TextField(
				new GUIContent(
					"Output Folder",
					"Select where to save the new prefab variant"
				),
				outputFolder
			);

			variantName = EditorGUILayout.TextField(
				new GUIContent(
					"Variant Name",
					"Name for the new prefab variant (leave empty to use original name + '_Variant')"
				),
				variantName
			);

			EditorGUILayout.EndVertical();
		}

		private void DrawPrefabFields()
		{
			EditorGUILayout.Space();
			EditorGUILayout.BeginVertical(
				"box"
			);

			if (IsPrefabVariant(
					originalPrefab
				))
				EditorGUILayout.HelpBox(
					"The selected original prefab is already a variant!",
					MessageType.Warning
				);

			EditorGUILayout.LabelField(
				"Source Prefabs".Underline(),
				new GUIStyle(
					EditorStyles.boldLabel
				) { richText = true }
			);

			originalPrefab = (GameObject)EditorGUILayout.ObjectField(
				new GUIContent(
					"Original Prefab",
					"Select the prefab you want to turn into a variant"
				),
				originalPrefab,
				typeof(GameObject),
				false
			);

			basePrefab = (GameObject)EditorGUILayout.ObjectField(
				new GUIContent(
					"Base Prefab",
					"Select the prefab that will be the base for the variant"
				),
				basePrefab,
				typeof(GameObject),
				false
			);

			EditorGUILayout.EndVertical();
		}

		private void FinalizePrefabCreation(GameObject variant)
		{
			// Select and highlight the new variant
			Selection.activeObject = variant;
			EditorGUIUtility.PingObject(
				variant
			);

			// Show success message
			EditorUtility.DisplayDialog(
				"Success",
				$"Prefab variant '{variant.name}' created successfully!",
				"OK"
			);
		}

		private void ThrowIfInputsAreNull(params GameObject[] gos)
		{
			if (InputsAreNull(
					gos
				))
				throw new ArgumentNullException(
					nameof(gos),
					"All prefabs must be assigned before conversion."
				);
		}

		private void ThrowIfPrefabAlreadyVariant(GameObject prefab)
		{
			if (IsPrefabVariant(
					prefab
				))
				throw new InvalidOperationException(
					"The original prefab is already a variant."
				);
		}

		#region Serialized Fields

		public GameObject originalPrefab = null!;
		public GameObject basePrefab     = null!;
		public string     outputFolder   = null!;
		public string     variantName    = null!;

		#endregion
	}
}
