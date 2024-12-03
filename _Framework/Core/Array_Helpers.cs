using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bezoro.Framework.Core
{
	public static class Array_Helpers
	{
		public static int Parallel_Threshold;

		static Array_Helpers()
		{
			Parallel_Threshold = 100;
		}

		public static void RemoveDuplicates<T_Array>( ref T_Array[] array )
		where T_Array : class
		{
			if ( array is not { Length: > 1 } ) return;

			var unique_elements = new List<T_Array>();
			var seen_elements   = new HashSet<T_Array>();

			for ( var i = 0 ; i < array.Length ; i++ )
			{
				var element = array[ i ];
				if ( element == null ) continue;
				if ( !seen_elements.Add( element ) ) continue;

				unique_elements.Add( element );
			}

			array = unique_elements.ToArray();
		}

		public static bool ArraysAreEqual<T_Array>( T_Array[] a, T_Array[] b )
		where T_Array : class
		{
			if ( a == null && b == null ) return true;
			if ( a == null || b == null ) return false;
			if ( a.Length != b.Length ) return false;

			for ( var i = 0 ; i < a.Length ; i++ )
			{
				if ( !EqualityComparer<T_Array>.Default.Equals( a[ i ], b[ i ] ) )
					return false;
			}

			return true;
		}

		public static void Add<T_Array>( ref T_Array[] array, T_Array element )
		where T_Array : class
		{
			Add( ref array, element, out _ );
		}

		public static void Add<T_Array>( ref T_Array[] array, T_Array element, out int index )
		where T_Array : class
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
				if ( array[ i ] != null ) continue;

				array[ i ] = element;
				index      = i;
				return;
			}

			ResizeArrayAndAddElement( ref array, element, 2, out var old_length );
			index = old_length;
		}

		private static void ResizeArrayAndAddElement<T_Element>(
			ref T_Element[] array,
			T_Element element,
			int factor,
			out int old_length
		)
		{
			old_length = array.Length;
			MultiplySize( ref array, factor );
			array[ old_length ] = element;
		}

		public static void Add<T_Element>( ref T_Element[] array, T_Element element, ref int valid_size, out int index )
		where T_Element : class
		{
			if ( valid_size < 0 )
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
				valid_size = 1;
				index      = 0;
				return;
			}

			if ( valid_size < array.Length )
			{
				array[ valid_size ] = element;
				valid_size++;
				index = valid_size;
				return;
			}

			ResizeArrayAndAddElement( ref array, element, 2, ref valid_size, out var old_length );
			index = old_length;
		}

		private static void ResizeArrayAndAddElement<T_Element>(
			ref T_Element[] array,
			T_Element element,
			int factor,
			ref int valid_size,
			out int old_length
		)
		{
			ResizeArrayAndAddElement( ref array, element, factor, out old_length );
			valid_size++;
		}

		public static void MultiplySize<T_Array>( ref T_Array[] array, int factor )
		{
			Array.Resize( ref array, array.Length * factor );
		}

		public static void AddUnique<T_Element>( ref T_Element[] array, T_Element element, out int index )
		where T_Element : class
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

		public static void AddUnique<T_Element>(
			ref T_Element[] array,
			T_Element element,
			ref int valid_size,
			out int index
		)
		where T_Element : class
		{
			if ( element == null )
			{
				index = -1;
				return;
			}

			if ( array == null || array.Length == 0 )
			{
				CreateArrayAndAddFirstElement( ref array, element );
				valid_size = 1;
				index      = 0;
				return;
			}

			if ( Array.IndexOf( array, element, 0, valid_size ) >= 0 )
			{
				index = -1;
				return;
			}

			Add( ref array, element, ref valid_size, out var i );
			index = i;
		}

		public static void CreateArrayAndAddFirstElement<T_Element>( ref T_Element[] array, T_Element element )
		{
			Assert.Is_Not_Null( element );

			if ( array == null )
				array = new T_Element[1];
			else if ( array.Length == 0 )
				array = new T_Element[1];

			array[ 0 ] = element;
		}

		public static void Merge<T_Array>( T_Array[] from, ref T_Array[] to )
		where T_Array : class
		{
			if ( from == null ) return;
			if ( from.Length == 0 ) return;

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
				if ( to[ i ] != null ) continue;

				to[ i ] = from[ i ];
			}
		}

		public static void Copy<T_Array>( T_Array[] from, ref T_Array[] to )
		{
			Assert.Is_Not_Null( from );
			Assert.Is_False( from.Length == 0 );

			if ( to == null || to.Length < from.Length ) to = new T_Array[from.Length];

			Array.Copy( from, to, from.Length );
		}

		public static bool TryRemove<T_Element>( ref T_Element[] array, T_Element element, out int index )
		where T_Element : class
		{
			Assert.Is_Not_Null( array );
			Assert.Is_False( array.Length == 0 );
			Assert.Is_Not_Null( element );

			var found_index = Array.IndexOf( array, element );

			if ( found_index >= 0 )
			{
				array[ found_index ] = default;
				index                = found_index;
				return true;
			}

			index = -1;
			return false;
		}

		public static bool TryRemoveElement<T_Element>(
			ref T_Element[] array,
			T_Element element,
			ref int valid_size,
			out int index
		)
		where T_Element : class
		{
			Assert.Is_Not_Null( array );
			Assert.Is_False( array.Length == 0 );
			Assert.Is_Not_Null( element );
			Assert.Is_True( valid_size >= 0 );
			Assert.Is_True( valid_size <= array.Length );

			return valid_size < Parallel_Threshold
				? TryRemoveElementSequential( ref array, element, ref valid_size, out index )
				: TryRemoveElementParallel( ref array, element, ref valid_size, out index );
		}

		private static bool TryRemoveElementSequential<T_Element>(
			ref T_Element[] array,
			T_Element element,
			ref int valid_size,
			out int index
		)
		where T_Element : class
		{
			var removal_index = Array.IndexOf( array, element, 0, valid_size );

			if ( removal_index >= 0 )
			{
				array[ removal_index ]  = array[ valid_size - 1 ];
				array[ valid_size - 1 ] = default!;
				valid_size--;
				index = removal_index;
				return true;
			}

			index = -1;
			return false;
		}

		private static bool TryRemoveElementParallel<T_Element>(
			ref T_Element[] array,
			T_Element element,
			ref int valid_size,
			out int index
		)
		where T_Element : class
		{
			var local_array      = array;
			var local_valid_size = valid_size;

			var removal_index = -1;
			var lock_object   = new object();

			Parallel.For(
				0, local_valid_size, ( i, state ) =>
				{
					if ( !EqualityComparer<T_Element>.Default.Equals( local_array[ i ], element ) )
						return;

					lock ( lock_object )
					{
						if ( removal_index != -1 ) return;

						removal_index = i;
						state.Stop();
					}
				}
			);

			if ( removal_index >= 0 )
				// Synchronize array updates
			{
				lock ( lock_object )
				{
					local_array[ removal_index ]        = local_array[ local_valid_size - 1 ];
					local_array[ local_valid_size - 1 ] = default!;
					local_valid_size--;

					array      = local_array;
					valid_size = local_valid_size;
					index      = removal_index;
					return true;
				}
			}

			index = -1;
			return false;
		}

		public static void Clear<T_Array>( ref T_Array[] array )
		{
			Assert.Is_Not_Null( array );
			Assert.Is_False( array.Length == 0 );

			var temp_array = new T_Array[array.Length];

			if ( array.Length < Parallel_Threshold )
			{
				for ( var i = 0 ; i < array.Length ; i++ )
					temp_array[ i ] = default;
			}
			else Parallel.For( 0, array.Length, i => { temp_array[ i ] = default; } );

			Array.Copy( temp_array, array, array.Length );
		}

		public static void Clear<T_Array_Type>( T_Array_Type[] array )
		{
			Assert.Is_Not_Null( array );
			Assert.Is_False( array.Length == 0 );

			if ( array.Length < Parallel_Threshold )
			{
				for ( var i = 0 ; i < array.Length ; i++ ) array[ i ] = default;
			}
			else Parallel.For( 0, array.Length, i => { array[ i ] = default; } );
		}

		public static void Clear<T_Array_Type>( ref T_Array_Type[] array, int valid_size )
		where T_Array_Type : class
		{
			Assert.Is_Not_Null( array );
			Assert.Is_False( array.Length == 0 );
			Assert.Is_True( valid_size >= 0 );
			Assert.Is_True( valid_size <= array.Length );

			var temp_array = array;

			if ( valid_size < Parallel_Threshold )
			{
				for ( var i = 0 ; i < valid_size ; i++ ) temp_array[ i ] = default!;
			}
			else Parallel.For( 0, valid_size, i => { temp_array[ i ] = default!; } );

			Array.Copy( temp_array, array, array.Length );
		}
	}
}
