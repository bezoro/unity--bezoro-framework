using System;
using UnityEngine;

namespace Bezoro.Framework.Unity
{
	[Serializable]
	public class Serializable_2D_Array<T>
	{
		public Serializable_2D_Array( int width, int height )
		{
			if ( width <= 0 || height <= 0 )
				throw new ArgumentException( "Width and height must be greater than zero." );

			this.width  = width;
			this.height = height;
		}

		public Serializable_2D_Array( T[,] array_2d )
		{
			if ( array_2d == null )
				throw new ArgumentNullException( nameof(array_2d), "Input array cannot be null." );

			var array_x = array_2d.GetLength( 0 );
			var array_y = array_2d.GetLength( 1 );

			if ( array_x <= 0 || array_y <= 0 )
				throw new ArgumentException( "Width and height must be greater than zero." );

			width  = array_x;
			height = array_y;
			Set_Data_From_2D_Array( array_2d );
		}

		public Serializable_2D_Array( T[] array1D, int width, int height )
		{
			if ( array1D == null )
				throw new ArgumentNullException( nameof(array1D), "Input array cannot be null." );

			if ( width <= 0 || height <= 0 )
				throw new ArgumentException( "Width and height must be greater than zero." );

			if ( array1D.Length != width * height )
				throw new ArgumentException( "The length of the input array does not match the specified dimensions." );

			this.width  = width;
			this.height = height;
			Set_Data_From_1D_Array( array1D, width, height );
		}

		[SerializeField] private T[] data;
		[SerializeField] private int width;
		[SerializeField] private int height;

		public T[] Data => data ??= new T[width * height];

		public int Height => height;

		public T this[ int x, int y ]
		{
			get
			{
				if ( x < 0 || x >= width || y < 0 || y >= height )
					throw new IndexOutOfRangeException( $"Index [{x},{y}] is out of range." );

				return data[y * width + x];
			}
			set
			{
				if ( x < 0 || x >= width || y < 0 || y >= height )
					throw new IndexOutOfRangeException( $"Index [{x},{y}] is out of range." );

				data[y * width + x] = value;
			}
		}

		public int Width => width;

		public void Resize( int new_width, int new_height )
		{
			if ( new_width <= 0 )
				throw new ArgumentOutOfRangeException( nameof(new_width), "New width must be greater than zero." );

			if ( new_height <= 0 )
				throw new ArgumentOutOfRangeException( nameof(new_height), "New height must be greater than zero." );

			if ( new_width == width && new_height == height ) return;

			var new_data   = new T[new_width * new_height];
			var min_width  = Math.Min( width, new_width );
			var min_height = Math.Min( height, new_height );

			for ( var i = 0 ; i < min_height ; i++ )
				Array.Copy( data, i * width, new_data, i * new_width, min_width );

			data   = new_data;
			width  = new_width;
			height = new_height;
		}

		public T[,] To_2D_Array()
		{
			var array_2d = new T[width, height];

			for ( var y = 0 ; y < height ; y++ )
			{
				for ( var x = 0 ; x < width ; x++ )
					array_2d[x, y] = this[x, y];
			}

			return array_2d;
		}

		public void Set_Data_From_1D_Array( T[] newData, int newWidth, int newHeight )
		{
			if ( newData.Length != newWidth * newHeight )
				throw new ArgumentException( "Array length does not match the specified dimensions." );

			width  = newWidth;
			height = newHeight;
			data   = new T[width * height];
			Array.Copy( newData, data, newData.Length );
		}

		public static implicit operator T[]( Serializable_2D_Array<T> source )
		{
			return source.data;
		}

		private void Set_Data_From_2D_Array( T[,] array2D )
		{
			width  = array2D.GetLength( 0 );
			height = array2D.GetLength( 1 );
			data   = new T[width * height];

			for ( var y = 0 ; y < height ; y++ )
			{
				for ( var x = 0 ; x < width ; x++ )
					data[y * width + x] = array2D[x, y];
			}
		}
	}
}
