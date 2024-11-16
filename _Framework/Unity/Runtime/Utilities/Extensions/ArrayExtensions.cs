using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Bezoro.Framework.Unity.Utilities.Helpers;
using Cysharp.Threading.Tasks;
using Object = UnityEngine.Object;

namespace Bezoro.Framework.Unity.Utilities
{
	public static class ArrayExtensions
	{
		public static bool IsNull<T>( this T[] array ) => array == null;

		public static bool IsNotNull<T>( this T[] array ) => array != null;

		public static bool IsEmpty<T>( this T[] array ) => array.Length == 0;

		public static bool IsNotEmpty<T>( this T[] array ) => array.Length > 0;

		public static bool IsNullOrEmpty<T>( this T[] array ) => array.IsNull() || array.IsEmpty();

		public static bool IsNotNullOrEmpty<T>( this T[] array ) => array.IsNotNull() || array.IsNotEmpty();

		public static bool IsUnityObject<T>( this T[] array, out T[] outUnityArray )
		{
			if ( array is object[] unityArray )
			{
				outUnityArray = unityArray as T[];

				return true;
			}

			outUnityArray = null;

			return false;
		}

		public static bool IsNotUnityObject<T>( this T[] array ) => array is not Object[];

		public static bool Contains<T>
			( this T[] array, T element, out int index, bool safetyChecks = true )
		where T : class
		{
			if ( safetyChecks )
			{
				if ( element.EqualsNull() )
				{
					index = -1;

					return false;
				}

				if ( array.IsNullOrEmpty() )
				{
					index = -1;

					return false;
				}
			}

			if ( array.IsUnityObject( out var unityArray ) && element.IsUnityObject( out var unityElement ) )
			{
				for ( var i = 0 ; i < unityArray.Length ; i++ )
				{
					if ( unityArray[i] != unityElement )
					{
						continue;
					}

					index = i;

					return true;
				}
			}
			else
			{
				for ( var i = 0 ; i < array.Length ; i++ )
				{
					if ( array[i] != element )
					{
						continue;
					}

					index = i;

					return true;
				}
			}

			index = -1;

			return false;
		}

		public static bool Contains<T>
			( this T[] array, T element, int validSize, out int index, bool safetyChecks = true )
		where T : class
		{
			if ( safetyChecks )
			{
				if ( validSize <= 0 )
				{
					index = -1;

					return false;
				}

				if ( element.EqualsNull() )
				{
					index = -1;

					return false;
				}

				if ( array.IsNullOrEmpty() )
				{
					index = -1;

					return false;
				}
			}

			if ( array.IsUnityObject( out var unityArray ) && element.IsUnityObject( out var unityElement ) )
			{
				for ( var i = 0 ; i < validSize ; i++ )
				{
					if ( unityArray[i] != unityElement )
					{
						continue;
					}

					index = i;

					return true;
				}
			}
			else
			{
				for ( var i = 0 ; i < validSize ; i++ )
				{
					if ( array[i] != element )
					{
						continue;
					}

					index = i;

					return true;
				}
			}

			index = -1;

			return false;
		}

		public static bool ContainsStruct<T>
			( this T[] array, T element, out int index, bool safetyChecks = true )
		where T : struct, IEquatable<T>
		{
			if ( safetyChecks )
			{
				if ( array.IsNullOrEmpty() )
				{
					index = -1;

					return false;
				}
			}

			for ( var i = 0 ; i < array.Length ; i++ )
			{
				if ( !array[i].Equals( element ) )
				{
					continue;
				}

				index = i;

				return true;
			}

			index = -1;

			return false;
		}

		public static int CountEmptyIndices<T>( this T[] array, bool safetyChecks = true )
		where T : class
		{
			if ( safetyChecks )
			{
				if ( array.IsNullOrEmpty() )
				{
					return -1;
				}
			}

			var emptyIndicesCount = 0;
			if ( array.IsUnityObject( out var unityArray ) )
			{
				for ( var i = 0 ; i < unityArray.Length ; i++ )
				{
					if ( unityArray[i].EqualsNull() )
					{
						emptyIndicesCount++;
					}
				}
			}
			else
			{
				for ( var i = 0 ; i < array.Length ; i++ )
				{
					if ( array[i].EqualsNull() )
					{
						emptyIndicesCount++;
					}
				}
			}

			return emptyIndicesCount;
		}

