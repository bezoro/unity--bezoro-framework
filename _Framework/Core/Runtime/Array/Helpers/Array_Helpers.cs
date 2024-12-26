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

		private static void Clear_Array_Parallel<T>(ref T[] array, I_Logger logger)
		{
			var temp_array = new T[array.Length];

			Parallel.For(
				0, array.Length, i =>
				{
					temp_array[i] = default;
					Log_Helpers.Log(logger, $"Cleared array element at index {i}.");
				}
			);

			System.Array.Copy(temp_array, array, array.Length);
		}

		private static void Clear_Array_Elements_Sequential<T>(ref T[] array, I_Logger logger)
		{
			for (var i = 0 ; i < array.Length ; i++)
			{
				Log_Helpers.Log(logger, $"Cleared array element at index {i}.");
				array[i] = default;
			}
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
			Log_Helpers.Log(logger, $"Array was full, resized by{resize_factor} and added element: {element}");
		}

		private static void Remove_Element_Sequential<T>(
			ref T[] array,
			T element,
			Array_Removal_Approach removal_approach,
			I_Logger logger = null
		)
		{
			// Find the index of the element
			var index = System.Array.IndexOf(array, element);

			if (index < 0)
			{
				Log_Helpers.Log_Warning(logger, "Element not found. No operation performed.");
				return;
			}

			// Handle removal approach
			switch ( removal_approach )
			{
				case Array_Removal_Approach.Resize:
					// Shift elements and resize the array
					for (var i = index ; i < array.Length - 1 ; i++)
						array[i] = array[i + 1];

					System.Array.Resize(ref array, array.Length - 1);

					Log_Helpers.Log(logger, $"Element '{element}' removed sequentially. Array resized.");
					break;
				case Array_Removal_Approach.Set_To_Null:
					// Nullify the element
					array[index] = default;

					Log_Helpers.Log(
						logger, $"Element '{element}' removed sequentially by setting it to null at index {index}."
					);

					break;
				default:
					Log_Helpers.Log_Warning(logger, $"Unhandled Array_Removal_Approach: {removal_approach}");
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
				Log_Helpers.Log_Warning(logger, "Element not found in array. No operation performed.");
				return new( -1, element, array.Length );
			}

			Log_Helpers.Log(logger, $"Element '{element}' found at index {found_index} using parallel search.");

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

					Log_Helpers.Log(
						logger,
						$"Element '{element}' removed by setting it to null at index {found_index} in parallel."
					);

					break;

				default:
					Log_Helpers.Log_Warning(logger, $"Unhandled Array_Removal_Approach: {removal_approach}");
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
				array = temp_array;

			Log_Helpers.Log(logger, $"Array resized in parallel. Element at index {index} removed.");
		}

		public static void Initialize_Null_Array<T>(ref T[] array, I_Logger logger = null)
		{
			if (array != null) return;

			array = System.Array.Empty<T>();
			Log_Helpers.Log(logger, "Initialized null array to an empty array.");
		}

		public static void Remove_Duplicates<T>(ref T[] array, I_Logger logger = null)
		{
			if (array == null)
			{
				Log_Helpers.Log_Warning(logger, "Array is null. Aborting remove operation.");
				return;
			}

			if (array.Length <= 1)
			{
				Log_Helpers.Log(logger, "Array has less than 2 elements. No duplicates to remove.");
				return;
			}

			var unique_elements = new List<T>();
			var seen_elements   = new HashSet<T>();

			foreach ( var element in array )
			{
				if (element == null) continue;
				if (!seen_elements.Add(element)) continue;
				unique_elements.Add(element);
				Log_Helpers.Log(logger, $"Removing duplicate: {element}");
			}

			array = unique_elements.ToArray();
			Log_Helpers.Log_Success(logger, "Removed duplicates from array.");
		}

		public static bool Compare_Arrays<T>(T[] a, T[] b, I_Logger logger = null)
		{
			if (a == null && b == null) return true;
			if (a == null || b == null) return false;
			if (a.Length != b.Length) return false;

			for (var i = 0 ; i < a.Length ; i++)
			{
				if (EqualityComparer<T>.Default.Equals(a[i], b[i])) continue;

				Log_Helpers.Log(logger, $"Array comparison failed at index {i}.");
				return false;
			}

			Log_Helpers.Log(logger, "Arrays are equal");
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
				Log_Helpers.Log_Warning(logger, "Element is null. Add operation aborted.");
				return;
			}

			// Initialize array if it's null or empty
			if (array == null || array.Length == 0)
			{
				array          = new T[1];
				array[0]       = element_to_add;
				inserted_index = 0;
				Log_Helpers.Log_Success(logger, $"Array was null. Initialized and added element: {element_to_add}");
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
				Log_Helpers.Log_Success(logger, $"Added element: {element_to_add} at index {i}");
				return;
			}

			// No empty slot, resize the array
			var original_size = array.Length;
			var new_size      = array.Length * resize_factor;
			System.Array.Resize(ref array, new_size);

			// Add the element in the first empty slot of the resized array
			array[original_size] = element_to_add;
			inserted_index       = original_size;

			Log_Helpers.Log_Success(
				logger,
				$"Resized array to {new_size}. Added element: {element_to_add} at index {inserted_index}"
			);
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
				Log_Helpers.Log_Warning(logger, $"Factor: {factor} is invalid. Array size will not be multiplied.");
				return;
			}

			System.Array.Resize(ref array, array.Length * factor);
			Log_Helpers.Log_Success(logger, $"Array size resized by factor: {factor}");
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
				Setup_Array_With_First_Element(ref array, element, logger);
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
				Log_Helpers.Log_Warning(logger, $"Element {element} already exists in array. Add operation aborted.");
				return;
			}

			Add(ref array, element, out var i, resize_factor);
			index = i;
			Log_Helpers.Log_Success(logger, $"Adding unique element: {element}");
		}

		public static Array_Element_Info<T> Find_Element_Sequentially<T>(
			T[] array,
			T element_to_find,
			I_Logger logger = null
		)
		{
			if (array == null || array.Length == 0)
			{
				Log_Helpers.Log_Warning(logger, "Array is null or empty. Aborting search.");
				return new( -1, element_to_find, array?.Length ?? 0 );
			}

			if (element_to_find == null)
			{
				Log_Helpers.Log_Warning(logger, "Element to find is null. Aborting search.");
				return new( -1, default, array.Length );
			}

			for (var i = 0 ; i < array.Length ; i++)
			{
				if (!array[i].Equals(element_to_find)) continue;

				Log_Helpers.Log_Success(logger, $"Element {element_to_find} found at index {i}");
				return new( i, element_to_find, array.Length );
			}

			Log_Helpers.Log_Warning(logger, $"Element {element_to_find} not found in array.");
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
				Log_Helpers.Log_Warning(logger, "Array is null or empty. Aborting search.");
				return new( -1, element_to_find, array?.Length ?? 0 );
			}

			if (element_to_find == null)
			{
				Log_Helpers.Log_Warning(logger, "Element to find is null. Aborting search.");
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

			if (result_index != -1)
				Log_Helpers.Log_Success(logger, $"Element {element_to_find} found at index {result_index}.");
			else
				Log_Helpers.Log_Warning(logger, $"Element {element_to_find} not found in array.");

			return new( result_index, element_to_find, array.Length );
		}

		public static bool Setup_Array_With_First_Element<T>(
			ref T[] array,
			T element,
			I_Logger logger = null
		)
		{
			Assert.Is_Not_Null(element);

			if (array != null && array.Length != 0) return false;

			array    = new T[1];
			array[0] = element;
			Log_Helpers.Log(logger, $"Created array and adding first element: {element}");
			return true;
		}

		public static void Merge<T>(T[] source, ref T[] target, I_Logger logger = null)
		{
			if (source == null)
			{
				Log_Helpers.Log_Warning(logger, "Source array is null. Aborting merge operation.");
				return;
			}

			if (source.Length == 0)
			{
				Log_Helpers.Log_Warning(logger, "Source array is empty. Aborting merge operation.");
				return;
			}

			if (target == null || target.Length == 0)
			{
				System.Array.Resize(ref target, source.Length);
				Copy(source, ref target);

				Log_Helpers.Log_Success(
					logger,
					$"Target array is null or empty. Resized to {source.Length} and copied source array."
				);

				return;
			}

			if (target.Length < source.Length)
			{
				System.Array.Resize(ref target, source.Length);
				Copy(source, ref target);

				Log_Helpers.Log_Success(
					logger,
					$"Target array is smaller than source array. Resized to {source.Length} and copied source array."
				);

				return;
			}

			for (var i = 0 ; i < source.Length ; i++)
			{
				if (target[i] != null) continue;

				target[i] = source[i];
			}

			Log_Helpers.Log_Success(logger, "Source array merged into target array.");
		}

		public static void Copy<T>(T[] from, ref T[] to, I_Logger logger = null)
		{
			if (from == null)
			{
				Log_Helpers.Log_Warning(logger, "Source array is null. Aborting copy operation.");
				return;
			}

			if (from.Length == 0)
			{
				Log_Helpers.Log_Warning(logger, "Source array is empty. Aborting copy operation.");
				return;
			}

			if (to == null || to.Length < from.Length)
			{
				Log_Helpers.Log(logger, $"Target array is null or too small. Resizing to {from.Length} elements.");
				to = new T[from.Length];
			}

			System.Array.Copy(from, to, from.Length);
			Log_Helpers.Log_Success(logger, $"Copied {from.Length} elements from {from} to {to}.");
		}

		public static bool Remove<T>(ref T[] array, T element, out int index, I_Logger logger = null)
		where T : class
		{
			if (array == null)
			{
				Log_Helpers.Log_Warning(logger, "Array is null. Aborting remove operation.");
				index = -1;
				return false;
			}

			if (array.Length == 0)
			{
				Log_Helpers.Log_Warning(logger, "Array is empty. Aborting remove operation.");
				index = -1;
				return false;
			}

			if (element == null)
			{
				Log_Helpers.Log_Warning(logger, "Element is null. Aborting remove operation.");
				index = -1;
				return false;
			}

			var found_index = System.Array.IndexOf(array, element);

			if (found_index < 0)
			{
				index = -1;
				Log_Helpers.Log_Warning(logger, $"Element {element} not found in array. Aborting remove operation.");
				return false;
			}

			array[found_index] = null;
			index              = found_index;
			Log_Helpers.Log_Success(logger, $"Removed element {element} at index {found_index}.");
			return true;
		}

		public static void Clear<T>(ref T[] array, I_Logger logger = null)
		{
			if (array == null)
			{
				Log_Helpers.Log_Warning(logger, "Array is null. Aborting clear operation.");
				return;
			}

			if (array.Length == 0)
			{
				Log_Helpers.Log_Warning(logger, "Array is empty. Aborting clear operation.");
				return;
			}

			if (array.Length < Parallel_Threshold)
				Clear_Array_Elements_Sequential(ref array, logger);
			else
				Clear_Array_Parallel(ref array, logger);

			Log_Helpers.Log_Success(logger, "Cleared array.");
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
				Log_Helpers.Log_Warning(logger, "Array is null or empty. Aborting remove operation.");
				return;
			}

			if (element == null)
			{
				Log_Helpers.Log_Warning(logger, "Element is null. Aborting remove operation.");
				return;
			}

			// Decide on the removal method based on the size of the array
			if (array.Length > Parallel_Threshold)
			{
				Log_Helpers.Log(
					logger,
					$"Array size is above the parallel threshold ({Parallel_Threshold}). Using parallel removal."
				);

				Remove_Element_Parallel(ref array, element, removal_approach, logger);
			}
			else
			{
				Log_Helpers.Log(
					logger,
					$"Array size is below the parallel threshold ({Parallel_Threshold}). Using sequential removal."
				);

				Remove_Element_Sequential(ref array, element, removal_approach, logger);
			}
		}

		public static void Add_Unique<T>(ref T[] array, T element)
		where T : class
		{
			Add_Unique(ref array, element, out _);
		}

		public static int Count_Non_Null_Elements<T>(T[] array, I_Logger logger = null)
		{
			var non_null_count = 0;

			foreach ( var element in array )
			{
				if (element != null)
					non_null_count++;
			}

			Log_Helpers.Log(logger, $"Counted {non_null_count} non-null elements in the array.");
			return non_null_count;
		}
	}
}
