using System.Collections.Generic;
using UnityEngine;

namespace Bezoro.Framework.Unity.Utilities.Extensions
{
	public static class ListExtensions
	{
		public static bool EqualsNullOrEmpty<T>( this IList<T> list ) => list.EqualsNull() || list.EqualsEmpty();

		public static bool EqualsNull<T>( this IList<T> list ) => list == null;

		public static bool EqualsEmpty<T>( this IList<T> list ) => list.Count == 0;

		public static bool IsNullOrEmpty<T>( this IList<T> list ) => list.IsNull() || list.IsEmpty();

		public static bool IsNull<T>( this IList<T> list ) => list is null;

		public static bool IsEmpty<T>( this IList<T> list ) => list.Count is 0;

		public static T GetRandom<T>( this IList<T> list )
		{
			if ( list == null || list.Count == 0 )
			{
				return default( T );
			}

			return list[Random.Range( 0, list.Count )];
		}

		public static void Shuffle<T>( this IList<T> list )
		{
			var n = list.Count;

			while (n > 1)
			{
				n--;
				var k = Random.Range( 0, n + 1 );
				( list[k], list[n] ) = ( list[n], list[k] );
			}
		}
	}
}
