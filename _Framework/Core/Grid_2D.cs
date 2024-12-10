using System;

namespace Bezoro.Framework.Core
{
	[Serializable]
	public class Grid_2D<T>
	{
		public Grid_2D( T[,] data )
		{
			Assert.Is_Not_Null_Or_Empty( data );

			Assert.Is_True(
				data.GetLength( 0 ) is >= _MIN_VALUE and <= _MAX_VALUE,
				$"{nameof(Width)} must be greater than 0 and less than 27"
			);

			Assert.Is_True(
				data.GetLength( 1 ) is >= _MIN_VALUE and <= _MAX_VALUE,
				$"{nameof(Height)} must be greater than 0 and less than 27"
			);

			Data   = data;
			Width  = data.GetLength( 0 );
			Height = data.GetLength( 1 );
		}

		public Grid_2D( int width, int height )
		{
			Assert.Is_True(
				width is >= _MIN_VALUE and <= _MAX_VALUE,
				$"{nameof(Width)} must be greater than 0 and less than 27"
			);

			Assert.Is_True(
				height is >= _MIN_VALUE and <= _MAX_VALUE,
				$"{nameof(Height)} must be greater than 0 and less than 27"
			);

			Width  = width;
			Height = height;
			Data   = new T[width, height];
		}

		private const int _MAX_VALUE = 26;
		private const int _MIN_VALUE = 1;

		public T[,] Data   { get; }
		public int  Height { get; }
		public int  Width  { get; }

		public char Int_To_Char( int number )
		{
			Assert.Is_True( number is >= 1 and <= 26, $"Number must be between 1 and 26. Given: {number}" );

			return (char)( number - 1 + 'A' );
		}

		public int Char_To_Int( char letter )
		{
			Assert.Is_True( letter is >= 'A' and <= 'Z', $"Letter must be between A and Z. Given: {letter}" );

			return letter - 'A' + 1;
		}
	}
}
