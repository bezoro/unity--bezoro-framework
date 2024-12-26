using System;

namespace Bezoro.Framework.Core
{
	/// <summary>
	/// Provides a dynamic array that allows for efficient addition of elements and removal
	/// by swapping the last element into the vacancy caused by the removal action. Suitable
	/// for use cases where order preservation of elements is not required.
	/// </summary>
	/// <typeparam name="T">Specifies the type of elements stored in the array.</typeparam>
	public class SwapbackArray<T>
	{
		public SwapbackArray(int count = 4, I_Logger logger = null)
		{
			_items  = new T[count];
			_count  = 0;
			_logger = logger;

			Log_Helpers.Log(_logger, $"SwapbackArray initialized with capacity: {count}");
		}

		~SwapbackArray()
		{
			Log_Helpers.Log(_logger, "SwapbackArray finalized and resources released.");
			_items  = null;
			_logger = null;
		}

		/// <summary>
		/// Internal storage array for the elements of the <see cref="SwapbackArray{T}" />.
		/// It dynamically resizes as elements are added or removed. The capacity is adjusted
		/// to optimize memory usage and access efficiency. This array supports reordering
		/// of elements when removing items.
		/// </summary>
		private T[] _items;
		/// <summary>
		/// Represents the current number of elements in the <see cref="SwapbackArray{T}" />.
		/// </summary>
		private int _count;

		/// <summary>
		/// A logger interface instance used to record operational messages, errors, and warnings
		/// within the <see cref="SwapbackArray{T}" />. It enables tracking and debugging of array-related
		/// operations and processes, enhancing observability and diagnostics.
		/// </summary>
		private I_Logger _logger;

		/// <summary>
		/// Gets the number of elements currently contained in the <see cref="SwapbackArray{T}" />.
		/// </summary>
		public int Count => _count;

		/// <summary>
		/// Gets the total number of elements that the internal array can hold without resizing.
		/// </summary>
		public int Capacity => _items.Length;

		/// <summary>
		/// Resizes the internal array to a new specified size, preserving existing elements.
		/// </summary>
		/// <param name="newSize">The new size of the array.</param>
		private void Resize(int newSize)
		{
			var new_items = new T[newSize];
			System.Array.Copy(_items, new_items, _count);
			_items = new_items;

			Log_Helpers.Log(_logger, $"Resize operation complete. New size: {newSize}");
		}

		/// <summary>
		/// Adds an item to the end of the SwapbackArray. If the array's capacity is insufficient,
		/// its size is doubled to accommodate the new element.
		/// </summary>
		/// <param name="item">The item to add to the SwapbackArray.</param>
		public void Add(T item)
		{
			if (_count == _items.Length)
			{
				Log_Helpers.Log_Warning(_logger, "Array capacity reached. Resizing...");
				Resize(_items.Length * 2);
				Log_Helpers.Log_Success(_logger, $"Array resized to new capacity: {_items.Length}");
			}

			_items[_count++] = item;
			Log_Helpers.Log_Success(_logger, $"Item added. Current count: {_count}");
		}

		/// <summary>
		/// Attempts to retrieve the value at the specified index in the dynamic array.
		/// </summary>
		/// <param name="index">The zero-based index of the element to retrieve.</param>
		/// <param name="value">
		/// The variable where the retrieved value will be stored if the index is valid; otherwise, the default
		/// value of the type.
		/// </param>
		/// <returns>
		/// A boolean value indicating whether the retrieval was successful.
		/// </returns>
		public bool TryGet(int index, out T value)
		{
			if (index < 0 || index >= _count)
			{
				Log_Helpers.Log_Error(_logger, $"Attempted to access invalid index: {index}");
				value = default;
				return false;
			}

			value = _items[index];
			Log_Helpers.Log(_logger, $"Item retrieved at index {index}: {value}");
			return true;
		}

		/// <summary>
		/// Removes the element at the specified index, replacing it with the last element in the array to maintain
		/// constant-time complexity. The array's order may change as a result of this operation.
		/// </summary>
		/// <param name="index">The index of the element to remove.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// Thrown if the index is less than 0 or greater than or equal to the count
		/// of elements in the array.
		/// </exception>
		public void RemoveAt(int index)
		{
			if (index < 0 || index >= _count)
			{
				Log_Helpers.Log_Error(_logger, $"Remove operation failed. Invalid index: {index}");
				throw new ArgumentOutOfRangeException(nameof(index));
			}

			Log_Helpers.Log(_logger, $"Removing item at index {index}.");

			// Replace the element at `index` with the last element (swap-and-trim).
			_items[index] = _items[_count - 1];

			// Nullify the last element (cleanup) and decrement the count.
			_items[--_count] = default;
			Log_Helpers.Log_Success(_logger, $"Item removed. Current count: {_count}");

			// Resize array if necessary: shrink if `_count` drops to one-fourth of capacity.
			if (_count > 0 && _count == _items.Length / 4 && _items.Length > 4)
			{
				Log_Helpers.Log_Warning(_logger, "Array under-utilized. Resizing...");
				Resize(_items.Length / 2);
				Log_Helpers.Log_Success(_logger, $"Array resized to new capacity: {_items.Length}");
			}
		}

		/// <summary>
		/// Removes all elements from the array.
		/// </summary>
		public void Clear()
		{
			Log_Helpers.Log(_logger, "Clearing all elements from the array.");
			System.Array.Clear(_items, 0, _count);
			_count = 0;
			Log_Helpers.Log_Success(_logger, "Array cleared.");
		}
	}
}
