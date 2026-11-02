using UnityEditor;
using UnityEditor.AddressableAssets;

namespace Bezoro.Unity.Core.Editor.Utilities
{
	/// <summary>
	///     Creates and manages Addressables groups and entries from editor scripts.
	/// </summary>
	public static class AddressableHelpers
	{
		public static void Mark_As_Addressable(string assetPath, string groupName)
		{
			var settings = AddressableAssetSettingsDefaultObject.Settings;
			var group = settings.FindGroup(
				groupName
			);

			if (group == null)
				group = settings.CreateGroup(
					groupName,
					false,
					false,
					false,
					null
				);

			settings.CreateOrMoveEntry(
				AssetDatabase.AssetPathToGUID(
					assetPath
				),
				group
			);
		}
	}
}
