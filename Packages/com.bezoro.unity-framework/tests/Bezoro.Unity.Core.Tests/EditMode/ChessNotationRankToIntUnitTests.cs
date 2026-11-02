using Bezoro.Unity.Core.Common.Helpers;
using NUnit.Framework;

namespace Bezoro.Unity.Core.Tests.EditMode
{
	/// <summary>
	///     Verifies conversion from chess notation rank strings to zero-based indices.
	/// </summary>
	[TestFixture]
	public class ChessNotationRankToIntUnitTests
	{
		[Test]
		public void ValidRankNotation_ReturnsCorrectIndex(
			[Range(
				1,
				26
			)]
			int index
		)
		{
			int rank          = index;
			int expectedIndex = rank - 1;

			int actualIndex = StringHelpers.ChessNotationRankToInt(
				rank.ToString()
			);

			Assert.That(
				actualIndex,
				Is.EqualTo(
					expectedIndex
				),
				$"Failed for rank {rank}"
			);
		}
	}
}
