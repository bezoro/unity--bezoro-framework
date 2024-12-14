using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Bezoro.Framework.Unity.Editor.Utilities
{
	public static class Unity_Object_Editor_Extensions
	{
		/// <summary>
		///     Checks if an asset of type <typeparamref name="T" /> is unique in the Unity project.
		///     If not, logs a warning message containing the paths of all instances found.
		/// </summary>
		/// <param name="asset">The asset to check.</param>
		/// <param name="logWarning">If true, logs a warning message. Otherwise, only returns true or false.</param>
		/// <typeparam name="T">The type of the asset to check.</typeparam>
		/// <returns>true if the asset is unique, false otherwise.</returns>
		public static bool WarnIfNotUniqueInAssets<T>(this T asset, bool logWarning = true)
		where T : Object
		{
			var type_name = typeof(T).Name;
			var guids     = AssetDatabase.FindAssets($"t:{type_name}");

			if (guids.Length <= 1) return true;

			var asset_path = AssetDatabase.GetAssetPath(asset);

			var other_asset_paths = guids
				.Select(guid => AssetDatabase.GUIDToAssetPath(guid))
				.Where(path => path != asset_path);

			var asset_paths = other_asset_paths.ToList();

			if (!asset_paths.Any()) return true;
			if (!logWarning) return false;

			var asset_paths_string = string.Join("\n", asset_paths);

			Debug.LogWarning
			(
				$"Multiple instances of {type_name} found. Only one should exist.\n"
				+ $"This asset is located at: {asset_path}\n"
				+ $"Other instances found at:\n{asset_paths_string}"
			);

			return false;
		}
	}
}
