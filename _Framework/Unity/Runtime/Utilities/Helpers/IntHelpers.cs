using System;

namespace Bezoro.Framework.Unity.Utilities.Helpers
{
	public static class IntHelpers
	{
		/// <summary>
		///     Converts two integer indices to a chess notation square.
		/// </summary>
		/// <param name="x">The index of the x-coordinate to convert.</param>
		/// <param name="y">The index of the y-coordinate to convert.</param>
		/// <returns>The chess notation for the given square.</returns>
		public static string IntToChessNotation(int x, int y)
		{
			var file = IntToChessNotationFile(x);
			var rank = IntToChessNotationRank(y);

			return $"{file}{rank}";
		}

		/// <summary>
		///     Converts an integer index to a chess notation rank.
		/// </summary>
		/// <param name="i">The index of the y-coordinate to convert.</param>
		/// <returns>The chess notation for the given rank.</returns>
		/// <exception cref="ArgumentOutOfRangeException">
		///     <paramref name="i" /> is negative or greater than 25.
		/// </exception>
		public static string IntToChessNotationRank(int i)
		{
			if (i < 0)
			{
				throw new ArgumentOutOfRangeException(
					nameof(i),
					"Rank must be non-negative."
				);
			}

			if (i > 25)
			{
				throw new ArgumentOutOfRangeException(
					nameof(i),
					"Rank must be less than 26 to fit within a-z range."
				);
			}

			return ( i + 1 ).ToString();
		}

		/// <summary>
		///     Converts an integer to a chess notation file.
		/// </summary>
		/// <param name="i">The index of the x-coordinate to convert.</param>
		/// <returns>The chess notation for the given index.</returns>
		/// <exception cref="ArgumentOutOfRangeException">
		///     <paramref name="i" /> is negative or greater than 25.
		/// </exception>
		public static string IntToChessNotationFile(int i)
		{
			if (i < 0)
			{
				throw new ArgumentOutOfRangeException(
					nameof(i),
					"File must be non-negative."
				);
			}

			if (i > 25)
			{
				throw new ArgumentOutOfRangeException(
					nameof(i),
					"File must be less than 26 to fit within a-z range."
				);
			}

			return ( (char)( 'a' + i ) ).ToString().ToUpper();
		}
	}
}
