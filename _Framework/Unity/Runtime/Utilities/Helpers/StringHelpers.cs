using System;

namespace Bezoro.Framework.Unity.Utilities.Helpers
{
	public static class StringHelpers
	{
		public static (int fileIndex, int rankIndex) ChessNotationToIndices(string notation)
		{
			var file = notation[..1];
			var rank = notation[1..];

			return ( ChessNotationFileToInt(file), ChessNotationRankToInt(rank) );
		}

		/// <summary>
		///     Converts a chess notation rank to an integer index.
		/// </summary>
		/// <param name="rank">The chess notation rank to convert.</param>
		/// <returns>The integer index corresponding to the given rank.</returns>
		/// <exception cref="ArgumentOutOfRangeException">
		///     <paramref name="rank" /> is less than 1 or greater than 26.
		/// </exception>
		public static int ChessNotationRankToInt(string rank)
		{
			if (!int.TryParse(rank, out var result))
			{
				throw new ArgumentException(
					"Rank must be a valid integer.",
					nameof(rank)
				);
			}

			if (result < 1 || result > 26)
			{
				throw new ArgumentOutOfRangeException(
					nameof(rank),
					"Rank must be between 1 and 26."
				);
			}

			return result - 1;
		}

		/// <summary>
		///     Converts a chess notation file to an integer index.
		/// </summary>
		/// <param name="file">The chess notation file to convert.</param>
		/// <returns>The integer index corresponding to the given file.</returns>
		/// <exception cref="ArgumentException">
		///     <paramref name="file" /> is not a single letter.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">
		///     <paramref name="file" /> is not between 'A' and 'Z'.
		/// </exception>
		public static int ChessNotationFileToInt(string file)
		{
			if (file.Length != 1)
			{
				throw new ArgumentException(
					"File must be a single letter.",
					nameof(file)
				);
			}

			var fileChar = char.ToUpper(file[0]);

			if (fileChar < 'A' || fileChar > 'Z')
			{
				throw new ArgumentOutOfRangeException(
					nameof(file),
					"File must be a letter between A and Z."
				);
			}

			return fileChar - 'A';
		}
	}
}
