using System;
using System.IO;
using Bezoro.Core.Extensions;
using Bezoro.Core.Utilities;
using Bezoro.Unity.Core.Common.Helpers;
using Bezoro.Unity.Core.Editor.Utilities;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.PackageManager;
using UnityEngine;

namespace Bezoro.Unity.Core.Editor.Tools
{
	/// <summary>
	///     Adds Bezoro menu actions for initial Unity project setup.
	/// </summary>
	public static class ProjectPreparer
	{
		/// <summary>
		///     Creates the default project directory structure under Assets.
		/// </summary>
		private static class DirectoryCreator
		{
			[MenuItem(
				Constants.BEZORO + "/Project Preparer/Create Default Directories",
				false,
				20
			)]
			public static void CreateDefaultDirectories_MenuItem()
			{
				string   root            = UnityEngine.Application.dataPath;
				string[] foldersToCreate = { Constants.PROJECT_FOLDER_NAME };

				foreach (string folder in foldersToCreate)
				{
					if (Directory.Exists(
							Path.Combine(
								root,
								folder
							)
						))
					{
						Debug.LogWarning(
							$"Directory {folder.Bold()} already exists at {root.Italic()}."
						);

						continue;
					}

					Directory.CreateDirectory(
						Path.Combine(
							root,
							folder
						)
					);

					Debug.Log(
						$"Directory {folder.Bold()} created at {root.Italic()}."
					);
				}

				AssetDatabase.Refresh();
			}
		}

		/// <summary>
		///     Switches Unity's active input handling to support both input systems.
		/// </summary>
		private static class InputSystemSettingsSetter
		{
			[MenuItem(
				Constants.BEZORO + "/Project Preparer/Set Input System (Both)",
				false,
				80
			)]
			private static void Set_Input_System_To_Both()
			{
				int       activeInputSystem = InputSystemHelpers.Get_Active_Input_System_Property().intValue;
				const int BOTH_INPUT_SYSTEM = (int)InputSystemHelpers.InputSystem.Both;

				if (activeInputSystem == BOTH_INPUT_SYSTEM)
				{
					EditorUtility.DisplayDialog(
						"",
						"It's already set",
						"Ok"
					);

					return;
				}

				InputSystemHelpers.Set_Active_Input_System(
					InputSystemHelpers.InputSystem.Both
				);

				bool userConfirmedRestart = EditorUtility.DisplayDialog(
					"",
					"Changing the Input System needs to restart the editor",
					"Ok"
				);

				if (userConfirmedRestart) EditorHelpers.Reopen_Project();
			}
		}

		/// <summary>
		///     Replaces Packages/manifest.json with the shared baseline manifest.
		/// </summary>
		private static class PackagesManifestOverwritter
		{
			// [MenuItem("Bezoro/Project Preparer/Overwrite 'manifest.json' from Gist. (2021 LTS Base)",
			// 		  false,
			// 		  60)]
			public static async void OverwriteManifest_MenuItem()
			{
				try
				{
					string gistURL = GistHelpers.Get_URL(
						"ebccb4c31b1bb9fc9379652f8acd15bc"
					);

					string contentsFromURL = await UrlHelpers.GetContents(
												 gistURL
											 );

					ReplaceManifestText(
						contentsFromURL
					);
				}
				catch (Exception ex)
				{
					Debug.LogError(
						$"Failed to overwrite package manifest: {ex}"
					);
				}
			}

			private static void ReplaceManifestText(string contents)
			{
				string existingManifest = Path.Combine(
					UnityEngine.Application.dataPath,
					"../Packages/manifest.json"
				);

				File.WriteAllText(
					existingManifest,
					contents
				);

				Client.Resolve();
			}
		}

		/// <summary>
		///     Applies the default standalone scripting backend and API compatibility settings.
		/// </summary>
		private static class PlayerSettingsSetter
		{
			[MenuItem(
				Constants.BEZORO + "/Project Preparer/Set Player Settings. (IL2CPP + .NET Framework)",
				false,
				40
			)]
			public static void SetPlayerSettings_MenuItem()
			{
				PlayerSettings.SetScriptingBackend(
					NamedBuildTarget.Standalone,
					ScriptingImplementation.IL2CPP
				);

				PlayerSettings.SetApiCompatibilityLevel(
					NamedBuildTarget.Standalone,
					ApiCompatibilityLevel.NET_Unity_4_8
				);
			}
		}
	}
}
