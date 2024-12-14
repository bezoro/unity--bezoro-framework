using System;
using System.Linq;
using Bezoro.Framework.Unity.Utilities;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Bezoro.Framework.Unity.Editor.Utilities
{
	public static class Asset_Helpers
	{
		public static bool Try_Find_Asset_In_Assets<T>(string asset_name, out T asset)
		where T : Object
		{
			return Try_Find_Asset_In_Specified_Folders(asset_name, new[] { "Assets" }, out asset);
		}

		public static bool Try_Find_Asset_In_Editor_Folders<T>(string asset_name, out T asset)
		where T : Object
		{
			var editor_folders = Get_Folder_Paths_By_Name("Editor");
			return Try_Find_Asset_In_Specified_Folders(asset_name, editor_folders, out asset);
		}

		public static bool Try_Find_Asset_In_Resources_Folders<T>(string asset_name, out T asset)
		where T : Object
		{
			var resources_folders = Get_Folder_Paths_By_Name("Resources");

			return Try_Find_Asset_In_Specified_Folders(asset_name, resources_folders, out asset);
		}

		public static bool Try_Find_Asset_In_Specified_Folders<T>(
			string asset_name,
			string[] folders_to_search_in,
			out T asset
		)
		where T : Object
		{
			asset = null;

			if (string.IsNullOrWhiteSpace(asset_name))
			{
				Debug.LogWarning("Asset name is null, empty, or whitespace.");
				return false;
			}

			if (folders_to_search_in.Is_Null_Or_Empty())
			{
				Debug.LogWarning("No folders specified to search.");
				return false;
			}

			var guids = AssetDatabase.FindAssets(asset_name, folders_to_search_in);

			if (guids.Is_Null_Or_Empty())
			{
				Debug.LogWarning($"No assets found matching name '{asset_name}' in specified folders.");
				return false;
			}

			var asset_path = string.Empty;

			foreach ( var guid in guids )
			{
				asset_path = AssetDatabase.GUIDToAssetPath(guid);
				if (string.IsNullOrWhiteSpace(asset_path)) continue;

				asset = AssetDatabase.LoadAssetAtPath<T>(asset_path);

				if (asset == null)
					continue;

				break;
			}

			if (asset == null)
			{
				Debug.LogWarning($"Asset '{asset_name}' could not be found in the specified folders.");
				return false;
			}

			Debug.Log($"Successfully found asset '{asset_name}' at '{asset_path}'.");
			return true;
		}

		private static string[] Get_Folder_Paths_By_Name(string folder_name)
		{
			if (string.IsNullOrWhiteSpace(folder_name))
			{
				Debug.LogWarning("Folder name is null, empty, or whitespace. Returning empty array.");
				return Array.Empty<string>();
			}

			var folders = AssetDatabase.GetSubFolders("Assets")
				.Where(folder => folder.Contains($"/{folder_name}"))
				.ToArray();

			if (folders.Is_Null_Or_Empty())
				Debug.LogWarning($"Folder '{folder_name}' not found.");

			return folders;
		}
	}
}
