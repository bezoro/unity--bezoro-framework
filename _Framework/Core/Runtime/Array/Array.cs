using System;
using System.Linq;

namespace Bezoro.Framework.Core
{
	public enum Merging_Strategy
	{
		Fill_Null_Elements_And_Append = 0,
		Fill_Null_Elements_And_Resize,
		Fill_Null_Elements_And_Prepend,
		Fill_Null_Elements_Only,
		Append_Only,
		Prepend_Only
	}

	public enum Null_Element_Handling
	{
		Exclude = 0,
		Include
	}

	public static class Array
	{
		/// <summary>
		///     The <c>Merging</c> class provides various static methods to manipulate and merge arrays.
		///     These methods handle operations such as filling, appending, and prepending elements between arrays,
		///     with additional options for handling null elements and resizing arrays.
		/// </summary>
		public static class Merging
		{
			private static void Append_Valid_Source_Elements<T>(
				T[] source,
				ref T[] target,
				I_Logger logger,
				int append_count,
				int source_index,
				bool exclude_nulls
			) where T : class
			{
				var original_length = target.Length;
				System.Array.Resize(ref target, original_length + append_count);

				for (int i = source_index, append_index = original_length ; i < source.Length ; i++)
				{
					if (!exclude_nulls || source[i] != null)
						target[append_index++] = source[i];
				}

				// Log final success message.
				Log_Helpers.Log_Success(
					logger, $"Filled null elements and appended {append_count} elements from source."
				);
			}

			private static bool CheckForRemainingElements<T>(
				T[] source,
				I_Logger logger,
				int source_index,
				bool exclude_nulls,
				out int append_count
			) where T : class
			{
				append_count = 0;

				for (var i = source_index ; i < source.Length ; i++)
				{
					if (!exclude_nulls || source[i] != null)
						append_count++;
				}

				if (append_count == 0)
				{
					// Log and exit if no elements are left to append.
					Log_Helpers.Log_Success(logger, "Filled null elements in the target array.");
					return true;
				}

				return false;
			}

			private static int Populate_Target_With_Source<T>(
				T[] source,
				T[] target,
				int source_index,
				bool exclude_nulls
			) where T : class
			{
				// Cache lengths for performance
				var source_length = source.Length;
				var target_length = target.Length;

				for (var target_index = 0 ;
					 target_index < target_length && source_index < source_length ;
					 target_index++)
				{
					// Skip non-null entries in the target array
					if (target[target_index] != null)
						continue;

					// Find the next valid source element (non-null if exclude_nulls is true)
					T source_element = null;

					while ( source_index < source_length )
					{
						source_element = source[source_index++];

						if (!exclude_nulls || source_element != null)
							break;
					}

					// Assign to target only if a valid source element was found
					if (source_element != null || !exclude_nulls)
					{
						target[target_index] = source_element;
					}
				}

				return source_index; // Return the updated source index
			}

			private static bool Validate_Source<T>(T[] source, I_Logger logger) where T : class
			{
				if (source != null) return true;

				Log_Helpers.Log_Warning(logger, "Source array must not be null. Aborting operation.");
				return false;
			}

			private static bool Validate_Target<T>(T[] target, I_Logger logger) where T : class
			{
				if (target != null) return true;

				Log_Helpers.Log_Warning(logger, "Target array must not be null. Aborting operation.");
				return false;
			}

			private static bool Handle_Null_Target_Initialization<T>(
				T[] source,
				ref T[] target,
				I_Logger logger,
				bool exclude_nulls
			)
			where T : class
			{
				if (target != null && target.Length != 0) return false;

				if (exclude_nulls)
				{
					// Initialize target with enough space for source elements
					target = new T[Array_Helpers.Count_Non_Null_Elements(source)];

					// Fill the target with elements from the source array
					Merging.Populate_Target_With_Source(source, target, 0, true);
				}
				else
				{
					// Initialize target with source elements
					target = source.ToArray();
				}

				Log_Helpers.Log_Success(logger, "Target was null. Initialized from source array.");
				return true;
			}

