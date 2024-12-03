using System;
using UnityEngine;

namespace Bezoro.Framework.Unity.Serializable2DArray
{
	[Serializable]
	public class Serializable2DArray<T>
	{
		[SerializeField] private T[] data;
		[SerializeField] private int width;
		[SerializeField] private int height;

		public Serializable2DArray( int width, int height )
		{
			if ( width <= 0 || height <= 0 )
			{
				throw new ArgumentException( "Width and height must be greater than zero." );
			}

			this.width  = width;
			this.height = height;
		}

		public Serializable2DArray( T[,] array2D )
		{
			if ( array2D == null )
			{
				throw new ArgumentNullException(
					nameof( array2D ),
					"Input array cannot be null." );
			}

			var newWidth  = array2D.GetLength( 0 );
			var newHeight = array2D.GetLength( 1 );

			if ( newWidth <= 0 || newHeight <= 0 )
			{
				throw new ArgumentException( "Width and height must be greater than zero." );
			}

			width  = newWidth;
			height = newHeight;
			FromArray2D( array2D );
		}

		public Serializable2DArray( T[] array1D, int width, int height )
		{
			if ( array1D == null )
			{
				throw new ArgumentNullException(
					nameof( array1D ),
					"Input array cannot be null." );
			}

			if ( width <= 0 || height <= 0 )
			{
				throw new ArgumentException( "Width and height must be greater than zero." );
			}

			if ( array1D.Length != width * height )
			{
				throw new ArgumentException
					( "The length of the input array does not match the specified dimensions." );
			}

			this.width  = width;
			this.height = height;
			SetData(
				array1D,
				width,
				height );
		}

		public int Width => width;

		public int Height => height;

		public T[] Data => data ??= new T[width * height];

		public T this[ int x, int y ]
		{
			get
			{
				if ( x < 0 || x >= width || y < 0 || y >= height )
				{
					throw new IndexOutOfRangeException( $"Index [{x},{y}] is out of range." );
				}

				return data[ y * width + x ];
			}
			set
			{
				if ( x < 0 || x >= width || y < 0 || y >= height )
				{
					throw new IndexOutOfRangeException( $"Index [{x},{y}] is out of range." );
				}

				data[ y * width + x ] = value;
			}
		}

		public void Resize( int newWidth, int newHeight )
		{
			if ( newWidth <= 0 || newHeight <= 0 )
			{
				throw new ArgumentException( "New width and height must be greater than zero." );
			}

			if ( newWidth == width && newHeight == height )
			{
				return; // No need to resize if dimensions are the same
			}

			var newData = new T[newWidth * newHeight];
			var minWidth = Math.Min(
				width,
				newWidth );

			var minHeight = Math.Min(
				height,
				newHeight );

			for ( var y = 0 ; y < minHeight ; y++ )
			{
				Array.Copy(
					Data,
					y * width,
					newData,
					y * newWidth,
					minWidth );
			}

			data   = newData;
			width  = newWidth;
			height = newHeight;
		}

		public T[,] To2DArray()
		{
			var array2D = new T[width, height];

			for ( var y = 0 ; y < height ; y++ )
			{
				for ( var x = 0 ; x < width ; x++ )
				{
					array2D[ x,
							 y ] = this[ x,
										 y ];
				}
			}

			return array2D;
		}

		public void SetData( T[] newData, int newWidth, int newHeight )
		{
			if ( newData.Length != newWidth * newHeight )
			{
				throw new ArgumentException( "Array length does not match the specified dimensions." );
			}

			width  = newWidth;
			height = newHeight;
			data   = new T[width * height];
			Array.Copy(
				newData,
				data,
				newData.Length );
		}

		private void FromArray2D( T[,] array2D )
		{
			width  = array2D.GetLength( 0 );
			height = array2D.GetLength( 1 );
			data   = new T[width * height];

			for ( var y = 0 ; y < height ; y++ )
			{
				for ( var x = 0 ; x < width ; x++ )
				{
					data[ y * width + x ] = array2D[ x,
													 y ];
				}
			}
		}

		public static implicit operator T[]( Serializable2DArray<T> array )
		{
			return array.data;
		}

		public static implicit operator Serializable2DArray<T>( T[] array1D )
		{
			var size = (int)Math.Sqrt( array1D.Length );

			if ( size * size != array1D.Length )
			{
				throw new ArgumentException( "Array length must be a perfect square for implicit conversion." );
			}

			return new(
				array1D,
				size,
				size );
		}

		public static implicit operator Serializable2DArray<T>( T[,] array2D )
		{
			return new( array2D );
		}

		public static explicit operator T[,]( Serializable2DArray<T> serializable2DArray )
		{
			return serializable2DArray.To2DArray();
		}
	}
}
