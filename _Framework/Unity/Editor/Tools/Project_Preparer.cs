using System.IO;
using Bezoro.Framework.Core;
using Bezoro.Framework.Unity.Editor.Utilities;
using Bezoro.Framework.Unity.Editor.Utilities.Helpers;
using Bezoro.Framework.Unity.Utilities;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.PackageManager;
using UnityEngine;

namespace Bezoro.Framework.Unity.Editor.Tools
{
	public static class Project_Preparer
	{
		private static class DirectoryCreator
		{
			[MenuItem(Constants.BEZORO + "/Project Preparer/Create Default Directories", false, 20)]
			public static void CreateDefaultDirectories_MenuItem()
			{
				var root              = Application.dataPath;
				var folders_to_create = new[] { Constants.PROJECT_FOLDER_NAME };

				foreach ( var folder in folders_to_create )
				{
					if (Directory.Exists(Path.Combine(root, folder)))
					{
						Debug.LogWarning($"Directory {folder.Bold()} already exists at {root.Italic()}.");
						continue;
					}

					Directory.CreateDirectory(Path.Combine(root, folder));
					Debug.Log($"Directory {folder.Bold()} created at {root.Italic()}.");
				}

				AssetDatabase.Refresh();
			}
		}

		private static class PlayerSettingsSetter
		{
			[MenuItem(
				Constants.BEZORO + "/Project Preparer/Set Player Settings. (IL2CPP + .NET Framework)", false, 40
			)]
			public static void SetPlayerSettings_MenuItem()
			{
				PlayerSettings.SetScriptingBackend(NamedBuildTarget.Standalone, ScriptingImplementation.IL2CPP);

				PlayerSettings.SetApiCompatibilityLevel(
					NamedBuildTarget.Standalone, ApiCompatibilityLevel.NET_Unity_4_8
				);
			}
		}

		private static class PackagesManifestOverwritter
		{
			// [MenuItem("Bezoro/Project Preparer/Overwrite 'manifest.json' from Gist. (2021 LTS Base)",
			// 		  false,
			// 		  60)]
			public static async void OverwriteManifest_MenuItem()
			{
				var gist_url          = Gist_Helpers.Get_URL("ebccb4c31b1bb9fc9379652f8acd15bc");
				var contents_from_url = await UrlHelpers.GetContents(gist_url);
				ReplaceManifestText(contents_from_url);
			}

			private static void ReplaceManifestText(string contents)
			{
				var existing_manifest = Path.Combine(Application.dataPath, "../Packages/manifest.json");
				File.WriteAllText(existing_manifest, contents);
				Client.Resolve();
			}
		}

		private static class InputSystemSettingsSetter
		{
			[MenuItem(Constants.BEZORO + "/Project Preparer/Set Input System (Both)", false, 80)]
			private static void Set_Input_System_To_Both()
			{
				var       active_input_system = Input_System_Helpers.Get_Active_Input_System_Property().intValue;
				const int both_input_system   = (int)Input_System_Helpers.Input_System.Both;

				if (active_input_system == both_input_system)
				{
					EditorUtility.DisplayDialog("", "It's already set", "Ok");
					return;
				}

				Input_System_Helpers.Set_Active_Input_System(Input_System_Helpers.Input_System.Both);

				var user_confirmed_restart = EditorUtility.DisplayDialog(
					"",
					"Changing the Input System needs to restart the editor",
					"Ok"
				);

				if (user_confirmed_restart)
					Editor_Helpers.Reopen_Project();
			}
		}
	}
}
