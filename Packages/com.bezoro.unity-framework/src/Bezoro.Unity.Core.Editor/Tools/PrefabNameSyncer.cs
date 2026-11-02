using UnityEditor;
using UnityEngine;

namespace Bezoro.Unity.Core.Editor.Tools
{
	/// <summary>
	///     Renames a selected prefab instance to match its source prefab asset.
	/// </summary>
	public static class PrefabNameSyncer
	{
		private const string PATH = "GameObject/Prefab/Sync Prefab Asset Name";

		[MenuItem(
			PATH,
			true
		)]
		private static bool ValidateSyncPrefabAssetName()
		{
			// Only show the menu item if:
			// 1. A GameObject is selected in the hierarchy
			// 2. The selected GameObject is a prefab instance
			if (Selection.activeGameObject == null) return false;

			return PrefabUtility.IsPartOfPrefabInstance(
				Selection.activeGameObject
			);
		}

		[MenuItem(
			PATH,
			false,
			0
		)]
		private static void SyncPrefabAssetName()
		{
			var selectedGameObject = Selection.activeGameObject;

			if (selectedGameObject == null)
			{
				Debug.LogWarning(
					"No GameObject selected."
				);

				return;
			}

			// Check if the selected object is a prefab instance
			if (!PrefabUtility.IsPartOfPrefabInstance(
					selectedGameObject
				))
			{
				Debug.LogWarning(
					"Selected GameObject is not a prefab instance."
				);

				return;
			}

			// Get the prefab asset
			var prefabAsset = PrefabUtility.GetCorrespondingObjectFromSource(
				selectedGameObject
			);

			if (prefabAsset == null)
			{
				Debug.LogError(
					"Could not find the prefab asset for the selected instance."
				);

				return;
			}

			// Get the asset path
			string assetPath = AssetDatabase.GetAssetPath(
				prefabAsset
			);

			if (string.IsNullOrEmpty(
					assetPath
				))
			{
				Debug.LogError(
					"Could not get asset path for the prefab."
				);

				return;
			}

			// Get the new name from the instance
			string newName = selectedGameObject.name;

			// Rename the asset
			string result = AssetDatabase.RenameAsset(
				assetPath,
				newName
			);

			if (!string.IsNullOrEmpty(
					result
				))
			{
				Debug.LogError(
					$"Failed to rename prefab asset: {result}"
				);

				return;
			}

			// Refresh the asset database
			AssetDatabase.Refresh();

			Debug.Log(
				$"Prefab asset renamed to: {newName}"
			);
		}
	}
}
