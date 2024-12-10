using System;
using System.Collections.Generic;
using UnityEngine;

namespace Bezoro.Framework.Unity.Utilities
{
	public static class GameObject_GUID_Helpers
	{
		public static string[] Get_GUIDs_From_GameObjects(
			IList<GameObject> game_objects,
			string[] output_array = null
		)
		{
			if ( game_objects == null || game_objects.Count == 0 ) return Array.Empty<string>();

			var count       = game_objects.Count;
			var uids        = output_array ?? new string[count];
			var valid_count = 0;

			for ( var i = 0 ; i < count ; i++ )
			{
				if ( game_objects[i].TryGetComponent( out Serializable_GUID uid_component ) )
					uids[valid_count++] = uid_component.Value;
			}

			if ( valid_count < count )
				Array.Resize( ref uids, valid_count );

			return uids;
		}
	}
}
