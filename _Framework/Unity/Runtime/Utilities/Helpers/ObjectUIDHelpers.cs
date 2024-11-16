using System;
using System.Collections.Generic;
using UnityEngine;

namespace Bezoro.Framework.Unity.Utilities
{
	public static class ObjectUIDHelpers
	{
		public static string[] GetUIDsFromGameObjects( IList<GameObject> gameObjects, string[] outputArray = null )
		{
			if ( gameObjects == null || gameObjects.Count == 0 )
			{
				return Array.Empty<string>();
			}

			var count = gameObjects.Count;
			var uids = outputArray ?? new string[count];
			var validCount = 0;

			for ( var i = 0 ; i < count ; i++ )
			{
				if ( gameObjects[i].TryGetComponent( out ObjectUID uidComponent ) )
				{
					uids[validCount++] = uidComponent.UID;
				}
			}

			if ( validCount < count )
			{
				Array.Resize( ref uids, validCount );
			}

			return uids;
		}
	}
}
