using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bezoro.Framework.Core
{
	public static class ArrayHelpers
	{
		static ArrayHelpers()
		{
			Parallel_Threshold = 100;
		}

		public static int Parallel_Threshold;

		private static void ClearArrayParallel<T>(ref T[] array)
		{
			var temp_array = new T[array.Length];

			Parallel.For(0, array.Length, i => { temp_array[i] = default; });
			System.Array.Copy(temp_array, array, array.Length);
		}

		private static void ClearArrayElementsSequential<T>(ref T[] array)
		{
			for (var i = 0 ; i < array.Length ; i++)
			{
				array[i] = default;
			}
		}

		private static void Resize_By_Factor_And_Add_Element<T>(
			ref T[] array,
			T element,
			out int index,
			out int valid_size,
			int resize_factor
		)
		where T : class
		{
			Resize_By_Factor_And_Add_Element(ref array, element, out index, resize_factor);
			valid_size = array.Length;
		}

		private static void Resize_By_Factor_And_Add_Element<T>(
			ref T[] array,
			T element,
			out int index,
			int resize_factor
		) where T : class
		{
			var old_length = array.Length;
			Resize_By_Factor(ref array, resize_factor);
			array[old_length] = element;
			index             = old_length;
		}

		private static void Remove_Element_Sequential<T>(
			ref T[] array,
			T element,
			ArrayRemovalApproach removal_approach
		)
		{
			// Find the index of the element
			var index = System.Array.IndexOf(array, element);

			if (index < 0)
			{
				Logger.Log_Warning("Element not found. No operation performed.");
				return;
			}

			// Handle removal approach
			switch ( removal_approach )
			{
				case ArrayRemovalApproach.Resize:
					// Shift elements and resize the array
					for (var i = index ; i < array.Length - 1 ; i++)
						array[i] = array[i + 1];

					System.Array.Resize(ref array, array.Length - 1);

					break;
				case ArrayRemovalApproach.SetToNull:
					// Nullify the element
					array[index] = default;

					break;
				default:
					Logger.Log_Warning($"Unhandled Array_Removal_Approach: {removal_approach}");
					break;
			}
		}

		private static ArrayElementInfo<T> Remove_Element_Parallel<T>(
			ref T[] array,
			T element,
			ArrayRemovalApproach removal_approach,
			ILogger logger = null
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
				Logger.Log_Warning("Element not found in array. No operation performed.");
				return new( -1, element, array.Length );
			}

			switch ( removal_approach )
			{
				case ArrayRemovalApproach.Resize:
					Resize_Array_Parallel(ref array, found_index);
					break;

				case ArrayRemovalApproach.SetToNull:
					lock ( lock_obj )
					{
						array[found_index] = default;
					}

					break;

				default:
					Logger.Log_Warning($"Unhandled Array_Removal_Approach: {removal_approach}");
					break;
			}

			return new( found_index, element, array.Length );
		}

		private static void Resize_Array_Parallel<T>(
			ref T[] array,
			int index
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
		}

		public static void Initialize_Null_Array<T>(ref T[] array)
		{
			if (array != null) return;

			array = System.Array.Empty<T>();
		}

		public static void Remove_Duplicates<T>(ref T[] array)
		{
			if (array == null)
			{
				Logger.Log_Warning("Array is null. Aborting remove operation.");
				return;
			}

			if (array.Length <= 1)
			{
				return;
			}

			var unique_elements = new List<T>();
			var seen_elements   = new HashSet<T>();

			foreach ( var element in array )
			{
				if (element == null) continue;
				if (!seen_elements.Add(element)) continue;
				unique_elements.Add(element);
				Logger.Log_Info($"Removing duplicate: {element}");
			}

			array = unique_elements.ToArray();
			Logger.LogSuccess("Removed duplicates from array.");
		}

		public static bool Compare_Arrays<T>(T[] a, T[] b)
		{
			if (a == null && b == null) return true;
			if (a == null || b == null) return false;
			if (a.Length != b.Length) return false;

			for (var i = 0 ; i < a.Length ; i++)
			{
				if (EqualityComparer<T>.Default.Equals(a[i], b[i])) continue;

				Logger.Log_Info($"Array comparison failed at index {i}.");
				return false;
			}

			Logger.Log_Info("Arrays are equal");
			return true;
		}

		public static void Add<T>(
			ref T[] array,
			T element_to_add,
			out int inserted_index,
			int resize_factor = 2
		)
		where T : class
		{
			if (element_to_add == null)
			{
				inserted_index = -1;
				Logger.Log_Warning("Element is null. Add operation aborted.");
				return;
			}

			// Initialize array if it's null or empty
			if (array == null || array.Length == 0)
			{
				array          = new T[1];
				array[0]       = element_to_add;
				inserted_index = 0;
				Logger.LogSuccess($"Array was null. Initialized and added element: {element_to_add}");
				return;
			}

			if (array.Length > Parallel_Threshold)
				Find_Element_Parallel(array, null);
			else
				Find_Element_Sequentially(array, null);

			// Find the first empty slot
			for (var i = 0 ; i < array.Length ; i++)
			{
				if (array[i] != null) continue;

				array[i]       = element_to_add;
				inserted_index = i;
				Logger.LogSuccess($"Added element: {element_to_add} at index {i}");
				return;
			}

			// No empty slot, resize the array
			var original_size = array.Length;
			var new_size      = array.Length * resize_factor;
			System.Array.Resize(ref array, new_size);

			// Add the element in the first empty slot of the resized array
			array[original_size] = element_to_add;
			inserted_index       = original_size;

			Logger.LogSuccess(
				$"Resized array to {new_size}. Added element: {element_to_add} at index {inserted_index}"
			);
		}

		public static void Add<T>(ref T[] array, T element, int resize_factor = 2)
		where T : class
		{
			Add(ref array, element, out _, resize_factor);
		}

		public static void Resize_By_Factor<T_Array>(ref T_Array[] array, int factor)
		{
			if (factor <= 1)
			{
				Logger.Log_Warning($"Factor: {factor} is invalid. Array size will not be multiplied.");
				return;
			}

			System.Array.Resize(ref array, array.Length * factor);
			Logger.LogSuccess($"Array size resized by factor: {factor}");
		}

		public static void Add_Unique<T>(
			ref T[] array,
			T element,
			out int index,
			int resize_factor = 2
		)
		where T : class
		{
			if (element == null)
			{
				index = -1;
				return;
			}

			if (array == null || array.Length == 0)
			{
				Setup_Array_With_First_Element(ref array, element);
				index = 0;
				return;
			}

			bool element_exists;

			if (array.Length > Parallel_Threshold)
				element_exists = Find_Element_Parallel(array, element).RelevantIndex >= 0;
			else
				element_exists = Find_Element_Sequentially(array, element).RelevantIndex >= 0;

			if (element_exists)
			{
				index = -1;
				Logger.Log_Warning($"Element {element} already exists in array. Add operation aborted.");
				return;
			}

			Add(ref array, element, out var i, resize_factor);
			index = i;
			Logger.LogSuccess($"Adding unique element: {element}");
		}

		public static ArrayElementInfo<T> Find_Element_Sequentially<T>(
			T[] array,
			T element_to_find
		)
		{
			if (array == null || array.Length == 0)
			{
				Logger.Log_Warning("Array is null or empty. Aborting search.");
				return new( -1, element_to_find, array?.Length ?? 0 );
			}

			if (element_to_find == null)
			{
				Logger.Log_Warning("Element to find is null. Aborting search.");
				return new( -1, default, array.Length );
			}

			for (var i = 0 ; i < array.Length ; i++)
			{
				if (!array[i].Equals(element_to_find)) continue;

				Logger.LogSuccess($"Element {element_to_find} found at index {i}");
				return new( i, element_to_find, array.Length );
			}

			Logger.Log_Warning($"Element {element_to_find} not found in array.");
			return new( -1, element_to_find, array.Length );
		}

		public static ArrayElementInfo<T> Find_Element_Parallel<T>(
			T[] array,
			T element_to_find
		)
		{
			if (array == null || array.Length == 0)
			{
				Logger.Log_Warning("Array is null or empty. Aborting search.");
				return new( -1, element_to_find, array?.Length ?? 0 );
			}

			if (element_to_find == null)
			{
				Logger.Log_Warning("Element to find is null. Aborting search.");
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
				Logger.LogSuccess($"Element {element_to_find} found at index {result_index}.");
			else
				Logger.Log_Warning($"Element {element_to_find} not found in array.");

			return new( result_index, element_to_find, array.Length );
		}

		public static bool Setup_Array_With_First_Element<T>(
			ref T[] array,
			T element
		)
		{
			Assert.IsNotNull(element);

			if (array != null && array.Length != 0) return false;

			array    = new T[1];
			array[0] = element;
			Logger.Log_Info($"Created array and adding first element: {element}");
			return true;
		}

		public static void Merge<T>(T[] source, ref T[] target, ILogger logger = null)
		{
			if (source == null)
			{
				Logger.Log_Warning("Source array is null. Aborting merge operation.");
				return;
			}

			if (source.Length == 0)
			{
				Logger.Log_Warning("Source array is empty. Aborting merge operation.");
				return;
			}

			if (target == null || target.Length == 0)
			{
				System.Array.Resize(ref target, source.Length);
				Copy(source, ref target);

				Logger.LogSuccess(
					$"Target array is null or empty. Resized to {source.Length} and copied source array."
				);

				return;
			}

			if (target.Length < source.Length)
			{
				System.Array.Resize(ref target, source.Length);
				Copy(source, ref target);

				Logger.LogSuccess(
					$"Target array is smaller than source array. Resized to {source.Length} and copied source array."
				);

				return;
			}

			for (var i = 0 ; i < source.Length ; i++)
			{
				if (target[i] != null) continue;

				target[i] = source[i];
			}

			Logger.LogSuccess("Source array merged into target array.");
		}

		public static void Copy<T>(T[] from, ref T[] to, ILogger logger = null)
		{
			if (from == null)
			{
				Logger.Log_Warning("Source array is null. Aborting copy operation.");
				return;
			}

			if (from.Length == 0)
			{
				Logger.Log_Warning("Source array is empty. Aborting copy operation.");
				return;
			}

			if (to == null || to.Length < from.Length)
			{
				Logger.Log_Info($"Target array is null or too small. Resizing to {from.Length} elements.");
				to = new T[from.Length];
			}

			System.Array.Copy(from, to, from.Length);
			Logger.LogSuccess($"Copied {from.Length} elements from {from} to {to}.");
		}

		public static bool Remove<T>(ref T[] array, T element, out int index, ILogger logger = null)
		where T : class
		{
			if (array == null)
			{
				Logger.Log_Warning("Array is null. Aborting remove operation.");
				index = -1;
				return false;
			}

			if (array.Length == 0)
			{
				Logger.Log_Warning("Array is empty. Aborting remove operation.");
				index = -1;
				return false;
			}

			if (element == null)
			{
				Logger.Log_Warning("Element is null. Aborting remove operation.");
				index = -1;
				return false;
			}

			var found_index = System.Array.IndexOf(array, element);

			if (found_index < 0)
			{
				index = -1;
				Logger.Log_Warning($"Element {element} not found in array. Aborting remove operation.");
				return false;
			}

			array[found_index] = null;
			index              = found_index;
			Logger.LogSuccess($"Removed element {element} at index {found_index}.");
			return true;
		}

		public static void Clear<T>(ref T[] array, ILogger logger = null)
		{
			if (array == null)
			{
				Logger.Log_Warning("Array is null. Aborting clear operation.");
				return;
			}

			if (array.Length == 0)
			{
				Logger.Log_Warning("Array is empty. Aborting clear operation.");
				return;
			}

			if (array.Length < Parallel_Threshold)
				ClearArrayElementsSequential(ref array);
			else
				ClearArrayParallel(ref array);

			Logger.LogSuccess("Cleared array.");
		}

		public static void Remove_Element<T>(
			ref T[] array,
			T element,
			ArrayRemovalApproach removal_approach = ArrayRemovalApproach.Resize,
			ILogger logger = null
		)
		{
			// Validate input
			if (array == null || array.Length == 0)
			{
				Logger.Log_Warning("Array is null or empty. Aborting remove operation.");
				return;
			}

			if (element == null)
			{
				Logger.Log_Warning("Element is null. Aborting remove operation.");
				return;
			}

			// Decide on the removal method based on the size of the array
			if (array.Length > Parallel_Threshold)
			{
				Logger.Log_Info(
					$"Array size is above the parallel threshold ({Parallel_Threshold}). Using parallel removal."
				);

				Remove_Element_Parallel(ref array, element, removal_approach, logger);
			}
			else
			{
				Logger.Log_Info(
					$"Array size is below the parallel threshold ({Parallel_Threshold}). Using sequential removal."
				);

				Remove_Element_Sequential(ref array, element, removal_approach);
			}
		}

		public static void Add_Unique<T>(ref T[] array, T element)
		where T : class
		{
			Add_Unique(ref array, element, out _);
		}

		public static int Count_Non_Null_Elements<T>(T[] array, ILogger logger = null)
		{
			var non_null_count = 0;

			foreach ( var element in array )
			{
				if (element != null)
					non_null_count++;
			}

			Logger.Log_Info($"Counted {non_null_count} non-null elements in the array.");
			return non_null_count;
		}
	}
}
