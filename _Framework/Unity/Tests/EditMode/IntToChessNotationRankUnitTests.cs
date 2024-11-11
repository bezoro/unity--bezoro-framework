using System;
using Bezoro.Framework.Unity.Utilities.Helpers;
using NUnit.Framework;

namespace Bezoro.Framework.Unity.Tests.EditMode
{
	[TestFixture]
	public class IntToChessNotationRankUnitTests
	{
		// Positive tests

		[TestCase(0, "A")]
		[TestCase(7, "H")]
		[TestCase(25, "Z")]
		public void ValidInput_ReturnsCorrectNotation(int y, string expectedNotation)
		{
			// Act
			var actualNotation = IntHelpers.IntToChessNotationFile(y);

			// Assert
			Assert.AreEqual(
				expectedNotation,
				actualNotation,
				$"For input {y}, expected {expectedNotation}, but got {actualNotation}"
			);
		}

		[Test]
		public void ValidInput_ReturnsCorrectNotationForRandomValues()
		{
			// Arrange
			var random = new Random();

			for ( var i = 0 ; i < 10 ; i++ )
			{
				var y = random.Next(0, 26);
				var expectedNotation = $"{(char)( 'A' + y )}";

				// Act
				var actualNotation = IntHelpers.IntToChessNotationFile(y);

				// Assert
				Assert.AreEqual(
					expectedNotation,
					actualNotation,
					$"For input {y}, expected {expectedNotation}, but got {actualNotation}"
				);
			}
		}

		// Negative tests

		[TestCase(-1)]
		[TestCase(-10)]
		[TestCase(-50)]
		public void NegativeInput_ThrowsArgumentOutOfRangeException(int negativeValue)
		{
			// Act & Assert
			Assert.Throws<ArgumentOutOfRangeException>(
				() => IntHelpers.IntToChessNotationRank(negativeValue),
				$"Expected ArgumentOutOfRangeException for x = {negativeValue}"
			);
		}

		[TestCase(26)]
		[TestCase(50)]
		[TestCase(100)]
		public void TooLargeInput_ThrowsArgumentOutOfRangeException(int largeValue)
		{
			// Act & Assert
			Assert.Throws<ArgumentOutOfRangeException>(
				() => IntHelpers.IntToChessNotationRank(largeValue),
				$"Expected ArgumentOutOfRangeException for x = {largeValue}"
			);
		}
	}
}