		public static int CountEmptyIndices<T>
			( this T[] array, int validSize, bool safetyChecks = true )
		where T : class
		{
			if ( safetyChecks )
			{
				if ( validSize <= 0 )
				{
					return -1;
				}

				if ( array.IsNullOrEmpty() )
				{
					return -1;
				}

				if ( validSize > array.Length )
				{
					return -1;
				}
			}

			return array.Length - validSize;
		}

		public static int CountFilledIndices<T>( this T[] array, bool safetyChecks = true )
		where T : class
		{
			if ( safetyChecks )
			{
				if ( array.IsNullOrEmpty() )
				{
					return -1;
				}
			}

			var filledIndicesCount = 0;
			if ( array.IsUnityObject( out var unityArray ) )
			{
				for ( var i = 0 ; i < unityArray.Length ; i++ )
				{
					if ( unityArray[i].NotEqualsNull() )
					{
						filledIndicesCount++;
					}
				}
			}
			else
			{
				for ( var i = 0 ; i < array.Length ; i++ )
				{
					if ( array[i].NotEqualsNull() )
					{
						filledIndicesCount++;
					}
				}
			}

			return filledIndicesCount;
		}

		/// <summary>
		///     Removes all null elements from the array and returns a new array containing only non-null elements.
		/// </summary>
		/// <typeparam name="T">The type of elements in the array.</typeparam>
		/// <param name="array">The array to trim.</param>
		/// <returns>A new array containing only non-null elements.</returns>
		public static T[] Trim<T>( this T[] array )
		{
			if ( array == null ) { return Array.Empty<T>(); }

			var count = array.Count( x => x != null );
			if ( count == array.Length ) { return array; }

			var result = new T[count];
			var index = 0;

			foreach ( var t in array )
			{
				if ( t != null ) { result[index++] = t; }
			}

			return result;
		}

		/// <summary>
		///     Clears all elements in the array by setting them to their default values.
		/// </summary>
		/// <typeparam name="T">The type of elements in the array.</typeparam>
		/// <param name="array">The array to clear.</param>
		public static void Clear<T>( this T[] array )
		{
			if ( array == null )
			{
				return;
			}

			Array.Clear( array, 0, array.Length );
		}

		/// <summary>
		///     Clears all elements in the 2D array by setting them to their default values.
		/// </summary>
		/// <typeparam name="T">The type of elements in the array.</typeparam>
		/// <param name="array">The array to clear.</param>
		public static void Clear<T>( this T[,] array )
		{
			if ( array == null )
			{
				return;
			}

			Array.Clear( array, 0, array.Length );
		}

		/// <summary>
		///     Processes all elements of a 2D array in their order of appearance.
		/// </summary>
		/// <param name="array">The array to process.</param>
		/// <param name="processFunc">
		///     A function that takes an element of the array, its x- and y-coordinates,
		///     and a <see cref="CancellationToken" />, and returns a <see cref="UniTask" />.
		/// </param>
		/// <param name="cancellationToken">
		///     A <see cref="CancellationToken" /> that can be used to cancel the processing.
		/// </param>
		/// <returns>
		///     A <see cref="UniTask" /> that represents the asynchronous operation.
		/// </returns>
		public static async UniTask Process<T>
		(
			this T[,] array,
			Func<T, int, int, CancellationToken, UniTask> processFunc,
			CancellationToken cancellationToken
		)
		{
			var rows = array.GetLength( 0 );
			var cols = array.GetLength( 1 );

			for ( var y = 0 ; y < rows ; y++ )
			{
				for ( var x = 0 ; x < cols ; x++ )
				{
					if ( cancellationToken.IsCancellationRequested )
					{
						return;
					}

					if ( array[y, x] == null )
					{
						continue;
					}

					await processFunc( array[y, x], x, y, cancellationToken );
					await UniTask.Yield();
				}
			}
		}

