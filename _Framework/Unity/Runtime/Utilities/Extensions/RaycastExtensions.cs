using System;
using UnityEngine;

namespace Bezoro.Framework.Unity.Utilities
{
	public static class RaycastExtensions
	{
		public static bool Raycast3D( this Ray ray, RaycastHit[] raycastHits3D, out GameObject[] hitObjects )
		{
			var hitCount = Physics.RaycastNonAlloc( ray, raycastHits3D, Mathf.Infinity );

			if ( hitCount > 0 )
			{
				var hitObjectsArray = new GameObject[hitCount];

				for ( var i = 0 ; i < hitCount ; i++ )
				{
					hitObjectsArray[i] = raycastHits3D[i].collider.gameObject;
				}

				hitObjects = hitObjectsArray;

				return true;
			}

			hitObjects = Array.Empty<GameObject>();

			return false;
		}

		public static bool Raycast2D
		(
			this Vector2 mousePosition,
			Camera mainCamera,
			RaycastHit2D[] raycastHits2D,
			out GameObject[] hitObjects
		)
		{
			var worldPoint = mainCamera.ScreenToWorldPoint( mousePosition );
			var contactFilter = new ContactFilter2D();
			var hitCount = Physics2D.Raycast( worldPoint, Vector2.zero, contactFilter, raycastHits2D, 200f );

			if ( hitCount > 0 )
			{
				var hitObjectsArray = new GameObject[hitCount];

				for ( var i = 0 ; i < hitCount ; i++ )
				{
					hitObjectsArray[i] = raycastHits2D[i].collider.gameObject;
				}

				hitObjects = hitObjectsArray;

				return true;
			}

			hitObjects = Array.Empty<GameObject>();

			return false;
		}
	}
}
