using System;

namespace Bezoro.Framework.Core
{
	/// <summary>
	///     Provides a dynamic array that allows for efficient addition of elements and removal
	///     by swapping the last element into the vacancy caused by the removal action. Suitable
	///     for use cases where order preservation of elements is not required.
	/// </summary>
	/// <typeparam name="T">Specifies the type of elements stored in the array.</typeparam>
	public class SwapbackArray<T>
	{
		public SwapbackArray(int count = 4, I_Logger logger = null)
		{
			_items  = new T[count];
			_count  = 0;
			_logger = logger;
		}

		~SwapbackArray()
		{
			// Clean up resources if necessary.
			_items  = null;
			_logger = null;
		}

		/// <summary>
		///     Internal storage array for the elements of the <see cref="SwapbackArray{T}" />.
		///     It dynamically resizes as elements are added or removed. The capacity is adjusted
		///     to optimize memory usage and access efficiency. This array supports reordering
		///     of elements when removing items.
		/// </summary>
		private T[] _items;
		/// <summary>
		///     Represents the current number of elements in the <see cref="SwapbackArray{T}" />.
		/// </summary>
		/// <remarks>
		///     This variable tracks the logical count of elements in the array, which may be less than the array's capacity.
		///     It is incremented when elements are added and decremented when elements are removed.
		/// </remarks>
		private int _count;

		/// <summary>
		///     A logger interface instance used to record operational messages, errors, and warnings
		///     within the <see cref="SwapbackArray{T}" />. It enables tracking and debugging of array-related
		///     operations and processes, enhancing observability and diagnostics.
		/// </summary>
		private I_Logger _logger;

		/// <summary>
		///     Gets the number of elements currently contained in the <see cref="SwapbackArray{T}" />.
		/// </summary>
		/// <value>
		///     The total count of elements stored in the array.
		/// </value>
		/// <remarks>
		///     The value returned represents the number of elements added to the array,
		///     excluding any available capacity not currently in use.
		/// </remarks>
		public int Count => _count;
		/// <summary>
		///     Gets the total number of elements that the internal array can hold without resizing.
		/// </summary>
		/// <remarks>
		///     The Capacity property represents the size of the underlying array used by the
		///     dynamic SwapbackArray data structure. If the number of elements exceeds this capacity,
		///     the internal array is automatically resized to accommodate additional elements.
		///     Capacity is always greater than or equal to the Count property, which represents
		///     the number of elements currently in the array.
		/// </remarks>
		/// <value>
		///     An integer representing the total number of elements that the array can store without resizing.
		/// </value>
		public int Capacity => _items.Length;

		/// <summary>
		///     Resizes the internal array to a new specified size, preserving existing elements.
		/// </summary>
		/// <param name="newSize">The new size of the array.</param>
		private void Resize(int newSize)
		{
			var new_items = new T[newSize];
			System.Array.Copy(_items, new_items, _count);
			_items = new_items;
		}

		/// <summary>
		///     Adds an item to the end of the SwapbackArray. If the array's capacity is insufficient,
		///     its size is doubled to accommodate the new element.
		/// </summary>
		/// <param name="item">The item to add to the SwapbackArray.</param>
		public void Add(T item)
		{
			if (_count == _items.Length)
				Resize(_items.Length * 2);

			_items[_count++] = item;
		}

		/// <summary>
		///     Attempts to retrieve the value at the specified index in the dynamic array.
		/// </summary>
		/// <param name="index">The zero-based index of the element to retrieve.</param>
		/// <param name="value">
		///     The variable where the retrieved value will be stored if the index is valid; otherwise, the default
		///     value of the type.
		/// </param>
		/// <returns>
		///     A boolean value indicating whether the retrieval was successful.
		///     Returns <c>true</c> if the index is within the bounds of the array and the value is retrieved successfully;
		///     otherwise, <c>false</c>.
		/// </returns>
		public bool TryGet(int index, out T value)
		{
			if (index < 0 || index >= _count)
			{
				value = default;
				return false;
			}

			value = _items[index];
			return true;
		}

		/// <summary>
		///     Removes the element at the specified index, replacing it with the last element in the array to maintain
		///     constant-time complexity.
		///     The array's order may change as a result of this operation.
		/// </summary>
		/// <param name="index">The index of the element to remove.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		///     Thrown if the index is less than 0 or greater than or equal to the count
		///     of elements in the array.
		/// </exception>
		public void RemoveAt(int index)
		{
			if (index < 0 || index >= _count)
				throw new ArgumentOutOfRangeException(nameof(index));

			// Replace the element at `index` with the last element (swap-and-trim).
			_items[index] = _items[_count - 1];

			// Nullify the last element (cleanup) and decrement the count.
			_items[--_count] = default;

			// Resize array if necessary: shrink if `_count` drops to one-fourth of capacity.
			if (_count > 0 && _count == _items.Length / 4 && _items.Length > 4) // Ensure a minimum array size.
			{
				Resize(_items.Length / 2); // Dynamically resize to half.
			}
		}

		/// <summary>
		///     Removes all elements from the array, setting the count to zero
		///     and resetting all elements to their default value.
		/// </summary>
		public void Clear()
		{
			System.Array.Clear(_items, 0, _count);
			_count = 0;
		}
	}
}
