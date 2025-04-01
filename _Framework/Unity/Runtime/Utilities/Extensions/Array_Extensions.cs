using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Bezoro.Framework.Core;
using Bezoro.Framework.Unity.Utilities.String;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Array = System.Array;
using Object = UnityEngine.Object;

namespace Bezoro.Framework.Unity.Utilities
{
	public static class Array_Extensions
	{
		/// <summary>
		/// Validates whether all elements in the array are UnityEngine.Object instances.
		/// </summary>
		/// <typeparam name="T">The type of elements in the array (must be a reference type)</typeparam>
		/// <param name="array">The array to validate</param>
		/// <param name="unityArray">When this method returns, contains the original array if validation succeeds, or null if validation fails</param>
		/// <returns>true if all elements are Unity objects; otherwise, false</returns>
		public static bool Is_Unity_Object<T>(this T[] array, out T[] unityArray)
		where T : class
		{
			unityArray = null;

			// Early return for null or empty arrays
			if (array == null || array.Length == 0)
				return false;

			// Cache type check result to avoid repeated IsAssignableFrom calls
			var is_unity_object_type = typeof(Object).IsAssignableFrom(typeof(T));

			if (!is_unity_object_type)
				return false;

			// If T is a Unity object type, we don't need additional runtime checks
			if (typeof(T).IsSubclassOf(typeof(Object)))
			{
				unityArray = array;
				return true;
			}

			// For interface types or classes that might be Unity objects, check each element
			foreach ( var element in array )
			{
				if (element != null && element is not Object)
					return false;
			}

			unityArray = array;
			return true;
		}

		/// <summary>
		/// Checks if the array is not a Unity Object array.
		/// </summary>
		/// <typeparam name="T">The type of elements in the array.</typeparam>
		/// <param name="array">The array to check.</param>
		/// <returns>True if the array is not a Unity Object array, false otherwise.</returns>
		public static bool Is_Not_Unity_Object<T>(this T[] array)
		where T : class
		{
			return !array.Is_Unity_Object(out _);
		}

		/// <summary>
		/// Searches for the specified element in the array and returns its index.
		/// </summary>
		/// <typeparam name="T">The type of elements in the array. Must be a reference type.</typeparam>
		/// <param name="array">The array to search in.</param>
		/// <param name="element">The element to find.</param>
		/// <param name="index">When this method returns, contains the index of the found element,
		/// or -1 if the element was not found.</param>
		/// <returns>true if the element was found; otherwise, false.</returns>
		/// <exception cref="ArgumentNullException">Thrown when array is null.</exception>
		public static bool Contains<T>(this T[] array, T element, out int index)
		where T : class
		{
			// Validate input parameters
			if (array == null)
				throw new ArgumentNullException(nameof(array));

			// Initialize output parameter
			index = -1;

			// Handle null element case - can't find null in array
			if (element == null)
				return false;

			// Check if we're dealing with Unity Objects
			var is_unity_object = array.Is_Unity_Object(out _) &&
								  element.Is_Unity_Object(out _);

			// Single loop implementation that handles both cases
			for (var i = 0 ; i < array.Length ; i++)
			{
				bool is_match;

				if (is_unity_object)
				{
					// Use Unity's equality comparison for Unity objects
					is_match = array[i] == element;
				}
				else
				{
					// Use regular reference equality for non-Unity objects
					is_match = array[i]?.Equals(element) ?? false;
				}

				if (is_match)
				{
					index = i;
					return true;
				}
			}

			return false;
		}

		public static bool Contains_Struct<T>(this T[] array, T element, out int index, bool safetyChecks = true)
		where T : struct, IEquatable<T>
		{
			if (safetyChecks)
			{
				if (array.Is_Null_Or_Empty())
				{
					index = -1;
					return false;
				}
			}

			for (var i = 0 ; i < array.Length ; i++)
			{
				if (!array[i].Equals(element))
					continue;

				index = i;
				return true;
			}

			index = -1;
			return false;
		}

		public static int Count_Empty_Indices<T>(this T[] array, bool safetyChecks = true)
		where T : class
		{
			if (safetyChecks)
			{
				if (array.Is_Null_Or_Empty())
					return -1;
			}

			var empty_indices_count = 0;

			if (array.Is_Unity_Object(out var unity_array))
			{
				for (var i = 0 ; i < unity_array.Length ; i++)
				{
					if (unity_array[i].Equals_Null())
						empty_indices_count++;
				}
			}
			else
			{
				for (var i = 0 ; i < array.Length ; i++)
				{
					if (array[i].Equals_Null())
						empty_indices_count++;
				}
			}

			return empty_indices_count;
		}

