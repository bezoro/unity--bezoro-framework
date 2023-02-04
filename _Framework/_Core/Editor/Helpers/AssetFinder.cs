using UnityEditor;

public static class AssetFinder
{
	public static T FindAsset<T>(string assetName)
	where T : UnityEngine.Object
	{
		var guids = AssetDatabase.FindAssets(assetName);
		foreach (var guid in guids)
		{
			var assetPath = AssetDatabase.GUIDToAssetPath(guid);
			var asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
			if (asset != null)
				return asset;
		}

		return null;
	}
}