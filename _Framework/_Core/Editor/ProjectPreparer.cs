using System.IO;
using Bezoro.Core.Editor.Extensions;
using Bezoro.Core.Editor.Helpers;
using Bezoro.Core.Helpers;
using UnityEditor;
using UnityEngine;

namespace Bezoro.Core.Editor
{
	public static class ProjectPreparer
	{
		private static class DirectoryCreator
		{
			[MenuItem("Bezoro/Create Default Directories.", false, 20)]
			public static void CreateDefaultDirectories_MenuItem()
			{
				var root = Application.dataPath;
				var foldersToCreate = new[] { "_Project" };

				foreach (var folder in foldersToCreate)
				{
					if (Directory.Exists(Path.Combine(root, folder)))
					{
						Debug.LogWarning
							($"Directory {folder.Bold()} already exists at {root.Italic()}.");

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
			[MenuItem("Bezoro/Set Player Settings. (IL2CPP + .NET 4.8)", false, 40)]
			public static void SetPlayerSettings_MenuItem()
			{
				PlayerSettings.SetScriptingBackend
					(BuildTargetGroup.Standalone, ScriptingImplementation.IL2CPP);

				PlayerSettings.SetApiCompatibilityLevel
					(BuildTargetGroup.Standalone, ApiCompatibilityLevel.NET_Unity_4_8);
			}
		}

		private static class ManifestdotjsonOverwritter
		{
			[MenuItem("Bezoro/Overwrite 'manifest.json' from Gist. (2021 LTS Base)", false, 60)]
			public static async void OverwriteManifest_MenuItem()
			{
				var gistUrl = GistHelpers.GetUrl("ebccb4c31b1bb9fc9379652f8acd15bc");
				var contentsFromUrl = await UrlHelpers.GetContents(gistUrl);
				ReplaceManifestText(contentsFromUrl);
			}

			private static void ReplaceManifestText(string contents)
			{
				var existingManifest = Path.Combine
					(Application.dataPath, "../Packages/manifest.json");

				File.WriteAllText(existingManifest, contents);
				UnityEditor.PackageManager.Client.Resolve();
			}
		}

		private static class InputSystemSettingsSetter
		{
			[MenuItem("Bezoro/Set Input System (Both)", false, 80)]
			private static void SetInputSystemBoth_MenuItem()
			{
				if (InputSystemHelpers.GetActiveInputSystemProperty().intValue ==
					(int)InputSystemHelpers.InputSystem.Both)
				{
					EditorUtility.DisplayDialog
						(
						 "",
						 "It's already set",
						 "Ok"
						);


					return;
				}

				InputSystemHelpers.SetActiveInputSystem(InputSystemHelpers.InputSystem.Both);

				if (EditorUtility.DisplayDialog
						(
						 "",
						 "Changing the Input System needs to restart the editor",
						 "Ok"
						))
				{
					EditorManipulator.ReopenProject();
				}
			}
		}

		private class PluginsSymlinkCreator
		{
			[MenuItem("Bezoro/Create Shared Plugins Symlink. (2021 LTS)", false, 100)]
			public static void CreateSharedPluginsSymlink_MenuItem()
			{
				const string symlinkName = "_SharedPlugins";
				const string sourcePath = "C:/Users/bezor/OneDrive/Unity Plugins/Shared";

				DirectoryHelpers.CreateSymlink(Application.dataPath, symlinkName, sourcePath);
			}

			[MenuItem("Bezoro/Create HDRP Plugins Symlink. (2021 LTS)", false, 100)]
			public static void CreateHDRPPluginsSymlink_MenuItem()
			{
				const string symlinkName = "_HDRPPlugins";
				const string sourcePath = "C:/Users/bezor/OneDrive/Unity Plugins/HDRP";

				DirectoryHelpers.CreateSymlink(Application.dataPath, symlinkName, sourcePath);
			}

			[MenuItem("Bezoro/Create URP Plugins Symlink. (2021 LTS)", false, 100)]
			public static void CreateURPPluginsSymlink_MenuItem()
			{
				const string symlinkName = "_URPPlugins";
				const string sourcePath = "C:/Users/bezor/OneDrive/Unity Plugins/URP";

				DirectoryHelpers.CreateSymlink(Application.dataPath, symlinkName, sourcePath);
			}
		}
	}
}