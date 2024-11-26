using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bezoro.Framework.Core
{
	public static class ArrayHelpers
	{
		public static int ParallelThreshold;

		static ArrayHelpers()
		{
			ParallelThreshold = 100;
		}

		public static bool ArraysAreEqual<T>( T[] a, T[] b ) where T : class
		{
			if ( a == null && b == null ) { return true; }
			if ( a == null || b == null ) { return false; }
			if ( a.Length != b.Length ) { return false; }

			for ( var i = 0 ; i < a.Length ; i++ )
			{
				if ( !EqualityComparer<T>.Default.Equals( a[i], b[i] ) ) { return false; }
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
				if ( array[i] != null ) { continue; }
				array[i] = element;
				index    = i;
				return;
			}

			ResizeArrayAndAddElement( ref array, element, 2, out var old_length );
			index = old_length;
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

			ResizeArrayAndAddElement( ref array, element, 2, ref validSize, out var old_length );
			index = old_length;
		}

		private static void ResizeArrayAndAddElement<T>(
			ref T[] array,
			T       element,
			int     factor,
			ref int validSize,
			out int oldLength )
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
			if ( array             == null ) { array = new T[1]; }
			else if ( array.Length == 0 ) { array    = new T[1]; }
			array[0] = element;
		}

		public static void Merge<T>( T[] from, ref T[] to )
		where T : class
		{
			if ( from        == null ) { return; }
			if ( from.Length == 0 ) { return; }

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

			var found_index = Array.IndexOf( array, element );
			if ( found_index >= 0 )
			{
				array[found_index] = default(T);
				index              = found_index;
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
			Assert.IsTrue( validSize <= array.Length );

			return validSize < ParallelThreshold
						 ? TryRemoveElementSequential( ref array, element, ref validSize, out index )
						 : TryRemoveElementParallel( ref array, element, ref validSize, out index );
		}

		private static bool TryRemoveElementSequential<T>( ref T[] array, T element, ref int validSize, out int index )
		where T : class
		{
			var removal_index = Array.IndexOf( array, element, 0, validSize );
			if ( removal_index >= 0 )
			{
				array[removal_index] = array[validSize - 1];
				array[validSize                        - 1] = default(T)!;
				validSize--;
				index = removal_index;
				return true;
			}

			index = -1;
			return false;
		}

		private static bool TryRemoveElementParallel<T>( ref T[] array, T element, ref int validSize, out int index )
		where T : class
		{
			var local_array      = array;
			var local_valid_size = validSize;

			var removal_index = -1;
			var lock_object   = new object();

			Parallel.For(
				0,
				local_valid_size,
				( i, state ) =>
				{
					if ( !EqualityComparer<T>.Default.Equals( local_array[i], element ) ) { return; }
					lock (lock_object)
					{
						if ( removal_index != -1 ) { return; }
						removal_index = i;
						state.Stop();
					}
				}
			);

			if ( removal_index >= 0 )
			{
				// Synchronize array updates
				lock (lock_object)
				{
					local_array[removal_index] = local_array[local_valid_size - 1];
					local_array[local_valid_size                              - 1] = default(T)!;
					local_valid_size--;

					array     = local_array;
					validSize = local_valid_size;
					index     = removal_index;
					return true;
				}
			}

			index = -1;
			return false;
		}

		public static void Clear<T>( ref T[] array )
		{
			Assert.IsNotNull( array );
			Assert.IsFalse( array.Length == 0 );

			var temp_array = new T[array.Length];
			if ( array.Length < ParallelThreshold )
			{
				for ( var i = 0 ; i < array.Length ; i++ ) { temp_array[i] = default(T); }
			}
			else
			{
				Parallel.For(
					0,
					array.Length,
					i => { temp_array[i] = default(T); }
				);
			}

			Array.Copy( temp_array, array, array.Length );
		}

		public static void Clear<T>( T[] array )
		{
			Assert.IsNotNull( array );
			Assert.IsFalse( array.Length == 0 );

			if ( array.Length < ParallelThreshold )
			{
				for ( var i = 0 ; i < array.Length ; i++ )
				{
					array[i] = default(T);
				}
			}
			else
			{
				Parallel.For( 0, array.Length, i => { array[i] = default(T); } );
			}
		}

		public static void Clear<T>( ref T[] array, int validSize )
		where T : class
		{
			Assert.IsNotNull( array );
			Assert.IsFalse( array.Length == 0 );
			Assert.IsTrue( validSize     >= 0 );
			Assert.IsTrue( validSize     <= array.Length );

			var temp_array = array;
			if ( validSize < ParallelThreshold )
			{
				for ( var i = 0 ; i < validSize ; i++ )
				{
					temp_array[i] = default(T)!;
				}
			}
			else
			{
				Parallel.For( 0, validSize, i => { temp_array[i] = default(T)!; } );
			}
			Array.Copy( temp_array, array, array.Length );
		}
	}
}
