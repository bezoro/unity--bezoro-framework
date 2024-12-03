using System.IO;
using Bezoro.Framework.Unity.Editor.Utilities.Helpers;
using Bezoro.Framework.Unity.Utilities;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

namespace Bezoro.Framework.Unity.Editor.Tools
{
	public static class ProjectPreparer
	{
		private static class DirectoryCreator
		{
			[MenuItem(
				"Bezoro/Project Preparer/Create Default Directories",
				false,
				20 )]
			public static void CreateDefaultDirectories_MenuItem()
			{
				var root            = Application.dataPath;
				var foldersToCreate = new[] { "__Project" };

				foreach ( var folder in foldersToCreate )
				{
					if ( Directory.Exists(
							Path.Combine(
								root,
								folder ) ) )
					{
						Debug.LogWarning
							( $"Directory {folder.Bold()} already exists at {root.Italic()}." );

						continue;
					}

					Directory.CreateDirectory(
						Path.Combine(
							root,
							folder ) );

					Debug.Log( $"Directory {folder.Bold()} created at {root.Italic()}." );
				}

				AssetDatabase.Refresh();
			}
		}

		private static class PlayerSettingsSetter
		{
			[MenuItem(
				"Bezoro/Project Preparer/Set Player Settings. (IL2CPP + .NET Framework)",
				false,
				40
			)]
			public static void SetPlayerSettings_MenuItem()
			{
				PlayerSettings.SetScriptingBackend(
					BuildTargetGroup.Standalone,
					ScriptingImplementation.IL2CPP
				);

				PlayerSettings.SetApiCompatibilityLevel(
					BuildTargetGroup.Standalone,
					ApiCompatibilityLevel.NET_Unity_4_8
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
				var gistUrl         = GistHelpers.GetUrl( "ebccb4c31b1bb9fc9379652f8acd15bc" );
				var contentsFromUrl = await UrlHelpers.GetContents( gistUrl );
				ReplaceManifestText( contentsFromUrl );
			}

			private static void ReplaceManifestText( string contents )
			{
				var existingManifest =
					Path.Combine(
						Application.dataPath,
						"../Packages/manifest.json" );

				File.WriteAllText(
					existingManifest,
					contents );

				Client.Resolve();
			}
		}

		private static class InputSystemSettingsSetter
		{
			[MenuItem(
				"Bezoro/Project Preparer/Set Input System (Both)",
				false,
				80 )]
			private static void SetInputSystemBoth_MenuItem()
			{
				if ( InputSystemHelpers.GetActiveInputSystemProperty().intValue
					 == (int)InputSystemHelpers.InputSystem.Both )
				{
					EditorUtility.DisplayDialog(
						"",
						"It's already set",
						"Ok"
					);

					return;
				}

				InputSystemHelpers.SetActiveInputSystem( InputSystemHelpers.InputSystem.Both );

				if ( EditorUtility.DisplayDialog(
						"",
						"Changing the Input System needs to restart the editor",
						"Ok"
					) )
				{
					EditorHelpers.ReopenProject();
				}
			}
		}
	}
}
