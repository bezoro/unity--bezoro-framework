using System;
using Bezoro.Unity.Core.Common.Helpers;
using NUnit.Framework;

namespace Bezoro.Unity.Core.Tests.EditMode
{
	/// <summary>
	///     Verifies conversion from zero-based indices to chess notation rank strings.
	/// </summary>
	[TestFixture]
	public class IntToChessNotationRankUnitTests
	{
		// Negative tests

		[TestCase(-1)]
		[TestCase(-10)]
		[TestCase(-50)]
		public void NegativeInput_ThrowsArgumentOutOfRangeException(int negativeValue) =>
			// Act & Assert
			Assert.Throws<ArgumentOutOfRangeException>(
				() => IntHelpers.IntToChessNotationRank(negativeValue),
				$"Expected ArgumentOutOfRangeException for x = {negativeValue}"
			);

		[TestCase(26)]
		[TestCase(50)]
		[TestCase(100)]
		public void TooLargeInput_ThrowsArgumentOutOfRangeException(int largeValue) =>
			// Act & Assert
			Assert.Throws<ArgumentOutOfRangeException>(
				() => IntHelpers.IntToChessNotationRank(largeValue),
				$"Expected ArgumentOutOfRangeException for x = {largeValue}"
			);
		// Positive tests

		[TestCase(0,  "1")]
		[TestCase(7,  "8")]
		[TestCase(25, "26")]
		public void ValidInput_ReturnsCorrectNotation(int y, string expectedNotation)
		{
			// Act
			string actualNotation = IntHelpers.IntToChessNotationRank(y);

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

			for (var i = 0; i < 10; i++)
			{
				int y = random.Next(0, 26);

				var expectedNotation = $"{y + 1}";

				// Act
				string actualNotation = IntHelpers.IntToChessNotationRank(y);

				// Assert
				Assert.AreEqual(
					expectedNotation,
					actualNotation,
					$"For input {y}, expected {expectedNotation}, but got {actualNotation}"
				);
			}
		}
	}
}
