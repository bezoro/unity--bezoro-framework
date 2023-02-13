using UnityEditor;

public static class AssetHelpers
{
	public static bool TryFindAsset<T>(string assetName, out T asset)
	where T : UnityEngine.Object =>
		TryFindAsset(assetName, new[] { "Assets" }, out asset);

	public static bool TryFindAsset<T>(string assetName, string[] searchInFolders, out T asset)
	where T : UnityEngine.Object
	{
		var guids = AssetDatabase.FindAssets(assetName, searchInFolders);
		foreach (var guid in guids)
		{
			var assetPath = AssetDatabase.GUIDToAssetPath(guid);
			asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
			if (asset != null)
				return true;
		}

		asset = null;
		return false;
	}
}