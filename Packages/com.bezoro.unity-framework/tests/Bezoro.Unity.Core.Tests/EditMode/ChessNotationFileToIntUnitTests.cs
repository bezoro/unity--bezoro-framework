using Bezoro.Unity.Core.Common.Helpers;
using NUnit.Framework;

namespace Bezoro.Unity.Core.Tests.EditMode
{
	/// <summary>
	///     Verifies conversion from chess notation file letters to zero-based indices.
	/// </summary>
	[TestFixture]
	public class ChessNotationFileToIntUnitTests
	{
		[Test]
		public void ValidFileNotation_ReturnsCorrectIndex(
			[Range(
				0,
				25
			)]
			int index
		)
		{
			var file          = (char)('a' + index);
			int expectedIndex = index;

			int actualIndex = StringHelpers.ChessNotationFileToInt(
				file.ToString()
			);

			Assert.That(
				actualIndex,
				Is.EqualTo(
					expectedIndex
				),
				$"Failed for file {file}"
			);
		}
	}
}
