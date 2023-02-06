using UnityEditor;

public static class AssetFinder
{
	public static bool TryFindAsset<T>(string assetName, out T asset)
	where T : UnityEngine.Object
	{
		var guids = AssetDatabase.FindAssets(assetName);
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