			/// <summary>
			///     Replaces null elements in the target array with corresponding non-null elements from the source array.
			///     This method does not resize the target array; only existing null slots are filled, and any remaining source
			///     elements are ignored if they do not fit.
			/// </summary>
			/// <typeparam name="T">The type of elements in the array.</typeparam>
			/// <param name="source">The source array providing elements to fill the null slots in the target array.</param>
			/// <param name="target">The target array whose null elements will be replaced.</param>
			/// <param name="logger">Optional logger for debug or error messages.</param>
			public static void Fill_Null_Elements_Only<T>(T[] source, ref T[] target, I_Logger logger = null)
			where T : class
			{
				// Validate input arrays. Log warnings if null and exit early if validation fails.
				if (!Merging.Validate_Source(source, logger)) return;

				// If the target array is null, initialize it with the source array and log the operation.
				if (target == null || target.Length == 0)
				{
					target = source.ToArray(); // Clone the source array to avoid reference issues.

					Log_Helpers.Log_Success(
						logger, "Target array was null and has been initialized with the source array."
					);

					return;
				}

				// Track the index of the source array.
				var source_index = 0;

				// Iterate through the target array.
				for (var target_index = 0 ; target_index < target.Length ; target_index++)
				{
					// Skip non-null elements in the target.
					if (target[target_index] != null) continue;

					// Exit early if there are no more source elements.
					if (source_index >= source.Length) break;

					// Get the next element from the source array.
					var source_element = source[source_index];

					if (source_element != null)
					{
						// Fill the null slot in the target array.
						target[target_index] = source_element;

						Log_Helpers.Log(
							logger, $"Element {source_element} added to target array at index {target_index}."
						);
					}

					// Move to the next element in the source array.
					source_index++;
				}

				// Log warning if not all source elements could be used.
				if (source_index < source.Length)
				{
					Log_Helpers.Log_Warning(
						logger,
						"Source array contains additional elements that could not be used to fill target array null slots."
					);

					return;
				}

				// Log success message.
				Log_Helpers.Log_Success(logger, "Source array elements used to fill target array null slots.");
			}

			/// <summary>
			///     Fills the null elements in the target array with elements from the source array.
			///     Resizes the target array if there are insufficient null elements to accommodate all source elements.
			/// </summary>
			/// <typeparam name="T">The type of elements in the arrays.</typeparam>
			/// <param name="source">The source array providing elements to fill the null slots in the target array.</param>
			/// <param name="target">The target array whose null elements will be replaced and potentially resized.</param>
			/// <param name="logger">Optional logger for debug or error messages.</param>
			public static void Fill_Null_Elements_And_Resize<T>(T[] source, ref T[] target, I_Logger logger = null)
			where T : class
			{
				// Validate input arrays. Log warnings if null and exit early if validation fails.
				if (!Merging.Validate_Source(source, logger)) return;

				// Keep track of source and target array indices.
				var source_index = 0;
				var target_index = 0;

				// Fill null elements in the target array using the source array.
				while ( target_index < target.Length && source_index < source.Length )
				{
					if (target[target_index] == null)
					{
						target[target_index] = source[source_index];
						source_index++;
					}

					target_index++;
				}

				// If there are remaining elements in the source array, resize the target array.
				if (source_index < source.Length)
				{
					// Add remaining elements to a resized array.
					var total_new_size = target.Length + ( source.Length - source_index );
					var resized_target = new T[total_new_size];

					// Use Array.Copy for better memory and performance efficiency.
					System.Array.Copy(target, resized_target, target.Length);

					System.Array.Copy(
						source, source_index, resized_target, target.Length, source.Length - source_index
					);

					// Reassign the reference and log success.
					target = resized_target;

					Log_Helpers.Log_Success(
						logger, $"Target array resized to fit all source elements. New size: {total_new_size}"
					);
				}
				else
				{
					// Log success if no resizing was needed.
					Log_Helpers.Log_Success(logger, "Merge Completed. No resizing needed.");
				}
			}