		/// <summary>
		///     Checks if an array is null.
		/// </summary>
		/// <param name="array">The array to check.</param>
		/// <param name="equalityType">The equality type to use.</param>
		/// <returns>true if the array is null, false otherwise.</returns>
		/// <remarks>
		///     The equality type can be specified as an optional parameter. If
		///     <paramref name="equalityType" /> is <see cref="EqualityType.Equal" />,
		///     the method checks if the array is null by using the equality
		///     operator (<c>==</c>). If it is <see cref="EqualityType.Is" />, the
		///     method checks if the array is null by using the <c>is</c> operator.
		/// </remarks>
		public static bool IsNull<T>( this T[] array, EqualityType equalityType = EqualityType.Equal )
		{
			switch ( equalityType )
			{
				case EqualityType.None: return false;
				case EqualityType.Equal: return array == null;
				case EqualityType.Is: return array is null;
				default: throw new ArgumentOutOfRangeException( nameof(equalityType), equalityType, null );
			}
		}

		/// <summary>
		///     Checks if an array is empty.
		/// </summary>
		/// <param name="array">The array to check.</param>
		/// <typeparam name="T">The type of the array elements.</typeparam>
		/// <param name="equalityType">The equality type to use.</param>
		/// <returns>true if the array is empty, false otherwise.</returns>
		/// <remarks>
		///     The equality type can be specified as an optional parameter. If
		///     <paramref name="equalityType" /> is <see cref="EqualityType.Equal" />,
		///     the method checks if the array is empty by using the equality operator
		///     (<c>==</c>). If it is <see cref="EqualityType.Is" />, the method
		///     checks if the array is empty by using the <c>is</c> operator.
		/// </remarks>
		public static bool IsEmpty<T>( this T[] array, EqualityType equalityType = EqualityType.Equal )
		{
			if ( array.IsNull( equalityType ) )
			{
				return false;
			}

			switch ( equalityType )
			{
				case EqualityType.None: return false;
				case EqualityType.Equal: return array.Length == 0;
				case EqualityType.Is: return array.Length is 0;
				default: throw new ArgumentOutOfRangeException( nameof(equalityType), equalityType, null );
			}
		}

		/// <summary>
		///     Checks if an array is null or empty.
		/// </summary>
		/// <param name="array">The array to check.</param>
		/// <typeparam name="T">The type of the array elements.</typeparam>
		/// <param name="equalityType">The equality type to use.</param>
		/// <returns>true if the array is null or empty, false otherwise.</returns>
		/// <remarks>
		///     The equality type can be specified as an optional parameter. If
		///     <paramref name="equalityType" /> is <see cref="EqualityType.Equal" />,
		///     the method checks if the array is null or empty by using the equality operator
		///     (<c>==</c>). If it is <see cref="EqualityType.Is" />, the method
		///     checks if the array is null or empty by using the <c>is</c> operator.
		/// </remarks>
		public static bool IsNullOrEmpty<T>( this T[] array, EqualityType equalityType = EqualityType.Equal )
		{
			switch ( equalityType )
			{
				case EqualityType.None: return false;
				case EqualityType.Equal:
				case EqualityType.Is:
					return array.IsNull( equalityType ) || array.IsEmpty( equalityType );
				default: throw new ArgumentOutOfRangeException( nameof(equalityType), equalityType, null );
			}
		}

		/// <summary>
		///     Checks if two arrays are equal.
		/// </summary>
		/// <param name="array">The first array to compare.</param>
		/// <param name="b">The second array to compare.</param>
		/// <param name="equalityType">The equality type to use.</param>
		/// <returns>true if the arrays are equal, false otherwise.</returns>
		/// <remarks>
		///     The equality type can be specified as an optional parameter. If
		///     <paramref name="equalityType" /> is <see cref="EqualityType.Equal" />,
		///     the method checks if the arrays are equal by using the equality
		///     operator (<c>==</c>). If it is <see cref="EqualityType.Is" />, the
		///     method checks if the arrays are equal by using the <c>is</c>
		///     operator.
		/// </remarks>
		public static bool ArraysAreEqual<T>( this T[] array, T[] b, EqualityType equalityType = EqualityType.Equal )
		{
			if ( array.IsNull( equalityType ) )
			{
				return b.IsNull( equalityType );
			}

			if ( b.IsNull( equalityType ) )
			{
				return false;
			}

			return array.Length == b.Length && array.SequenceEqual( b );
		}

		public static T[] AddRange<T>( this T[] array, T[] items )
		{
			if ( items.IsNullOrEmpty() )
			{
				return array;
			}

			var newLength = array.Length + items.Length;
			var newArray = new T[newLength];
			Array.Copy( array, newArray, array.Length );
			Array.Copy( items, 0, newArray, array.Length, items.Length );

			return newArray;
		}

