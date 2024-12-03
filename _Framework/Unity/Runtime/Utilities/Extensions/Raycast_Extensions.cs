using System;
using UnityEngine;

namespace Bezoro.Framework.Unity.Utilities
{
	public static class Raycast_Extensions
	{
		public static bool Raycast3D( this Ray ray, RaycastHit[] hits, out GameObject[] hit_game_objects )
		{
			var hit_count = Physics.RaycastNonAlloc( ray, hits, Mathf.Infinity );

			if ( hit_count > 0 )
			{
				var hit_objects_array = new GameObject[hit_count];

				for ( var i = 0 ; i < hit_count ; i++ ) hit_objects_array[ i ] = hits[ i ].collider.gameObject;
				hit_game_objects = hit_objects_array;
				return true;
			}

			hit_game_objects = Array.Empty<GameObject>();
			return false;
		}

		public static bool Raycast2D(
			this Vector2 mousePosition,
			Camera mainCamera,
			RaycastHit2D[] raycastHits2D,
			out GameObject[] hitObjects
		)
		{
			var world_point    = mainCamera.ScreenToWorldPoint( mousePosition );
			var contact_filter = new ContactFilter2D();
			var hit_count      = Physics2D.Raycast( world_point, Vector2.zero, contact_filter, raycastHits2D, 200f );

			if ( hit_count > 0 )
			{
				var hit_objects_array = new GameObject[hit_count];

				for ( var i = 0 ; i < hit_count ; i++ ) hit_objects_array[ i ] = raycastHits2D[ i ].collider.gameObject;

				hitObjects = hit_objects_array;
				return true;
			}

			hitObjects = Array.Empty<GameObject>();
			return false;
		}
	}
}
