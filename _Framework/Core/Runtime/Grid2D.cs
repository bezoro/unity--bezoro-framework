using System;

namespace Bezoro.Framework.Core
{
	[Serializable]
	public class Grid2D<T> : Grid2D
	{
		public Grid2D(T[,] data) : base(data.GetLength(0), data.GetLength(1))
		{
			Assert.Is_Not_Null_Or_Empty(data);

			Data = data;
		}

		public T[,] Data { get; }

		public char Int_To_Char(int number)
		{
			Assert.IsTrue(number is >= 1 and <= 26, $"Number must be between 1 and 26. Given: {number}");

			return (char)( number - 1 + 'A' );
		}

		public int Char_To_Int(char letter)
		{
			Assert.IsTrue(letter is >= 'A' and <= 'Z', $"Letter must be between A and Z. Given: {letter}");

			return letter - 'A' + 1;
		}
	}

	public class Grid2D
	{
		public Grid2D(int width, int height)
		{
			Assert.IsTrue(
				width is >= _MIN_VALUE and <= _MAX_VALUE,
				$"{nameof(Width)} must be greater than 0 and less than 27"
			);

			Assert.IsTrue(
				height is >= _MIN_VALUE and <= _MAX_VALUE,
				$"{nameof(Height)} must be greater than 0 and less than 27"
			);

			Width  = width;
			Height = height;

			Logger.LogSuccess($"{nameof(Grid2D)} created with size: {Width}x{Height}.");
		}

		private const int _MAX_VALUE = 26;
		private const int _MIN_VALUE = 1;

		public int Height { get; }
		public int Width  { get; }
	}
}
