using System.Collections.Generic;
using UnityEngine;

namespace Bezoro.Framework.Unity.Utilities
{
	public static class List_Extensions
	{
		public static bool Equals_Null_Or_Empty<T>( this IList<T> list )
		{
			return list.Equals_Null() || list.Equals_Empty();
		}

		public static bool Equals_Null<T>( this IList<T> list )
		{
			return list == null;
		}

		public static bool Equals_Empty<T>( this IList<T> list )
		{
			return list.Count == 0;
		}

		public static bool Is_Null_Or_Empty<T>( this IList<T> list )
		{
			return list.Is_Null() || list.Is_Empty();
		}

		public static bool Is_Null<T>( this IList<T> list )
		{
			return list is null;
		}

		public static bool Is_Empty<T>( this IList<T> list )
		{
			return list.Count is 0;
		}

		public static T Get_Random<T>( this IList<T> list )
		{
			if ( list == null || list.Count == 0 )
				return default;

			return list[ Random.Range( 0, list.Count ) ];
		}

		public static void Shuffle<T>( this IList<T> list )
		{
			var n = list.Count;

			while ( n > 1 )
			{
				n--;
				var k = Random.Range( 0, n + 1 );

				( list[ k ], list[ n ] ) = ( list[ n ], list[ k ] );
			}
		}
	}
}
