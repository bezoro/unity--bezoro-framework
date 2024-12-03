using Bezoro.Framework.Unity.Utilities;
using NUnit.Framework;

namespace Bezoro.Framework.Unity.Tests.EditMode
{
	[TestFixture]
	public class ChessNotationFileToIntUnitTests
	{
		[Test]
		public void ValidFileNotation_ReturnsCorrectIndex(
			[Range(
				0,
				25 )]
			int index
		)
		{
			var file          = (char)( 'a' + index );
			var expectedIndex = index;

			var actualIndex = StringHelpers.ChessNotationFileToInt( file.ToString() );

			Assert.That(
				actualIndex,
				Is.EqualTo( expectedIndex ),
				$"Failed for file {file}" );
		}
	}
}
