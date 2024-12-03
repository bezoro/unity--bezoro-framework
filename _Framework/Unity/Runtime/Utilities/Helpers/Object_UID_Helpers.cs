using System;
using System.Collections.Generic;
using UnityEngine;

namespace Bezoro.Framework.Unity.Utilities
{
	public static class Object_UID_Helpers
	{
		public static string[] GetUIDsFromGameObjects( IList<GameObject> gameObjects, string[] outputArray = null )
		{
			if ( gameObjects == null || gameObjects.Count == 0 )
			{
				return Array.Empty<string>();
			}

			var count       = gameObjects.Count;
			var uids        = outputArray ?? new string[count];
			var valid_count = 0;

			for ( var i = 0 ; i < count ; i++ )
			{
				if ( gameObjects[ i ].TryGetComponent( out Object_UID uid_component ) )
				{
					uids[ valid_count++ ] = uid_component.UID;
				}
			}

			if ( valid_count < count )
			{
				Array.Resize(
					ref uids,
					valid_count );
			}

			return uids;
		}
	}
}