		/// <summary>
		///     Concatenates a variable number of arrays to a single array.
		/// </summary>
		/// <param name="array">The first array to concatenate.</param>
		/// <param name="arrays">The arrays to concatenate.</param>
		/// <returns>The concatenated array.</returns>
		public static T[] Concat<T>( this T[] array, params T[][] arrays )
		{
			if ( arrays.IsNullOrEmpty() || arrays.All( a => a.IsNullOrEmpty() ) )
			{
				return array ?? Array.Empty<T>();
			}

			var result = new T[array?.Length ?? 0];
			var index = 0;

			if ( array != null )
			{
				Array.Copy( array, 0, result, index, array.Length );
				index += array.Length;
			}

			foreach ( var arr in arrays )
			{
				if ( arr == null )
				{
					continue;
				}

				Array.Copy( arr, 0, result, index, arr.Length );
				index += arr.Length;
			}

			return result;
		}

		/// <summary>
		///     Returns a string representation of the array, with each element name
		///     formatted according to the type of the element.
		/// </summary>
		/// <param name="array">The array to format.</param>
		/// <returns>A string representation of the array.</returns>
		/// <remarks>
		///     The format of the string is "[item1, item2, ...]". If the element is
		///     a <see cref="string" />, it is enclosed in double quotes. If the
		///     element is <see langword="null" />, it is represented as "null".
		///     Otherwise, the element is represented by its result of calling
		///     <see cref="object.ToString()" />.
		/// </remarks>
		public static string ToStringFormat<T>( this T[] array )
		{
			if ( array == null )
			{
				return "null";
			}

			if ( array.Length == 0 )
			{
				return "[]";
			}

			return "[" + string.Join( ", ", array.Select( ArrayHelpers.GetElementName ).ToString().Italic() ) + "]";
		}

		/// <summary>
		///     Checks if a 2D array is null or empty.
		/// </summary>
		/// <typeparam name="T">The type of elements in the array.</typeparam>
		/// <param name="array">The 2D array to check.</param>
		/// <returns>True if the array is null or empty, false otherwise.</returns>
		public static bool IsNullOrEmpty<T>( this T[,] array ) => array == null || array.Length == 0;

		public static T[] Add<T>( this T[] array, T item )
		{
			ValidationHelpers.ValidateObjectNotNull( array );
			ValidationHelpers.ValidateObjectNotNull( item );

			var currentLength = array.Length;

			for ( var i = 0 ; i < currentLength ; i++ )
			{
				if ( !EqualityComparer<T>.Default.Equals( array[i], default( T ) ) )
				{
					continue;
				}

				array[i] = item;

				return array;
			}

			int newCapacity;

			if ( currentLength < 8 )
			{
				newCapacity = currentLength * 2;
			}
			else if ( currentLength < 64 )
			{
				newCapacity = (int)( currentLength * 1.5 );
			}
			else
			{
				newCapacity = (int)( currentLength * 1.25 );
			}

			var newArray = new T[newCapacity];
			Array.Copy( array, newArray, currentLength );
			newArray[currentLength] = item;

			return newArray;
		}

		/// <summary>
		///     Removes an item from the array and returns a new array with the remaining elements.
		/// </summary>
		/// <typeparam name="T">The type of elements in the array.</typeparam>
		/// <param name="array">The array to remove from.</param>
		/// <param name="item">The item to remove.</param>
		/// <returns>A new array with the item removed.</returns>
		public static T[] Remove<T>( this T[] array, T item )
		{
			ValidationHelpers.ValidateObjectNotNull( array );
			ValidationHelpers.ValidateObjectNotNull( item );

			var currentLength = array.Length;
			var newArray = new T[currentLength - 1];
			var newIndex = 0;
			var found = false;

			for ( var i = 0 ; i < currentLength ; i++ )
			{
				if ( !found && EqualityComparer<T>.Default.Equals( array[i], item ) )
				{
					found = true;

					continue;
				}

				if ( newIndex >= newArray.Length )
				{
					return array;
				}

				newArray[newIndex] = array[i];
				newIndex++;
			}

			return found ? newArray : array;
		}
	}

	public enum EqualityType
	{
		None = -1,
		Equal,
		Is
	}

	public enum InequalityType
	{
		None = -1,
		NotEqual,
		Not
	}
}