			/// <summary>
			///     Appends elements from the source array to the end of the target array.
			///     Optionally, null elements in the source array can be excluded during the append operation.
			///     This method does not alter the existing elements in the target array.
			/// </summary>
			/// <typeparam name="T">The type of elements in the arrays.</typeparam>
			/// <param name="source">The source array providing elements to append to the target array.</param>
			/// <param name="target">The target array to which elements will be appended. This may be resized if necessary.</param>
			/// <param name="null_element_handling">
			///     A boolean flag indicating whether to include null elements from the source array in the append operation.
			/// </param>
			/// <param name="logger">Optional logger for debug or error messages.</param>
			public static void Append_Only<T>(
				T[] source,
				ref T[] target,
				Null_Element_Handling null_element_handling = Null_Element_Handling.Exclude,
				I_Logger logger = null
			) where T : class
			{
				// Validate input arrays. Log warnings if null and exit early if validation fails.
				if (!Merging.Validate_Source(source, logger)) return;

				var elements_to_append_count = 0;

				// Calculate how many valid elements will be appended from the source array.
				foreach ( var element in source )
				{
					if (null_element_handling == Null_Element_Handling.Include || element != null)
						elements_to_append_count++;
				}

				// If there are no valid elements to append, exit early.
				if (elements_to_append_count == 0)
				{
					Log_Helpers.Log(logger, "No elements to append from the source array.");
					return;
				}

				// Resize the target array to hold the new elements.
				var original_length = target.Length;
				System.Array.Resize(ref target, original_length + elements_to_append_count);

				// Append elements directly to the target array.
				var append_index = original_length;

				foreach ( var element in source )
				{
					if (null_element_handling == Null_Element_Handling.Include || element != null)
						target[append_index++] = element;
				}

				// Log a success message.
				Log_Helpers.Log_Success(
					logger, $"Appended {elements_to_append_count} elements to the end of the target array."
				);
			}

			/// <summary>
			///     Prepends elements from the source array to the beginning of the target array.
			///     Optionally, null elements in the source array can be excluded during the prepend operation.
			///     This method does not alter the existing elements in the target array.
			/// </summary>
			/// <param name="source">The source array providing elements to prepend to the target array.</param>
			/// <param name="target">The target array where elements from the source array will be prepended.</param>
			/// <param name="preserve_null_elements">
			///     A boolean flag indicating whether to include null elements from the source array in the prepend operation.
			/// </param>
			/// <param name="logger">Optional logger for debug or error messages.</param>
			/// <typeparam name="T">The type of elements in the arrays.</typeparam>
			public static void Prepend_Only<T>(
				T[] source,
				ref T[] target,
				Null_Element_Handling preserve_null_elements = Null_Element_Handling.Exclude,
				I_Logger logger = null
			) where T : class
			{
				// Validate source and target arrays. Log warnings and exit early if validation fails.
				if (!Merging.Validate_Source(source, logger)) return;

				// Calculate the size of the new array.
				var non_null_count = preserve_null_elements == Null_Element_Handling.Include
					? source.Length
					: source.Count(item => item != null);

				var new_array = new T[non_null_count + target.Length];

				// Fill the first part of the array with source elements.
				var current_index = 0;

				foreach ( var element in source )
				{
					if (preserve_null_elements == Null_Element_Handling.Exclude && element == null) continue;

					new_array[current_index++] = element;
				}

				// Copy the existing target elements into the second part of the array.
				System.Array.Copy(target, 0, new_array, current_index, target.Length);

				// Assign the new array to the target reference.
				target = new_array;

				// Log success message.
				Log_Helpers.Log_Success(logger, "Source array elements were prepended successfully.");
			}

			/// <summary>
			///     Fills the null elements in the target array with elements from the source array and
			///     appends the remaining unconsumed elements from the source array to the end of the target array.
			///     Optionally, null elements in the source array can be excluded from the operation.
			/// </summary>
			/// <typeparam name="T">The type of elements in the arrays.</typeparam>
			/// <param name="source">The source array providing elements to fill the null slots in the target array.</param>
			/// <param name="target">
			///     The target array whose null elements will be replaced, and where remaining source elements will be
			///     appended.
			/// </param>
			/// <param name="logger">Optional logger for debug or error messages.</param>
			/// <param name="null_element_handling">
			///     A boolean flag indicating whether to include null elements from the source array during the operation.
			/// </param>
			public static void Fill_Null_Elements_And_Append<T>(
				T[] source,
				ref T[] target,
				Null_Element_Handling null_element_handling = Null_Element_Handling.Exclude,
				I_Logger logger = null
			) where T : class
			{
				// Validate the input arrays. Logs warnings and exit early if validation fails.
				var validation_successful = Merging.Validate_Source(source, logger);
				if (!validation_successful) return;

				// Cache the condition to skip null elements in the source
				var exclude_nulls = null_element_handling == Null_Element_Handling.Exclude;

				var target_initialized = Merging.Handle_Null_Target_Initialization(
					source, ref target, logger, exclude_nulls);

				if (target_initialized) return;

				var source_index = 0;

				// Step 1: Fill null elements in target from the source array.
				source_index = Merging.Populate_Target_With_Source(source, target, source_index, exclude_nulls);

				// Step 2: Calculate remaining valid source elements.
				if (Merging.CheckForRemainingElements(
						source, logger, source_index, exclude_nulls, out var append_count))
					return;

				// Resize target and append remaining valid source elements.
				Merging.Append_Valid_Source_Elements(
					source, ref target, logger, append_count, source_index, exclude_nulls);
			}

