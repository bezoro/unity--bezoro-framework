using UnityEditor;
using UnityEditor.AddressableAssets;

namespace Bezoro.Framework.Unity.Editor.Utilities.Helpers
{
	public static class AddressableHelpers
	{
		public static void MarkAsAddressable( string assetPath, string groupName )
		{
			var settings = AddressableAssetSettingsDefaultObject.Settings;
			var group = settings.FindGroup( groupName );
			if ( group == null )
			{
				group = settings.CreateGroup( groupName, false, false, false, null );
			}

			settings.CreateOrMoveEntry( AssetDatabase.AssetPathToGUID( assetPath ), group );
		}
	}
}
