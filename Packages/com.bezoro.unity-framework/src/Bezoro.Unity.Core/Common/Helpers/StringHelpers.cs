using System;

namespace Bezoro.Unity.Core.Common.Helpers;

/// <summary>
///     Parses string values used by chess notation and Unity utilities.
/// </summary>
public static class StringHelpers
{
	public static (int fileIndex, int rankIndex) ChessNotationToIndices(string notation)
	{
		if (string.IsNullOrWhiteSpace(notation) || notation.Length < 2)
			throw new ArgumentException("Notation must contain a file and rank.", nameof(notation));

		string file = notation[..1];
		string rank = notation[1..];

		if (!char.IsLetter(file[0]))
			throw new ArgumentException("Notation file must be a letter.", nameof(notation));

		return (ChessNotationFileToInt(
					file
				), ChessNotationRankToInt(
					rank
				));
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
		if (string.IsNullOrEmpty(file))
			throw new ArgumentException("File is Null or Empty.", nameof(file));

		if (file.Length != 1)
			throw new ArgumentException("File must be a single letter.", nameof(file));

		var fileChar = char.ToUpper(file[0]);

		if (fileChar < 'A' || fileChar > 'Z')
			throw new ArgumentOutOfRangeException(nameof(file), "File must be a letter between A and Z.");

		return fileChar - 'A';
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
		if (!int.TryParse(
				rank,
				out int result
			))
			throw new ArgumentException(
				"Rank must be a valid integer.",
				nameof(rank)
			);

		if (result < 1 || result > 26)
			throw new ArgumentOutOfRangeException(
				nameof(rank),
				"Rank must be between 1 and 26."
			);

		return result - 1;
	}
}
