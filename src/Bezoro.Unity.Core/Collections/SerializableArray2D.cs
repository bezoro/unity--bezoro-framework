using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;
using Array = System.Array;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace Bezoro.Unity.Core.Collections;

/// <summary>
///     Represents a serializable 2D array.
/// </summary>
/// <typeparam name="T">The type of elements in the array.</typeparam>
[Serializable]
public class SerializableArray2D<T> : IEnumerable<T>
{
	[SerializeField] private T[] elements;
	[SerializeField] private int width;
	[SerializeField] private int height;

	/// <summary>
	///     Creates a new 2D array with the specified dimensions.
	/// </summary>
	/// <param name="width">Width of the array.</param>
	/// <param name="height">Height of the array.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public SerializableArray2D(int width, int height)
	{
		CheckSerializability();
		ValidateDimensions(width, height);

		this.width  = width;
		this.height = height;
		elements    = new T[width * height];
	}

	/// <summary>
	///     Creates a 2D array from an existing 2D array.
	/// </summary>
	/// <param name="array2d">Source 2D array.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public SerializableArray2D(T[,] array2d)
	{
		CheckSerializability();

		if (array2d == null)
			throw new ArgumentNullException(nameof(array2d), "Input array cannot be null.");

		int arrayHeight = array2d.GetLength(0);

		int arrayWidth = array2d.GetLength(1);

		ValidateDimensions(arrayWidth, arrayHeight);

		width    = arrayWidth;
		height   = arrayHeight;
		elements = new T[width * height];

		LoadFrom2DArray(array2d);
	}

	/// <summary>
	///     Creates a 2D array from a 1D array with specified dimensions.
	/// </summary>
	/// <param name="array1D">Source 1D array.</param>
	/// <param name="width">Width of the array.</param>
	/// <param name="height">Height of the array.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public SerializableArray2D(T[] array1D, int width, int height)
	{
		CheckSerializability();

		if (array1D == null)
			throw new ArgumentNullException(nameof(array1D), "Input array cannot be null.");

		ValidateDimensions(width, height);

		if (array1D.Length != width * height)
			throw new ArgumentException("The length of the input array does not match the specified dimensions.");

		this.width  = width;
		this.height = height;
		elements    = new T[this.width * this.height];

		LoadFrom1DArray(array1D, width, height);
	}

