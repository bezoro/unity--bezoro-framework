using Bezoro.Framework.Unity.Utilities;
using NUnit.Framework;

namespace Bezoro.Framework.Unity.Tests.EditMode
{
	[TestFixture]
	public class ChessNotationRankToIntUnitTests
	{
		[Test]
		public void ValidRankNotation_ReturnsCorrectIndex(
			[Range(
				1,
				26 )]
			int index
		)
		{
			var rank          = index;
			var expectedIndex = rank - 1;

			var actualIndex = StringHelpers.ChessNotationRankToInt( rank.ToString() );

			Assert.That(
				actualIndex,
				Is.EqualTo( expectedIndex ),
				$"Failed for rank {rank}" );
		}
	}
}
