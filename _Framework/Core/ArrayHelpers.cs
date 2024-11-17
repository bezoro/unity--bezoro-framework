using System;
using System.Collections.Generic;

namespace Bezoro.Framework.Core
{
	public static class ArrayHelpers
	{
		public static bool ArraysAreEqual<T>( T[] a, T[] b ) where T : class
		{
			if ( a == null && b == null ) { return true; }

			if ( a == null || b == null ) { return false; }

			if ( a.Length != b.Length ) { return false; }

			for ( var i = 0 ; i < a.Length ; i++ )
			{
				if ( !EqualityComparer<T>.Default.Equals( a[i], b[i] ) )
				{
					return false;
				}
			}

			return true;
		}

		public static void Add<T>( ref T[] array, T element ) where T : class
		{
			Add( ref array, element, out _ );
		}

		public static void Add<T>( ref T[] array, T element, out int index ) where T : class
		{
			if ( element == null )
			{
				index = -1;

				return;
			}

			if ( array == null || array.Length == 0 )
			{
				CreateArrayAndAddFirstElement( ref array, element );
				index = 0;

				return;
			}

			for ( var i = 0 ; i < array.Length ; i++ )
			{
				if ( array[i] != null )
				{
					continue;
				}

				array[i] = element;
				index    = i;

				return;
			}

			ResizeArrayAndAddElement( ref array, element, 2, out var oldLength );
			index = oldLength;
		}

		private static void ResizeArrayAndAddElement<T>( ref T[] array, T element, int factor, out int oldLength )
		{
			oldLength = array.Length;
			MultiplySize( ref array, factor );
			array[oldLength] = element;
		}

		public static void Add<T>( ref T[] array, T element, ref int validSize, out int index ) where T : class
		{
			if ( validSize < 0 )
			{
				index = -1;

				return;
			}

			if ( element == null )
			{
				index = -1;

				return;
			}

			if ( array == null || array.Length == 0 )
			{
				CreateArrayAndAddFirstElement( ref array, element );
				validSize = 1;
				index     = 0;

				return;
			}

			if ( validSize < array.Length )
			{
				array[validSize] = element;
				validSize++;
				index = validSize;

				return;
			}

			ResizeArrayAndAddElement( ref array, element, 2, ref validSize, out var oldLength );
			index = oldLength;
		}

		private static void ResizeArrayAndAddElement<T>
			( ref T[] array, T element, int factor, ref int validSize, out int oldLength )
		{
			ResizeArrayAndAddElement( ref array, element, factor, out oldLength );
			validSize++;
		}

		public static void MultiplySize<T>( ref T[] array, int factor )
		{
			Array.Resize( ref array, array.Length * factor );
		}

		public static void AddUnique<T>( ref T[] array, T element, out int index ) where T : class
		{
			if ( element == null )
			{
				index = -1;

				return;
			}

			if ( array == null || array.Length == 0 )
			{
				CreateArrayAndAddFirstElement( ref array, element );
				index = 0;

				return;
			}

			if ( Array.IndexOf( array, element ) >= 0 )
			{
				index = -1;

				return;
			}

			Add( ref array, element, out var i );
			index = i;
		}

		public static void AddUnique<T>( ref T[] array, T element, ref int validSize, out int index ) where T : class
		{
			if ( element == null )
			{
				index = -1;

				return;
			}

			if ( array == null || array.Length == 0 )
			{
				CreateArrayAndAddFirstElement( ref array, element );
				validSize = 1;
				index     = 0;

				return;
			}

			if ( Array.IndexOf( array, element, 0, validSize ) >= 0 )
			{
				index = -1;

				return;
			}

			Add( ref array, element, ref validSize, out var i );
			index = i;
		}

		public static void CreateArrayAndAddFirstElement<T>( ref T[] array, T element )
		{
			Assert.IsNotNull( element );
			if ( array == null || array.Length == 0 )
			{
				array = new T[1];
			}

			array[0] = element;
		}

		public static void Merge<T>( T[] from, ref T[] to )
		where T : class
		{
			if ( from == null || from.Length == 0 )
			{
				return;
			}

			if ( to == null || to.Length == 0 )
			{
				Array.Resize( ref to, from.Length );
				Copy( from, ref to );

				return;
			}

			if ( to.Length < from.Length )
			{
				Array.Resize( ref to, from.Length );
				Copy( from, ref to );

				return;
			}

			for ( var i = 0 ; i < from.Length ; i++ )
			{
				if ( to[i] != null ) { continue; }

				to[i] = from[i];
			}
		}

		public static void Copy<T>( T[] from, ref T[] to )
		{
			Assert.IsNotNull( from );
			Assert.IsFalse( from.Length == 0 );

			if ( to == null || to.Length < from.Length )
			{
				to = new T[from.Length];
			}

			Array.Copy( from, to, from.Length );
		}

		public static bool TryRemove<T>( ref T[] array, T element, out int index ) where T : class
		{
			Assert.IsNotNull( array );
			Assert.IsFalse( array.Length == 0 );
			Assert.IsNotNull( element );

			var foundIndex = Array.IndexOf( array, element );
			if ( foundIndex >= 0 )
			{
				array[foundIndex] = default(T);
				index             = foundIndex;
				return true;
			}

			index = -1;
			return false;
		}

		public static bool TryRemoveElement<T>( ref T[] array, T element, ref int validSize, out int index )
		where T : class
		{
			Assert.IsNotNull( array );
			Assert.IsFalse( array.Length == 0 );
			Assert.IsNotNull( element );
			Assert.IsTrue( validSize >= 0 );

			// Find the index of the element to remove
			var removalIndex = Array.IndexOf( array, element, 0, validSize );

			if ( removalIndex >= 0 )
			{
				// Move the last valid element to the removalIndex
				array[removalIndex] = array[validSize - 1];

				// Set the last valid element to the default value
				array[validSize - 1] = default(T)!;

				// Update the valid size
				validSize--;

				// Set the index to the removalIndex
				index = removalIndex;

				return true;
			}

			index = -1;
			return false;
		}

		public static void Clear<T>( ref T[] array ) where T : class
		{
			if ( array == null || array.Length == 0 )
			{
				return;
			}

			for ( var i = 0 ; i < array.Length ; i++ )
			{
				array[i] = default(T)!;
			}
		}

		public static void Clear<T>( ref T[] array, int validSize ) where T : class
		{
			if ( validSize <= 0 )
			{
				return;
			}

			if ( array == null || array.Length == 0 )
			{
				return;
			}

			for ( var i = 0 ; i < validSize ; i++ )
			{
				array[i] = default(T)!;
			}
		}
	}
}