	/// <summary>
	///     Gets the height of the 2D array.
	/// </summary>
	public int Height
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => height;
	}

	/// <summary>
	///     Gets the width of the 2D array.
	/// </summary>
	public int Width
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => width;
	}

	/// <summary>
	///     Gets the underlying data array.
	/// </summary>
	public T[] Elements
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => elements ??= new T[width * height];
	}

	/// <summary>
	///     Gets or sets the element at the specified indices.
	/// </summary>
	/// <param name="x">The x-coordinate (column).</param>
	/// <param name="y">The y-coordinate (row).</param>
	/// <returns>The element at [x, y].</returns>
	public T this[int x, int y]
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			ValidateIndices(x, y);

			return elements[Index(x, y)];
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			ValidateIndices(x, y);

			elements[Index(x, y)] = value;
		}
	}

	/// <summary>
	///     Implicit conversion to array.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static implicit operator T[](SerializableArray2D<T>? source) =>
		source?.GetDataCopy() ?? Array.Empty<T>(); // Return a copy to prevent modification of internal data

	/// <summary>
	///     Attempts to get the element directly above the specified cell.
	/// </summary>
	/// <param name="x">The x-coordinate (column) of the current cell.</param>
	/// <param name="y">The y-coordinate (row) of the current cell.</param>
	/// <param name="element">
	///     When this method returns, contains the element above, if found; otherwise, the default value for
	///     the type T.
	/// </param>
	/// <returns>true if an element exists at (x, y-1); otherwise, false.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool TryGetElementAbove(int x, int y, out T? element) =>
		TryGetElementInDirection(x, y, Direction.Above, out element);

	/// <summary>
	///     Attempts to get the element directly above the specified cell.
	/// </summary>
	/// <param name="gridPosition">The Vector2Int containing x and y coordinates of the current cell.</param>
	/// <param name="element">
	///     When this method returns, contains the element above, if found; otherwise, the default value for
	///     the type T.
	/// </param>
	/// <returns>true if an element exists at (gridPosition.x, gridPosition.y-1); otherwise, false.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool TryGetElementAbove(Vector2Int gridPosition, out T? element) =>
		TryGetElementInDirection(gridPosition.x, gridPosition.y, Direction.Above, out element);

	/// <summary>
	///     Attempts to get the element directly below the specified cell.
	/// </summary>
	/// <param name="x">The x-coordinate (column) of the current cell.</param>
	/// <param name="y">The y-coordinate (row) of the current cell.</param>
	/// <param name="element">
	///     When this method returns, contains the element below, if found; otherwise, the default value for
	///     the type T.
	/// </param>
	/// <returns>true if an element exists at (x, y+1); otherwise, false.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool TryGetElementBelow(int x, int y, out T? element) =>
		TryGetElementInDirection(x, y, Direction.Below, out element);

	/// <summary>
	///     Attempts to get the element directly below the specified cell.
	/// </summary>
	/// <param name="gridPosition">The Vector2Int containing x and y coordinates of the current cell.</param>
	/// <param name="element">
	///     When this method returns, contains the element below, if found; otherwise, the default value for
	///     the type T.
	/// </param>
	/// <returns>true if an element exists at (gridPosition.x, gridPosition.y+1); otherwise, false.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool TryGetElementBelow(Vector2Int gridPosition, out T? element) =>
		TryGetElementInDirection(gridPosition.x, gridPosition.y, Direction.Below, out element);

	/// <summary>
	///     Attempts to get the bottom-left diagonal element relative to the specified cell.
	/// </summary>
	/// <param name="x">The x-coordinate (column) of the current cell.</param>
	/// <param name="y">The y-coordinate (row) of the current cell.</param>
	/// <param name="element">
	///     When this method returns, contains the element at bottom-left, if found; otherwise, the default
	///     value for the type T.
	/// </param>
	/// <returns>true if an element exists at (x-1, y+1); otherwise, false.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool TryGetElementBottomLeft(int x, int y, out T? element) =>
		TryGetElementInDirection(x, y, Direction.BottomLeft, out element);

	/// <summary>
	///     Attempts to get the bottom-left diagonal element relative to the specified cell.
	/// </summary>
	/// <param name="gridPosition">The Vector2Int containing x and y coordinates of the current cell.</param>
	/// <param name="element">
	///     When this method returns, contains the element at bottom-left, if found; otherwise, the default
	///     value for the type T.
	/// </param>
	/// <returns>true if an element exists at (gridPosition.x-1, gridPosition.y+1); otherwise, false.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool TryGetElementBottomLeft(Vector2Int gridPosition, out T? element) =>
		TryGetElementInDirection(gridPosition.x, gridPosition.y, Direction.BottomLeft, out element);

	/// <summary>
	///     Attempts to get the bottom-right diagonal element relative to the specified cell.
	/// </summary>
	/// <param name="x">The x-coordinate (column) of the current cell.</param>
	/// <param name="y">The y-coordinate (row) of the current cell.</param>
	/// <param name="element">
	///     When this method returns, contains the element at bottom-right, if found; otherwise, the default
	///     value for the type T.
	/// </param>
	/// <returns>true if an element exists at (x+1, y+1); otherwise, false.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool TryGetElementBottomRight(int x, int y, out T? element) =>
		TryGetElementInDirection(x, y, Direction.BottomRight, out element);

	/// <summary>
	///     Attempts to get the bottom-right diagonal element relative to the specified cell.
	/// </summary>
	/// <param name="gridPosition">The Vector2Int containing x and y coordinates of the current cell.</param>
	/// <param name="element">
	///     When this method returns, contains the element at bottom-right, if found; otherwise, the default
	///     value for the type T.
	/// </param>
	/// <returns>true if an element exists at (gridPosition.x+1, gridPosition.y+1); otherwise, false.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool TryGetElementBottomRight(Vector2Int gridPosition, out T? element) =>
		TryGetElementInDirection(gridPosition.x, gridPosition.y, Direction.BottomRight, out element);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool TryGetElementInDirection(int x, int y, Direction direction, out T? element)
	{
		element = default;

		if (!IsWithinBounds(x, y)) return false;

		int newX = x, newY = y;

		switch (direction)
		{
			case Direction.Above:
				newY--;
				break;
			case Direction.Below:
				newY++;
				break;
			case Direction.Left:
				newX--;
				break;
			case Direction.Right:
				newX++;
				break;
			case Direction.TopLeft:
				newX--;
				newY--;
				break;
			case Direction.TopRight:
				newX++;
				newY--;
				break;
			case Direction.BottomLeft:
				newX--;
				newY++;
				break;
			case Direction.BottomRight:
				newX++;
				newY++;
				break;
			default:
				return false;
		}

		// Check bounds and return element if valid
		if (!IsWithinBounds(newX, newY)) return false;

		element = Elements[Index(newX, newY)];

		return true;
	}

	/// <summary>
	///     Attempts to get the element directly to the left of the specified cell.
	/// </summary>
	/// <param name="x">The x-coordinate (column) of the current cell.</param>
	/// <param name="y">The y-coordinate (row) of the current cell.</param>
	/// <param name="element">
	///     When this method returns, contains the element to the left, if found; otherwise, the default
	///     value for the type T.
	/// </param>
	/// <returns>true if an element exists at (x-1, y); otherwise, false.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool TryGetElementLeft(int x, int y, out T? element) =>
		TryGetElementInDirection(x, y, Direction.Left, out element);

	/// <summary>
	///     Attempts to get the element directly to the left of the specified cell.
	/// </summary>
	/// <param name="gridPosition">The Vector2Int containing x and y coordinates of the current cell.</param>
	/// <param name="element">
	///     When this method returns, contains the element to the left, if found; otherwise, the default
	///     value for the type T.
	/// </param>
	/// <returns>true if an element exists at (gridPosition.x-1, gridPosition.y); otherwise, false.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool TryGetElementLeft(Vector2Int gridPosition, out T? element) =>
		TryGetElementInDirection(gridPosition.x, gridPosition.y, Direction.Left, out element);

	/// <summary>
	///     Attempts to get the element directly to the right of the specified cell.
	/// </summary>
	/// <param name="x">The x-coordinate (column) of the current cell.</param>
	/// <param name="y">The y-coordinate (row) of the current cell.</param>
	/// <param name="element">
	///     When this method returns, contains the element to the right, if found; otherwise, the default
	///     value for the type T.
	/// </param>
	/// <returns>true if an element exists at (x+1, y); otherwise, false.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool TryGetElementRight(int x, int y, out T? element) =>
		TryGetElementInDirection(x, y, Direction.Right, out element);

	/// <summary>
	///     Attempts to get the element directly to the right of the specified cell.
	/// </summary>
	/// <param name="gridPosition">The Vector2Int containing x and y coordinates of the current cell.</param>
	/// <param name="element">
	///     When this method returns, contains the element to the right, if found; otherwise, the default
	///     value for the type T.
	/// </param>
	/// <returns>true if an element exists at (gridPosition.x+1, gridPosition.y); otherwise, false.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool TryGetElementRight(Vector2Int gridPosition, out T? element) =>
		TryGetElementInDirection(gridPosition.x, gridPosition.y, Direction.Right, out element);

	/// <summary>
	///     Attempts to get the top-left diagonal element relative to the specified cell.
	/// </summary>
	/// <param name="x">The x-coordinate (column) of the current cell.</param>
	/// <param name="y">The y-coordinate (row) of the current cell.</param>
	/// <param name="element">
	///     When this method returns, contains the element at top-left, if found; otherwise, the default
	///     value for the type T.
	/// </param>
	/// <returns>true if an element exists at (x-1, y-1); otherwise, false.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool TryGetElementTopLeft(int x, int y, out T? element) =>
		TryGetElementInDirection(x, y, Direction.TopLeft, out element);

	/// <summary>
	///     Attempts to get the top-left diagonal element relative to the specified cell.
	/// </summary>
	/// <param name="gridPosition">The Vector2Int containing x and y coordinates of the current cell.</param>
	/// <param name="element">
	///     When this method returns, contains the element at top-left, if found; otherwise, the default
	///     value for the type T.
	/// </param>
	/// <returns>true if an element exists at (gridPosition.x-1, gridPosition.y-1); otherwise, false.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool TryGetElementTopLeft(Vector2Int gridPosition, out T? element) =>
		TryGetElementInDirection(gridPosition.x, gridPosition.y, Direction.TopLeft, out element);

	/// <summary>
	///     Attempts to get the top-right diagonal element relative to the specified cell.
	/// </summary>
	/// <param name="x">The x-coordinate (column) of the current cell.</param>
	/// <param name="y">The y-coordinate (row) of the current cell.</param>
	/// <param name="element">
	///     When this method returns, contains the element at top-right, if found; otherwise, the default
	///     value for the type T.
	/// </param>
	/// <returns>true if an element exists at (x+1, y-1); otherwise, false.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool TryGetElementTopRight(int x, int y, out T? element) =>
		TryGetElementInDirection(x, y, Direction.TopRight, out element);

	/// <summary>
	///     Attempts to get the top-right diagonal element relative to the specified cell.
	/// </summary>
	/// <param name="gridPosition">The Vector2Int containing x and y coordinates of the current cell.</param>
	/// <param name="element">
	///     When this method returns, contains the element at top-right, if found; otherwise, the default
	///     value for the type T.
	/// </param>
	/// <returns>true if an element exists at (gridPosition.x+1, gridPosition.y-1); otherwise, false.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool TryGetElementTopRight(Vector2Int gridPosition, out T? element) =>
		TryGetElementInDirection(gridPosition.x, gridPosition.y, Direction.TopRight, out element);

	/// <summary>
	///     Gets the element at the specified indices.
	/// </summary>
	/// <param name="x">The x-coordinate (column).</param>
	/// <param name="y">The y-coordinate (row).</param>
	/// <returns>The element at [x, y].</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public T GetElementAt(int x, int y)
	{
		ValidateIndices(x, y);

		return elements[Index(x, y)];
	}

	/// <summary>
	///     Gets the element directly above the specified cell.
	/// </summary>
	/// <param name="x">The x-coordinate (column) of the current cell.</param>
	/// <param name="y">The y-coordinate (row) of the current cell.</param>
	/// <returns>The element at (x, y-1).</returns>
	/// <exception cref="IndexOutOfRangeException">Thrown if (x, y-1) is out of bounds.</exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public T? GetElementAbove(int x, int y) =>
		TryGetElementAbove(x, y, out var element)
			? element
			: default;

	/// <summary>
	///     Gets the element directly above the specified cell.
	/// </summary>
	/// <param name="gridPosition">The Vector2Int containing x and y coordinates of the current cell.</param>
	/// <returns>The element at (x, y-1).</returns>
	/// <exception cref="IndexOutOfRangeException">Thrown if (x, y-1) is out of bounds.</exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public T? GetElementAbove(Vector2Int gridPosition) =>
		GetElementAbove(gridPosition.x, gridPosition.y);

	/// <summary>
	///     Gets the element directly below the specified cell.
	/// </summary>
	/// <param name="x">The x-coordinate (column) of the current cell.</param>
	/// <param name="y">The y-coordinate (row) of the current cell.</param>
	/// <returns>The element at (x, y+1).</returns>
	/// <exception cref="IndexOutOfRangeException">Thrown if (x, y+1) is out of bounds.</exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public T? GetElementBelow(int x, int y) =>
		TryGetElementBelow(x, y, out var element)
			? element
			: default;

	/// <summary>
	///     Gets the element directly below the specified cell.
	/// </summary>
	/// <param name="gridPosition">The Vector2Int containing x and y coordinates of the current cell.</param>
	/// <returns>The element at (x, y+1).</returns>
	/// <exception cref="IndexOutOfRangeException">Thrown if (x, y+1) is out of bounds.</exception>
	public T? GetElementBelow(Vector2Int gridPosition) =>
		TryGetElementBelow(gridPosition, out var element)
			? element
			: default;

	/// <summary>
	///     Gets the bottom-left diagonal element relative to the specified cell.
	/// </summary>
	/// <param name="x">The x-coordinate (column) of the current cell.</param>
	/// <param name="y">The y-coordinate (row) of the current cell.</param>
	/// <returns>The element at (x-1, y+1).</returns>
	/// <exception cref="IndexOutOfRangeException">Thrown if (x-1, y+1) is out of bounds.</exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public T? GetElementBottomLeft(int x, int y) =>
		TryGetElementBottomLeft(x, y, out var element)
			? element
			: default;

	/// <summary>
	///     Gets the bottom-left diagonal element relative to the specified cell.
	/// </summary>
	/// <param name="gridPosition">The Vector2Int containing x and y coordinates of the current cell.</param>
	/// <returns>The element at (x-1, y+1).</returns>
	/// <exception cref="IndexOutOfRangeException">Thrown if (x-1, y+1) is out of bounds.</exception>
	public T? GetElementBottomLeft(Vector2Int gridPosition) =>
		TryGetElementBottomLeft(gridPosition, out var element)
			? element
			: default;

	/// <summary>
	///     Gets the bottom-right diagonal element relative to the specified cell.
	/// </summary>
	/// <param name="x">The x-coordinate (column) of the current cell.</param>
	/// <param name="y">The y-coordinate (row) of the current cell.</param>
	/// <returns>The element at (x+1, y+1).</returns>
	/// <exception cref="IndexOutOfRangeException">Thrown if (x+1, y+1) is out of bounds.</exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public T? GetElementBottomRight(int x, int y) =>
		TryGetElementBottomRight(x, y, out var element)
			? element
			: default;

	/// <summary>
	///     Gets the bottom-right diagonal element relative to the specified cell.
	/// </summary>
	/// <param name="gridPosition">The Vector2Int containing x and y coordinates of the current cell.</param>
	/// <returns>The element at (x+1, y+1).</returns>
	/// <exception cref="IndexOutOfRangeException">Thrown if (x+1, y+1) is out of bounds.</exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public T? GetElementBottomRight(Vector2Int gridPosition) =>
		TryGetElementBottomRight(gridPosition, out var element)
			? element
			: default;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public T? GetElementInDirection(int x, int y, Direction direction) =>
		TryGetElementInDirection(x, y, direction, out var element)
			? element
			: default;

	/// <summary>
	///     Gets the element directly to the left of the specified cell.
	/// </summary>
	/// <param name="x">The x-coordinate (column) of the current cell.</param>
	/// <param name="y">The y-coordinate (row) of the current cell.</param>
	/// <returns>The element at (x-1, y).</returns>
	/// <exception cref="IndexOutOfRangeException">Thrown if (x-1, y) is out of bounds.</exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public T? GetElementLeft(int x, int y) =>
		TryGetElementLeft(x, y, out var element)
			? element
			: default;

	/// <summary>
	///     Gets the element directly to the left of the specified cell.
	/// </summary>
	/// <param name="gridPosition">The Vector2Int containing x and y coordinates of the current cell.</param>
	/// <returns>The element at (x-1, y).</returns>
	/// <exception cref="IndexOutOfRangeException">Thrown if (x-1, y) is out of bounds.</exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public T? GetElementLeft(Vector2Int gridPosition) =>
		TryGetElementLeft(gridPosition, out var element)
			? element
			: default;

	/// <summary>
	///     Gets the element directly to the right of the specified cell.
	/// </summary>
	/// <param name="x">The x-coordinate (column) of the current cell.</param>
	/// <param name="y">The y-coordinate (row) of the current cell.</param>
	/// <returns>The element at (x+1, y).</returns>
	/// <exception cref="IndexOutOfRangeException">Thrown if (x+1, y) is out of bounds.</exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public T? GetElementRight(int x, int y) =>
		TryGetElementRight(x, y, out var element)
			? element
			: default;

	/// <summary>
	///     Gets the element directly to the right of the specified cell.
	/// </summary>
	/// <param name="gridPosition">The Vector2Int containing x and y coordinates of the current cell.</param>
	/// <returns>The element at (x+1, y).</returns>
	/// <exception cref="IndexOutOfRangeException">Thrown if (x+1, y) is out of bounds.</exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public T? GetElementRight(Vector2Int gridPosition) =>
		TryGetElementRight(gridPosition, out var element)
			? element
			: default;

	/// <summary>
	///     Gets the top-left diagonal element relative to the specified cell.
	/// </summary>
	/// <param name="x">The x-coordinate (column) of the current cell.</param>
	/// <param name="y">The y-coordinate (row) of the current cell.</param>
	/// <returns>The element at (x-1, y-1).</returns>
	/// <exception cref="IndexOutOfRangeException">Thrown if (x-1, y-1) is out of bounds.</exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public T? GetElementTopLeft(int x, int y) =>
		TryGetElementTopLeft(x, y, out var element)
			? element
			: default;

	/// <summary>
	///     Gets the top-left diagonal element relative to the specified cell.
	/// </summary>
	/// <param name="gridPosition">The Vector2Int containing x and y coordinates of the current cell.</param>
	/// <returns>The element at (x-1, y-1).</returns>
	/// <exception cref="IndexOutOfRangeException">Thrown if (x-1, y-1) is out of bounds.</exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public T? GetElementTopLeft(Vector2Int gridPosition) =>
		TryGetElementTopLeft(gridPosition, out var element)
			? element
			: default;

	/// <summary>
	///     Gets the top-right diagonal element relative to the specified cell.
	/// </summary>
	/// <param name="x">The x-coordinate (column) of the current cell.</param>
	/// <param name="y">The y-coordinate (row) of the current cell.</param>
	/// <returns>The element at (x+1, y-1).</returns>
	/// <exception cref="IndexOutOfRangeException">Thrown if (x+1, y-1) is out of bounds.</exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public T? GetElementTopRight(int x, int y) =>
		TryGetElementTopRight(x, y, out var element)
			? element
			: default;

	/// <summary>
	///     Gets the top-right diagonal element relative to the specified cell.
	/// </summary>
	/// <param name="gridPosition">The Vector2Int containing x and y coordinates of the current cell.</param>
	/// <returns>The element at (x+1, y-1).</returns>
	/// <exception cref="IndexOutOfRangeException">Thrown if (x+1, y-1) is out of bounds.</exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public T? GetElementTopRight(Vector2Int gridPosition) =>
		TryGetElementTopRight(gridPosition, out var element)
			? element
			: default;

	/// <summary>
	///     Gets all elements in the specified column.
	/// </summary>
	/// <param name="x">The x-coordinate (column).</param>
	/// <returns>An array with all elements in the column.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public T[] GetColumn(int x)
	{
		if (x < 0 || x >= width)
			throw new ArgumentOutOfRangeException(nameof(x), "Column index is out of range.");

		var column = new T[height];

		for (var y = 0; y < height; y++)
			column[y] = Elements[y * width + x];

		return column;
	}

	/// <summary>
	///     Returns a copy of the internal data array.
	/// </summary>
	/// <returns>A copy of the internal data</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public T[] GetDataCopy()
	{
		var source = Elements;
		var copy   = new T[source.Length];
		Array.Copy(source, copy, source.Length);

		return copy;
	}

	/// <summary>
	///     Gets all elements in the specified row.
	/// </summary>
	/// <param name="y">The y-coordinate (row).</param>
	/// <returns>An array with all elements in the row.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public T[] GetRow(int y)
	{
		if (y < 0 || y >= height)
			throw new ArgumentOutOfRangeException(nameof(y), "Row index is out of range.");

		var row = new T[width];
		Array.Copy(Elements, y * width, row, 0, width);

		return row;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Clear() =>
		Array.Clear(Elements, 0, Elements.Length);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Fill(T element) =>
		Array.Fill(Elements, element);

	/// <summary>
	///     Loads data from a 1D array.
	/// </summary>
	/// <param name="newData">Source data array.</param>
	/// <param name="newWidth">Width for the new data.</param>
	/// <param name="newHeight">Height for the new data.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void LoadFrom1DArray(T[] newData, int newWidth, int newHeight)
	{
		if (newData == null)
			throw new ArgumentNullException(nameof(newData), "Input array cannot be null.");

		if (newData.Length != newWidth * newHeight)
			throw new ArgumentException("Array length does not match the specified dimensions.");

		ValidateDimensions(newWidth, newHeight);

		if (Elements.Length != newData.Length) elements = new T[newWidth * newHeight];

		width  = newWidth;
		height = newHeight;
		Array.Copy(newData, elements, newData.Length);
	}

	/// <summary>
	///     Loads data from a 2D array.
	/// </summary>
	/// <param name="array2D">Source 2D array.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void LoadFrom2DArray(T[,] array2D)
	{
		if (array2D == null)
			throw new ArgumentNullException(nameof(array2D), "Input array cannot be null.");

		int arrayHeight = array2D.GetLength(0);

		int arrayWidth = array2D.GetLength(1);

		if (arrayHeight == 0 || arrayWidth == 0)
			throw new ArgumentException("Input array has zero dimensions.");

		if (width != arrayWidth || height != arrayHeight)
			throw new ArgumentException("Input array dimensions do not match the defined width and height.");

		if (Elements.Length != width * height) elements = new T[width * height];

		for (var y = 0; y < height; y++)
		{
			for (var x = 0; x < width; x++)
				// Standard C# 2D arrays use [row, column] format
				Elements[y * width + x] = array2D[y, x];
		}
	}

	// <summary>
	/// Initializes or re-initializes the internal array based on the current width and height.
	/// All existing data will be lost.
	/// </summary>
	public void Reinitialize()
	{
		ValidateDimensions(width, height);

		elements = new T[width * height];
	}

	/// <summary>
	///     Initializes or re-initializes the internal array based on the current width and height,
	///     filling it with the specified value.
	///     All existing data will be lost.
	/// </summary>
	/// <param name="defaultValue">The value to fill the array with.</param>
	public void Reinitialize(T defaultValue)
	{
		Reinitialize();
		Fill(defaultValue);
	}

	/// <summary>
	///     Resizes the array to new dimensions.
	/// </summary>
	/// <param name="newWidth">New width.</param>
	/// <param name="newHeight">New height.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Resize(int newWidth, int newHeight)
	{
		ValidateDimensions(newWidth, newHeight);

		if (newWidth == width && newHeight == height) return;

		var newData = new T[newWidth * newHeight];

		// Copy existing data within the bounds of both old and new dimensions
		int minWidth = Math.Min(width, newWidth);

		int minHeight = Math.Min(height, newHeight);

		for (var y = 0; y < minHeight; y++)
		{
			for (var x = 0; x < minWidth; x++)
			{
				int oldIndex = y * width + x;
				int newIndex = y * newWidth + x;
				newData[newIndex] = Elements[oldIndex];
			}
		}

		width    = newWidth;
		height   = newHeight;
		elements = newData;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void SetElementAt(int x, int y, T value)
	{
		ValidateIndices(x, y);

		Elements[Index(x, y)] = value;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void SetElementAt(Vector2Int gridPosition, T value) =>
		SetElementAt(gridPosition.x, gridPosition.y, value);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void ValidateDimensions(int width, int height)
	{
		if (width <= 0)
			throw new ArgumentOutOfRangeException(nameof(width), "Width must be greater than zero.");

		if (height <= 0)
			throw new ArgumentOutOfRangeException(nameof(height), "Height must be greater than zero.");
	}

	/// <summary>
	///     Checks if the given coordinates are within the bounds of the array.
	/// </summary>
	/// <param name="x">The x-coordinate (column).</param>
	/// <param name="y">The y-coordinate (row).</param>
	/// <returns>True if the coordinates are within bounds, otherwise false.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private bool IsWithinBounds(int x, int y) =>
		x >= 0 && x < width && y >= 0 && y < height;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private int Index(int x, int y) => y * width + x;

	[Conditional("UNITY_EDITOR")]
	private void CheckSerializability()
	{
#if UNITY_EDITOR

		// Check if T is serializable to warn developers about potential serialization issues
		if (!typeof(T).IsValueType &&
			!typeof(Object).IsAssignableFrom(typeof(T)) &&
			!Attribute.IsDefined(typeof(T), typeof(SerializableAttribute)))
			Debug.LogWarning($"{typeof(T).Name} may not be serializable by Unity in SerializableArray2D<T>.");
#endif
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void ValidateIndices(int x, int y)
	{
		if (!IsWithinBounds(x, y))
			throw new IndexOutOfRangeException($"Index [{x},{y}] is out of range.");
	}

	#region Interface Implementations

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public IEnumerator GetEnumerator() =>
		Elements.GetEnumerator();

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	IEnumerator<T> IEnumerable<T>.GetEnumerator() => ((IEnumerable<T>)Elements).GetEnumerator();

	#endregion
}

/// <summary>
///     Lists the supported Direction values.
/// </summary>
public enum Direction
{
	Above,
	Below,
	Left,
	Right,
	TopLeft,
	TopRight,
	BottomLeft,
	BottomRight
}
