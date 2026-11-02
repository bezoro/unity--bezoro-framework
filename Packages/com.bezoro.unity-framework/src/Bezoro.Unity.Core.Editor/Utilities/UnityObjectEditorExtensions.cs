using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Bezoro.Unity.Core.Editor.Utilities
{
	/// <summary>
	///     Contains extension methods for Unity Object Editor.
	/// </summary>
	public static class UnityObjectEditorExtensions
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
			string typeName = typeof(T).Name;
			string[] guids = AssetDatabase.FindAssets(
				$"t:{typeName}"
			);

			if (guids.Length <= 1) return true;

			string assetPath = AssetDatabase.GetAssetPath(
				asset
			);

			var otherAssetPaths = guids
								  .Select(guid => AssetDatabase.GUIDToAssetPath(
											  guid
										  )
								  )
								  .Where(path => path != assetPath
								  );

			var assetPaths = otherAssetPaths.ToList();

			if (!assetPaths.Any()) return true;

			if (!logWarning) return false;

			var assetPathsString = string.Join(
				"\n",
				assetPaths
			);

			Debug.LogWarning
			(
				$"Multiple instances of {typeName} found. Only one should exist.\n" +
				$"This asset is located at: {assetPath}\n" +
				$"Other instances found at:\n{assetPathsString}"
			);

			return false;
		}
	}
}
