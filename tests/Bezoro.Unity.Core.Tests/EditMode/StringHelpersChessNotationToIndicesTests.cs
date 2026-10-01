using System;
using Bezoro.Unity.Core.Common.Helpers;
using NUnit.Framework;

namespace Bezoro.Unity.Core.Tests.EditMode
{
	/// <summary>
	///     Verifies combined chess notation parsing into zero-based file and rank indices.
	/// </summary>
	[TestFixture]
	public sealed class StringHelpersChessNotationToIndicesTests
	{
		[TestCase("aa1")]
		[TestCase("11")]
		public void ChessNotationToIndices_WhenFileIsInvalid_Throws(string notation)
		{
			Assert.Throws<ArgumentException>(() => StringHelpers.ChessNotationToIndices(notation));
		}

		[TestCase(null)]
		[TestCase("")]
		[TestCase(" ")]
		[TestCase("a")]
		public void ChessNotationToIndices_WhenNotationIsMissingFileOrRank_ThrowsArgumentException(string notation)
		{
			Assert.Throws<ArgumentException>(() => StringHelpers.ChessNotationToIndices(notation));
		}

		[TestCase("a1",  0,  0)]
		[TestCase("H8",  7,  7)]
		[TestCase("z26", 25, 25)]
		public void ChessNotationToIndices_WhenNotationIsValid_ReturnsFileAndRankIndices(
			string notation,
			int    expectedFile,
			int    expectedRank)
		{
			var indices = StringHelpers.ChessNotationToIndices(notation);

			Assert.AreEqual(expectedFile, indices.fileIndex);
			Assert.AreEqual(expectedRank, indices.rankIndex);
		}

		[TestCase("a0")]
		[TestCase("a27")]
		public void ChessNotationToIndices_WhenRankIsOutOfRange_Throws(string notation)
		{
			Assert.Throws<ArgumentOutOfRangeException>(() => StringHelpers.ChessNotationToIndices(notation));
		}
	}
}
