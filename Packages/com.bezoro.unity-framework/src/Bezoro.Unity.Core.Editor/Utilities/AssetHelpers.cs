using System;
using System.IO;
using System.Linq;
using Bezoro.Core.Extensions;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Bezoro.Unity.Core.Editor.Utilities
{
	// TODO: Fix naming conventions
	/// <summary>
	///     Finds Unity assets by name within Assets, Editor, Resources, or caller-specified folders.
	/// </summary>
	public static class AssetHelpers
	{
		public static bool Try_Find_Asset_In_Assets<T>(string assetName, out T asset)
			where T : Object =>
			Try_Find_Asset_In_Specified_Folders(
				assetName,
				new[] { "Assets" },
				out asset
			);

		public static bool Try_Find_Asset_In_Editor_Folders<T>(string assetName, out T asset)
			where T : Object
		{
			string[] editorFolders = Get_Folder_Paths_By_Name(
				"Editor"
			);

			return Try_Find_Asset_In_Specified_Folders(
				assetName,
				editorFolders,
				out asset
			);
		}

		public static bool Try_Find_Asset_In_Resources_Folders<T>(string assetName, out T asset)
			where T : Object
		{
			string[] resourcesFolders = Get_Folder_Paths_By_Name(
				"Resources"
			);

			return Try_Find_Asset_In_Specified_Folders(
				assetName,
				resourcesFolders,
				out asset
			);
		}

		public static bool Try_Find_Asset_In_Specified_Folders<T>(
			string   assetName,
			string[] foldersToSearchIn,
			out T    asset
		)
			where T : Object
		{
			asset = null!;

			if (string.IsNullOrWhiteSpace(
					assetName
				))
			{
				Debug.LogWarning(
					"Asset name is null, empty, or whitespace."
				);

				return false;
			}

			if (foldersToSearchIn.IsNullOrEmpty())
			{
				Debug.LogWarning(
					"No folders specified to search."
				);

				return false;
			}

			string[] guids = AssetDatabase.FindAssets(
				assetName,
				foldersToSearchIn
			);

			if (guids.IsNullOrEmpty())
			{
				Debug.LogWarning(
					$"No assets found matching name '{assetName}' in specified folders."
				);

				return false;
			}

			string? assetPath = string.Empty;

			foreach (string guid in guids)
			{
				assetPath = AssetDatabase.GUIDToAssetPath(
					guid
				);

				if (string.IsNullOrWhiteSpace(
						assetPath
					)) continue;

				asset = AssetDatabase.LoadAssetAtPath<T>(
					assetPath
				);

				if (asset == null) continue;

				break;
			}

			if (asset == null)
			{
				Debug.LogWarning(
					$"Asset '{assetName}' could not be found in the specified folders."
				);

				return false;
			}

			Debug.Log(
				$"Successfully found asset '{assetName}' at '{assetPath}'."
			);

			return true;
		}

		private static string[] Get_Folder_Paths_By_Name(string folderName)
		{
			if (string.IsNullOrWhiteSpace(
					folderName
				))
			{
				Debug.LogWarning(
					"Folder name is null, empty, or whitespace. Returning empty array."
				);

				return Array.Empty<string>();
			}

			string[] folders = AssetDatabase.FindAssets(
												$"{folderName} t:DefaultAsset",
												new[] { "Assets" }
											)
											.Select(AssetDatabase.GUIDToAssetPath)
											.Where(AssetDatabase.IsValidFolder)
											.Where(folder =>
													   string.Equals(
														   Path.GetFileName(folder),
														   folderName,
														   StringComparison.OrdinalIgnoreCase
													   )
											)
											.ToArray();

			if (folders.IsNullOrEmpty())
				Debug.LogWarning(
					$"Folder '{folderName}' not found."
				);

			return folders;
		}
	}
}