			public static void Fill_Null_Elements_And_Prepend<T>(
				T[] source,
				ref T[] target,
				Null_Element_Handling null_element_handling = Null_Element_Handling.Exclude,
				I_Logger logger = null
			) where T : class
			{
				// Validate input arrays. Log warnings if null and exit early if validation fails.
				if (!Merging.Validate_Source(source, logger)) return;

				// Step 1: Fill null elements in the target array using the source array.
				var source_index = 0;

				for (var target_index = 0 ;
					 target_index < target.Length && source_index < source.Length ;
					 target_index++)
				{
					if (target[target_index] == null &&
						( source[source_index] != null || null_element_handling == Null_Element_Handling.Include ))
					{
						target[target_index] = source[source_index];
					}

					if (null_element_handling == Null_Element_Handling.Exclude ||
						source[source_index] != null)
						source_index++;
				}

				// Step 2: Prepend remaining source elements directly to the target array.
				var remaining_length = source.Length - source_index;

				if (remaining_length > 0)
				{
					// Optimization: Allocate new target array once directly for final size.
					var new_target = new T[target.Length + remaining_length];

					// Copy remaining source elements to the beginning of the new target array.
					System.Array.Copy(source, source_index, new_target, 0, remaining_length);

					// Copy the original target elements into the new array.
					System.Array.Copy(target, 0, new_target, remaining_length, target.Length);

					// Assign the new array back to the target reference.
					target = new_target;
				}

				// Log success message.
				Log_Helpers.Log_Success(
					logger, "Filled null slots in the target array and prepended remaining source elements."
				);
			}
		}

		/// <summary>
		///     Merges the elements of the source array into the target array using the specified merging strategy and null element
		///     handling options.
		///     Depending on the strategy, the method can fill null slots, append, or prepend elements while managing null elements
		///     as per the configuration.
		/// </summary>
		/// <typeparam name="T">The type of elements in the arrays, constrained to reference type.</typeparam>
		/// <param name="source">The source array containing elements to be merged into the target array.</param>
		/// <param name="target">The target array that will be updated with elements from the source array.</param>
		/// <param name="strategy">
		///     Specifies how to merge the source array into the target array.
		///     Options include filling null elements, appending, prepending, or resizing the array.
		/// </param>
		/// <param name="null_element_handling">
		///     Determines how to handle null elements from the source during the merge process.
		///     Options include preserving null elements or excluding them from the merge.
		/// </param>
		/// <param name="logger">
		///     Optional logger for capturing debug, warning, error or success messages during the merge process.
		/// </param>
		public static void Merge<T>(
			T[] source,
			T[] target,
			Merging_Strategy strategy = Merging_Strategy.Fill_Null_Elements_And_Append,
			Null_Element_Handling null_element_handling = Null_Element_Handling.Exclude,
			I_Logger logger = null
		)
		where T : class
		{
			switch ( strategy )
			{
				case Merging_Strategy.Fill_Null_Elements_And_Append:
					Merging.Fill_Null_Elements_And_Append(source, ref target, null_element_handling, logger);
					break;
				case Merging_Strategy.Fill_Null_Elements_And_Resize:
					Merging.Fill_Null_Elements_And_Resize(source, ref target, logger);
					break;
				case Merging_Strategy.Fill_Null_Elements_And_Prepend:
					Merging.Fill_Null_Elements_And_Prepend(source, ref target, null_element_handling, logger);
					break;
				case Merging_Strategy.Fill_Null_Elements_Only:
					Merging.Fill_Null_Elements_Only(source, ref target, logger);
					break;
				case Merging_Strategy.Append_Only:
					Merging.Append_Only(source, ref target, null_element_handling, logger);
					break;
				case Merging_Strategy.Prepend_Only:
					Merging.Prepend_Only(source, ref target, null_element_handling, logger);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(strategy), strategy, null);
			}
		}
	}
}
