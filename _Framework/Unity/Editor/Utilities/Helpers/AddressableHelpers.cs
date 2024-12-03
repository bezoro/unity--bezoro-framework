using UnityEditor;
using UnityEditor.AddressableAssets;

namespace Bezoro.Framework.Unity.Editor.Utilities.Helpers
{
	public static class AddressableHelpers
	{
		public static void MarkAsAddressable( string asset_path, string group_name )
		{
			var settings = AddressableAssetSettingsDefaultObject.Settings;
			var group    = settings.FindGroup( group_name );

			if ( group == null )
			{
				group = settings.CreateGroup( group_name, false, false, false, null );
			}

			settings.CreateOrMoveEntry( AssetDatabase.AssetPathToGUID( asset_path ), group );
		}
	}
}
