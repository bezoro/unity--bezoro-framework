using System;
using UnityEngine;
using Array = System.Array;

namespace Bezoro.Framework.Unity
{
	/// <summary>
	///     Represents a serializable 2D array.
	/// </summary>
	/// <typeparam name="T">The type of elements in the array.</typeparam>
	[Serializable]
	public class Serializable_2D_Array<T>
	{
		public Serializable_2D_Array(int width, int height)
		{
			Throw_If_Dimensions_Not_Positive(width, height);

			this.width  = width;
			this.height = height;
			data        = new T[width * height];
		}

		public Serializable_2D_Array(T[,] array_2d)
		{
			if (array_2d == null)
				throw new ArgumentNullException(nameof(array_2d), "Input array cannot be null.");

			var array_x = array_2d.GetLength(0);
			var array_y = array_2d.GetLength(1);

			Throw_If_Dimensions_Not_Positive(array_x, array_y);

			width  = array_x;
			height = array_y;
			Load_From_2D_Array(array_2d);
		}

		public Serializable_2D_Array(T[] array1D, int width, int height)
		{
			if (array1D == null)
				throw new ArgumentNullException(nameof(array1D), "Input array cannot be null.");

			Throw_If_Dimensions_Not_Positive(width, height);

			if (array1D.Length != width * height)
				throw new ArgumentException("The length of the input array does not match the specified dimensions.");

			this.width  = width;
			this.height = height;
			Load_From_1D_Array(array1D, width, height);
		}

		[SerializeField] private T[] data;
		[SerializeField] private int width;
		[SerializeField] private int height;

		public T[] Data   => data ??= new T[width * height];
		public int Height => height;

		/// <summary>
		///     Gets or sets the element at the specified indices.
		/// </summary>
		/// <param name="x">The x-coordinate.</param>
		/// <param name="y">The y-coordinate.</param>
		/// <returns>The element at [x, y].</returns>
		public T this[int x, int y]
		{
			get
			{
				if (x < 0 || x >= width || y < 0 || y >= height)
					throw new IndexOutOfRangeException($"Index [{x},{y}] is out of range.");

				return data[y * width + x];
			}
			set
			{
				if (x < 0 || x >= width || y < 0 || y >= height)
					throw new IndexOutOfRangeException($"Index [{x},{y}] is out of range.");

				data[y * width + x] = value;
			}
		}

		public int Width => width;

		public void Load_From_1D_Array(T[] new_data, int new_width, int new_height)
		{
			if (new_data == null)
				throw new ArgumentNullException(nameof(new_data), "Input array cannot be null.");

			if (new_data.Length != new_width * new_height)
				throw new ArgumentException("Array length does not match the specified dimensions.");

			Throw_If_Dimensions_Not_Positive(new_width, new_height);

			if (data == null || data.Length != new_data.Length)
				data = new T[new_width * new_height];

			width  = new_width;
			height = new_height;

			Array.Copy(new_data, data, new_data.Length);
		}

		public void Load_From_2D_Array(T[,] array2D)
		{
			if (array2D == null || array2D.Length == 0)
			{
				Logger.LogWarning("Input array is null or empty.");
				return;
			}

			var array_width  = array2D.GetLength(1);
			var array_height = array2D.GetLength(0);

			if (width != array_width || height != array_height)
				throw new ArgumentException("Input array dimensions do not match the defined width and height.");

			if (data == null || data.Length != width * height)
				data = new T[width * height];

			for (var y = 0 ; y < height ; y++)
			{
				for (var x = 0 ; x < width ; x++)
					data[y * width + x] = array2D[y, x];
			}
		}

		public static implicit operator T[](Serializable_2D_Array<T> source)
		{
			return source.data;
		}

		public void Resize(int new_width, int new_height)
		{
			if (new_width <= 0)
				throw new ArgumentOutOfRangeException(nameof(new_width), "New width must be greater than zero.");

			if (new_height <= 0)
				throw new ArgumentOutOfRangeException(nameof(new_height), "New height must be greater than zero.");

			if (new_width == width && new_height == height)
			{
				Logger.LogWarning("New dimensions are the same as the current dimensions.");
				return;
			}

			var new_data   = new T[new_width * new_height];
			var min_width  = Math.Min(width, new_width);
			var min_height = Math.Min(height, new_height);

			for (var i = 0 ; i < min_height ; i++)
				Array.Copy(data, i * width, new_data, i * new_width, min_width);

			data   = new_data;
			width  = new_width;
			height = new_height;
			Logger.LogSuccess($"Array resized to {width}x{height}.");
		}

		public T[,] To_2D_Array()
		{
			var array_2d = new T[width, height];

			for (var y = 0 ; y < height ; y++)
			{
				for (var x = 0 ; x < width ; x++)
					array_2d[x, y] = this[x, y];
			}

			return array_2d;
		}

		private bool Throw_If_Dimensions_Not_Positive(int desired_width, int desired_height)
		{
			if (desired_width <= 0)
				throw new ArgumentOutOfRangeException(nameof(desired_width), "Width must be greater than zero.");

			if (desired_height <= 0)
				throw new ArgumentOutOfRangeException(nameof(desired_height), "Height must be greater than zero.");

			return true;
		}
	}
}
