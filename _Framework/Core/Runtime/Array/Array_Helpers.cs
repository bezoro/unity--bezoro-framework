using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bezoro.Framework.Core
{
	public static class Array_Helpers
	{
		static Array_Helpers()
		{
			Parallel_Threshold = 100;
		}

		public static int Parallel_Threshold;

		public static void Initialize_Array_If_Null<T>(ref T[] array, I_Logger logger = null)
		{
			if (array != null) return;

			array = Array.Empty<T>();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			logger?.Log("Initialized null array to an empty array.");
#endif
		}

		public static void Remove_Duplicates<T>(ref T[] array, I_Logger logger = null)
		{
			if (array == null)
			{
				logger?.Log_Warning("Array is null. Aborting remove operation.");
				return;
			}

			if (array.Length <= 1)
			{
				logger?.Log("Array has less than 2 elements. No duplicates to remove.");
				return;
			}

			var unique_elements = new List<T>();
			var seen_elements   = new HashSet<T>();

			foreach ( var element in array )
			{
				if (element == null) continue;
				if (!seen_elements.Add(element)) continue;
				unique_elements.Add(element);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
				logger?.Log($"Removing duplicate: {element}");
#endif
			}

			array = unique_elements.ToArray();
		}

		public static bool Compare_Arrays<T>(T[] a, T[] b, I_Logger logger = null)
		{
			if (a == null && b == null) return true;
			if (a == null || b == null) return false;
			if (a.Length != b.Length) return false;

			for (var i = 0 ; i < a.Length ; i++)
			{
				if (EqualityComparer<T>.Default.Equals(a[i], b[i])) continue;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
				logger?.Log($"Array comparison failed at index {i}.");
#endif
				return false;
			}
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			logger?.Log("Arrays are equal");
#endif
			return true;
		}

		public static void Add<T>(
			ref T[] array,
			T element_to_add,
			out int inserted_index,
			int resize_factor = 2,
			I_Logger logger = null
		)
		where T : class
		{
			if (element_to_add == null)
			{
				inserted_index = -1;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
				logger?.Log_Warning("Element is null. Add operation aborted.");
#endif
				return;
			}

			// Initialize array if it's null or empty
			if (array == null || array.Length == 0)
			{
				array          = new T[1];
				array[0]       = element_to_add;
				inserted_index = 0;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
				logger?.Log($"Array was null. Initialized and added element: {element_to_add}");
#endif
				return;
			}

			if (array.Length > Parallel_Threshold)
				Find_Element_Parallel(array, null, logger);
			else
				Find_Element_Sequentially(array, null, logger);

			// Find the first empty slot
			for (var i = 0 ; i < array.Length ; i++)
			{
				if (array[i] != null) continue;

				array[i]       = element_to_add;
				inserted_index = i;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
				logger?.Log($"Added element: {element_to_add} at index {i}");
#endif
				return;
			}

			// No empty slot, resize the array
			var original_size = array.Length;
			var new_size      = array.Length * resize_factor;
			Array.Resize(ref array, new_size);

			// Add the element in the first empty slot of the resized array
			array[original_size] = element_to_add;
			inserted_index       = original_size;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			logger?.Log($"Resized array to {new_size}. Added element: {element_to_add} at index {inserted_index}");
#endif
		}

		public static void Add<T>(ref T[] array, T element, int resize_factor = 2, I_Logger logger = null)
		where T : class
		{
			Add(ref array, element, out _, resize_factor, logger);
		}

		public static void Resize_By_Factor<T_Array>(ref T_Array[] array, int factor, I_Logger logger)
		{
			if (factor <= 1)
			{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
				logger?.Log_Warning($"Factor: {factor} is invalid. Array size will not be multiplied.");
#endif
				return;
			}

			Array.Resize(ref array, array.Length * factor);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			logger?.Log($"Array size resized by factor: {factor}");
#endif
		}

		public static void Add_Unique<T>(
			ref T[] array,
			T element,
			out int index,
			int resize_factor = 2,
			I_Logger logger = null
		)
		where T : class
		{
			if (element == null)
			{
				logger?.Log_Warning("Element is null. Add operation aborted.");
				index = -1;
				return;
			}

			if (array == null || array.Length == 0)
			{
				Create_Array_And_Add_First_Element_If_Needed(ref array, element, logger);
				index = 0;
				return;
			}

			bool element_exists;

			if (array.Length > Parallel_Threshold)
				element_exists = Find_Element_Parallel(array, element, logger).Relevant_Index >= 0;
			else
				element_exists = Find_Element_Sequentially(array, element, logger).Relevant_Index >= 0;

			if (element_exists)
			{
				index = -1;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
				logger?.Log($"Element {element} already exists in array. Add operation aborted.");
#endif
				return;
			}

			Add(ref array, element, out var i, resize_factor);
			index = i;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			logger?.Log_Success($"Adding unique element: {element}");
#endif
		}

		public static Array_Element_Info<T> Find_Element_Sequentially<T>(
			T[] array,
			T element_to_find,
			I_Logger logger = null
		)
		{
			if (array == null || array.Length == 0)
			{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
				logger?.Log_Warning("Array is null or empty. Aborting search.");
#endif
				return new( -1, element_to_find, array?.Length ?? 0 );
			}

			if (element_to_find == null)
			{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
				logger?.Log_Warning("Element to find is null. Aborting search.");
#endif
				return new( -1, default, array.Length );
			}

			for (var i = 0 ; i < array.Length ; i++)
			{
				if (!array[i].Equals(element_to_find)) continue;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
				logger?.Log($"Element {element_to_find} found at index {i}");
#endif
				return new( i, element_to_find, array.Length );
			}

#if UNITY_EDITOR || DEVELOPMENT_BUILD
			logger?.Log($"Element {element_to_find} not found in array.");
#endif
			return new( -1, element_to_find, array.Length );
		}

		public static Array_Element_Info<T> Find_Element_Parallel<T>(
			T[] array,
			T element_to_find,
			I_Logger logger = null
		)
		{
			if (array == null || array.Length == 0)
			{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
				logger?.Log_Warning("Array is null or empty. Aborting search.");
#endif
				return new( -1, element_to_find, array?.Length ?? 0 );
			}

			if (element_to_find == null)
			{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
				logger?.Log_Warning("Element to find is null. Aborting search.");
#endif
				return new( -1, default, array.Length );
			}

			var result_index = -1;
			var locker       = new object();

			Parallel.For(
				0, array.Length, (i, state) =>
				{
					if (array[i]?.Equals(element_to_find) != true) return;

					lock ( locker )
					{
						result_index = i;
						state.Stop();
					}
				}
			);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
			logger?.Log(
				result_index != -1
					? $"Element {element_to_find} found at index {result_index}."
					: $"Element {element_to_find} not found in array."
			);
#endif
			return new( result_index, element_to_find, array.Length );
		}

		public static bool Create_Array_And_Add_First_Element_If_Needed<T>(
			ref T[] array,
			T element,
			I_Logger logger = null
		)
		{
			Assert.Is_Not_Null(element);

			if (array != null && array.Length != 0) return false;

			array    = new T[1];
			array[0] = element;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			logger?.Log($"Creating array and adding first element: {element}");
#endif
			return true;
		}

		public static void Merge<T>(T[] from, ref T[] to)
		where T : class
		{
			if (from == null) return;
			if (from.Length == 0) return;

			if (to == null || to.Length == 0)
			{
				Array.Resize(ref to, from.Length);
				Copy(from, ref to);
				return;
			}

			if (to.Length < from.Length)
			{
				Array.Resize(ref to, from.Length);
				Copy(from, ref to);
				return;
			}

			for (var i = 0 ; i < from.Length ; i++)
			{
				if (to[i] != null) continue;

				to[i] = from[i];
			}
		}

		public static void Copy<T>(T[] from, ref T[] to)
		{
			Assert.Is_Not_Null(from);
			Assert.Is_False(from.Length == 0);

			if (to == null || to.Length < from.Length)
				to = new T[from.Length];

			Array.Copy(from, to, from.Length);
		}

		public static bool TryRemove<T>(ref T[] array, T element, out int index)
		where T : class
		{
			Assert.Is_Not_Null(array);
			Assert.Is_False(array.Length == 0);
			Assert.Is_Not_Null(element);

			var found_index = Array.IndexOf(array, element);

			if (found_index >= 0)
			{
				array[found_index] = null;
				index              = found_index;
				return true;
			}

			index = -1;
			return false;
		}

		public static void Clear<T_Array>(ref T_Array[] array)
		{
			Assert.Is_Not_Null(array);
			Assert.Is_False(array.Length == 0);

			var temp_array = new T_Array[array.Length];

			if (array.Length < Parallel_Threshold)
			{
				for (var i = 0 ; i < array.Length ; i++)
					temp_array[i] = default;
			}
			else Parallel.For(0, array.Length, i => { temp_array[i] = default; });

			Array.Copy(temp_array, array, array.Length);
		}

		public static void Clear<T_Array_Type>(T_Array_Type[] array)
		{
			Assert.Is_Not_Null(array);
			Assert.Is_False(array.Length == 0);

			if (array.Length < Parallel_Threshold)
			{
				for (var i = 0 ; i < array.Length ; i++) array[i] = default;
			}
			else Parallel.For(0, array.Length, i => { array[i] = default; });
		}

		public static void Clear<T>(ref T[] array, int valid_size)
		{
			Assert.Is_Not_Null(array);
			Assert.Is_False(array.Length == 0);
			Assert.Is_True(valid_size >= 0);
			Assert.Is_True(valid_size <= array.Length);

			var temp_array = array;

			if (valid_size < Parallel_Threshold)
			{
				for (var i = 0 ; i < valid_size ; i++)
					temp_array[i] = default!;
			}
			else
			{
				Parallel.For(
					0, valid_size, i =>
					{
						temp_array[i] = default!;
					}
				);
			}

			Array.Copy(temp_array, array, array.Length);
		}

		public static void Remove_Element<T>(
			ref T[] array,
			T element,
			Array_Removal_Approach removal_approach = Array_Removal_Approach.Resize,
			I_Logger logger = null
		)
		{
			// Validate input
			if (array == null || array.Length == 0)
			{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
				logger?.Log_Warning("Array is null or empty. Aborting remove operation.");
#endif
				return;
			}

			if (element == null)
			{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
				logger?.Log_Warning("Element is null. Aborting remove operation.");
#endif
				return;
			}

			// Decide on the removal method based on the size of the array
			if (array.Length > Parallel_Threshold)
			{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
				logger?.Log(
					$"Array size is above the parallel threshold ({Parallel_Threshold}). Using parallel removal."
				);
#endif

				Remove_Element_Parallel(ref array, element, removal_approach, logger);
			}
			else
			{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
				logger?.Log(
					$"Array size is below the parallel threshold ({Parallel_Threshold}). Using sequential removal."
				);
#endif

				Remove_Element_Sequential(ref array, element, removal_approach, logger);
			}
		}

		public static void Add_Unique<T>(ref T[] array, T element)
		where T : class
		{
			Add_Unique(ref array, element, out _);
		}

		private static void Resize_By_Factor_And_Add_Element<T>(
			ref T[] array,
			T element,
			out int index,
			out int valid_size,
			int resize_factor,
			I_Logger logger
		)
		where T : class
		{
			Resize_By_Factor_And_Add_Element(ref array, element, out index, resize_factor, logger);
			valid_size = array.Length;
		}

		private static void Resize_By_Factor_And_Add_Element<T>(
			ref T[] array,
			T element,
			out int index,
			int resize_factor,
			I_Logger logger
		) where T : class
		{
			var old_length = array.Length;
			Resize_By_Factor(ref array, resize_factor, logger);
			array[old_length] = element;
			index             = old_length;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			logger?.Log($"Array was full, resized by{resize_factor} and added element: {element}");
#endif
		}

		private static void Remove_Element_Sequential<T>(
			ref T[] array,
			T element,
			Array_Removal_Approach removal_approach,
			I_Logger logger = null
		)
		{
			// Find the index of the element
			var index = Array.IndexOf(array, element);

			if (index < 0)
			{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
				logger?.Log_Warning("Element not found. No operation performed.");
#endif
				return;
			}

			// Handle removal approach
			switch ( removal_approach )
			{
				case Array_Removal_Approach.Resize:
					// Shift elements and resize the array
					for (var i = index ; i < array.Length - 1 ; i++)
						array[i] = array[i + 1];

					Array.Resize(ref array, array.Length - 1);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
					logger?.Log($"Element '{element}' removed sequentially. Array resized.");
#endif
					break;

				case Array_Removal_Approach.Set_To_Null:
					// Nullify the element
					array[index] = default;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
					logger?.Log($"Element '{element}' removed sequentially by setting it to null at index {index}.");
#endif
					break;

				default:
#if UNITY_EDITOR || DEVELOPMENT_BUILD
					logger?.Log_Warning($"Unhandled Array_Removal_Approach: {removal_approach}");
#endif
					break;
			}
		}

		private static Array_Element_Info<T> Remove_Element_Parallel<T>(
			ref T[] array,
			T element,
			Array_Removal_Approach removal_approach,
			I_Logger logger = null
		)
		{
			var found_index = -1;
			var lock_obj    = new object();

			var local_array = array;

			Parallel.For(
				0, local_array.Length, (i, state) =>
				{
					if (!EqualityComparer<T>.Default.Equals(local_array[i], element)) return;

					lock ( lock_obj )
					{
						if (found_index != -1) return;

						found_index = i;
						state.Stop();
					}
				}
			);

			if (found_index == -1)
			{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
				logger?.Log_Warning("Element not found in array. No operation performed.");
#endif
				return new( -1, element, array.Length );
			}

#if UNITY_EDITOR || DEVELOPMENT_BUILD
			logger?.Log($"Element '{element}' found at index {found_index} using parallel search.");
#endif

			switch ( removal_approach )
			{
				case Array_Removal_Approach.Resize:
					Resize_Array_Parallel(ref array, found_index, logger);
					break;

				case Array_Removal_Approach.Set_To_Null:
					lock ( lock_obj )
					{
						array[found_index] = default;
					}

#if UNITY_EDITOR || DEVELOPMENT_BUILD
					logger?.Log(
						$"Element '{element}' removed by setting it to null at index {found_index} in parallel."
					);
#endif

					break;

				default:
#if UNITY_EDITOR || DEVELOPMENT_BUILD
					logger?.Log_Warning($"Unhandled Array_Removal_Approach: {removal_approach}");
#endif
					break;
			}

			return new( found_index, element, array.Length );
		}

		private static void Resize_Array_Parallel<T>(
			ref T[] array,
			int index,
			I_Logger logger = null
		)
		{
			// Create a new array with one less element
			var temp_array  = new T[array.Length - 1];
			var local_array = array;

			var copy_lock = new object();

			// Copy elements into the new array in parallel
			Parallel.For(
				0, local_array.Length, i =>
				{
					if (i < index)
						temp_array[i] = local_array[i];
					else if (i > index)
						temp_array[i - 1] = local_array[i];
				}
			);

			lock ( copy_lock )
			{
				array = temp_array;
			}

#if UNITY_EDITOR || DEVELOPMENT_BUILD
			logger?.Log($"Array resized in parallel. Element at index {index} removed.");
#endif
		}
	}
}