		public static int Count_Empty_Indices<T>(this T[] array, int validSize, bool safetyChecks = true)
		where T : class
		{
			if (safetyChecks)
			{
				if (validSize <= 0)
				{
					return -1;
				}

				if (array.Is_Null_Or_Empty())
				{
					return -1;
				}

				if (validSize > array.Length)
				{
					return -1;
				}
			}

			return array.Length - validSize;
		}

		public static int CountFilledIndices<T>(this T[] array, bool safetyChecks = true)
		where T : class
		{
			if (safetyChecks)
			{
				if (array.Is_Null_Or_Empty())
				{
					return -1;
				}
			}

			var filledIndicesCount = 0;

			if (array.Is_Unity_Object(out var unityArray))
			{
				for (var i = 0 ; i < unityArray.Length ; i++)
				{
					if (unityArray[i].Not_Equals_Null())
					{
						filledIndicesCount++;
					}
				}
			}
			else
			{
				for (var i = 0 ; i < array.Length ; i++)
				{
					if (array[i].Not_Equals_Null())
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
		public static T[] Trim<T>(this T[] array)
		{
			if (array == null) { return Array.Empty<T>(); }

			var count = array.Count(x => x != null);

			if (count == array.Length) { return array; }

			var result = new T[count];
			var index  = 0;

			foreach ( var t in array )
			{
				if (t != null) { result[index++] = t; }
			}

			return result;
		}

		/// <summary>
		///     Clears all elements in the array by setting them to their default values.
		/// </summary>
		/// <typeparam name="T">The type of elements in the array.</typeparam>
		/// <param name="array">The array to clear.</param>
		public static void Clear<T>(this T[] array)
		{
			if (array == null) return;

			Array.Clear(array, 0, array.Length);
		}

		/// <summary>
		///     Clears all elements in the 2D array by setting them to their default values.
		/// </summary>
		/// <typeparam name="T">The type of elements in the array.</typeparam>
		/// <param name="array">The array to clear.</param>
		public static void Clear<T>(this T[,] array)
		{
			if (array == null) return;

			Array.Clear(array, 0, array.Length);
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
		public static async UniTask Process<T>(
			this T[,] array,
			Func<T, int, int, CancellationToken, UniTask> processFunc,
			CancellationToken cancellationToken
		)
		{
			var rows = array.GetLength(0);
			var cols = array.GetLength(1);

			for (var y = 0 ; y < rows ; y++)
			{
				for (var x = 0 ; x < cols ; x++)
				{
					if (cancellationToken.IsCancellationRequested) return;

					if (array[y, x] == null) continue;

					await processFunc(array[y, x], x, y, cancellationToken);
					await UniTask.Yield();
				}
			}
		}

		/// <summary>
		/// Determines whether the specified array is null.
		/// </summary>
		/// <typeparam name="T">The type of elements in the array.</typeparam>
		/// <param name="array">The array to check.</param>
		/// <returns>true if the array is null; otherwise, false.</returns>
		/// <remarks>
		/// This method provides a clean, straightforward way to check if an array is null.
		/// It follows the principle of least surprise by having a single, clear way to perform the check.
		/// </remarks>
		public static bool IsNull<T>(this T[] array)
		{
			return array is null;
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
		///     <paramref name="equalityType" /> is <see cref="Equality_Type.Equal" />,
		///     the method checks if the array is empty by using the equality operator
		///     (<c>==</c>). If it is <see cref="Equality_Type.Is" />, the method
		///     checks if the array is empty by using the <c>is</c> operator.
		/// </remarks>
		public static bool Is_Empty<T>(this T[] array, Equality_Type equalityType = Equality_Type.Equal)
		{
			if (array.Is_Null())
			{
				return false;
			}

			switch ( equalityType )
			{
				case Equality_Type.None: return false;

				case Equality_Type.Equal: return array.Length == 0;

				case Equality_Type.Is: return array.Length is 0;

				default:
					throw new ArgumentOutOfRangeException(
						nameof(equalityType),
						equalityType,
						null
					);
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
		///     <paramref name="equalityType" /> is <see cref="Equality_Type.Equal" />,
		///     the method checks if the array is null or empty by using the equality operator
		///     (<c>==</c>). If it is <see cref="Equality_Type.Is" />, the method
		///     checks if the array is null or empty by using the <c>is</c> operator.
		/// </remarks>
		public static bool Is_Null_Or_Empty<T>(this T[] array, Equality_Type equalityType = Equality_Type.Equal)
		{
			switch ( equalityType )
			{
				case Equality_Type.None: return false;

				case Equality_Type.Equal:
				case Equality_Type.Is: return array.Is_Null() || array.Is_Empty(equalityType);

				default:
					throw new ArgumentOutOfRangeException(
						nameof(equalityType),
						equalityType,
						null
					);
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
		///     <paramref name="equalityType" /> is <see cref="Equality_Type.Equal" />,
		///     the method checks if the arrays are equal by using the equality
		///     operator (<c>==</c>). If it is <see cref="Equality_Type.Is" />, the
		///     method checks if the arrays are equal by using the <c>is</c>
		///     operator.
		/// </remarks>
		public static bool ArraysAreEqual<T>(this T[] array, T[] b, Equality_Type equalityType = Equality_Type.Equal)
		{
			if (array.Is_Null())
			{
				return b.Is_Null();
			}

			if (b.Is_Null())
			{
				return false;
			}

			return array.Length == b.Length && array.SequenceEqual(b);
		}

		public static T[] AddRange<T>(this T[] array, T[] items)
		{
			if (items.Is_Null_Or_Empty())
			{
				return array;
			}

			var newLength = array.Length + items.Length;
			var newArray  = new T[newLength];

			Array.Copy(
				array,
				newArray,
				array.Length
			);

			Array.Copy(
				items,
				0,
				newArray,
				array.Length,
				items.Length
			);

			return newArray;
		}

		/// <summary>
		///     Concatenates a variable number of arrays to a single array.
		/// </summary>
		/// <param name="array">The first array to concatenate.</param>
		/// <param name="arrays">The arrays to concatenate.</param>
		/// <returns>The concatenated array.</returns>
		public static T[] Concat<T>(this T[] array, params T[][] arrays)
		{
			if (arrays.Is_Null_Or_Empty() || arrays.All(a => a.Is_Null_Or_Empty()))
			{
				return array ?? Array.Empty<T>();
			}

			var result = new T[array?.Length ?? 0];
			var index  = 0;

			if (array != null)
			{
				Array.Copy(
					array,
					0,
					result,
					index,
					array.Length
				);

				index += array.Length;
			}

			foreach ( var arr in arrays )
			{
				if (arr == null)
				{
					continue;
				}

				Array.Copy(
					arr,
					0,
					result,
					index,
					arr.Length
				);

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
		public static string ToStringFormat<T>(this T[] array)
		{
			if (array == null)
			{
				return "null";
			}

			if (array.Length == 0)
			{
				return "[]";
			}

			return "[" + string.Join(
				", ",
				array.Select(Array_Helpers.Get_Element_Name).ToString().Italic()
			) + "]";
		}

		/// <summary>
		///     Checks if a 2D array is null or empty.
		/// </summary>
		/// <typeparam name="T">The type of elements in the array.</typeparam>
		/// <param name="array">The 2D array to check.</param>
		/// <returns>True if the array is null or empty, false otherwise.</returns>
		public static bool Is_Null_Or_Empty<T>(this T[,] array)
		{
			return array == null || array.Length == 0;
		}

		public static T[] Initialize_Null_Array_If_Necessary<T>(this T[] array)
		{
			return array ?? Array.Empty<T>();
		}

		public static T[] Add<T>(this T[] array, T item)
		{
			ArrayHelpers.Initialize_Null_Array(ref array);
			Validation_Helpers.Validate_Object_Not_Null(item);

			if (array.Length <= 0)
			{
				array    = new T[1];
				array[0] = item;
				Debug.Log($"Array initialized with 1 element: {item.GetType().Name}");
				return array;
			}

			var current_length = array.Length;

			for (var i = 0 ; i < current_length ; i++)
			{
				if (!EqualityComparer<T>.Default.Equals(array[i], default)) continue;

				array[i] = item;
				return array;
			}

			int new_capacity;

			if (current_length < 8)
				new_capacity = current_length * 2;
			else if (current_length < 64)
				new_capacity = (int)( current_length * 1.5 );
			else
				new_capacity = (int)( current_length * 1.25 );

			var new_array = new T[new_capacity];
			Array.Copy(array, new_array, current_length);
			new_array[current_length] = item;
			return new_array;
		}

		/// <summary>
		///     Removes an item from the array and returns a new array with the remaining elements.
		/// </summary>
		/// <typeparam name="T">The type of elements in the array.</typeparam>
		/// <param name="array">The array to remove from.</param>
		/// <param name="item">The item to remove.</param>
		/// <returns>A new array with the item removed.</returns>
		public static T[] Remove<T>(this T[] array, T item)
		{
			Validation_Helpers.Validate_Object_Not_Null(array);
			Validation_Helpers.Validate_Object_Not_Null(item);

			var current_length = array.Length;
			var new_array      = new T[current_length - 1];
			var new_index      = 0;
			var found          = false;

			for (var i = 0 ; i < current_length ; i++)
			{
				if (!found && EqualityComparer<T>.Default.Equals(array[i], item))
				{
					found = true;
					continue;
				}

				if (new_index >= new_array.Length) return array;

				new_array[new_index] = array[i];
				new_index++;
			}

			return found ? new_array : array;
		}
	}

	public enum Equality_Type
	{
		None = -1,
		Equal,
		Is
	}

	public enum Inequality_Type
	{
		None = -1,
		Not_Equal,
		Not
	}
}
