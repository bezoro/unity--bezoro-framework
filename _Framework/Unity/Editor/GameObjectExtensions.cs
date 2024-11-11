using UnityEditor;
using UnityEngine;

namespace Bezoro.Framework.Unity.Editor
{
	public static class GameObjectExtensions
	{
		public static string TryGetAssetGUID(this GameObject gameObject)
		{
			var path = AssetDatabase.GetAssetPath(gameObject);
			if (!string.IsNullOrEmpty(path))
			{
				return AssetDatabase.AssetPathToGUID(path);
			}

			BDebug.LogError("GameObject is not an asset or is not part of the project.");

			return null;
		}
	}
}
