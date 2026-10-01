using System;
using Bezoro.Unity.Core.Common.Helpers;
using NUnit.Framework;

namespace Bezoro.Unity.Core.Tests.EditMode
{
	/// <summary>
	///     Verifies conversion from zero-based file and rank indices to chess notation.
	/// </summary>
	[TestFixture]
	public class IntToChessNotationUnitTests
	{
		[Test]
		public void EdgeCases_ReturnsCorrectNotation()
		{
			// Arrange & Act & Assert
			Assert.AreEqual(
				"A1",
				IntHelpers.IntToChessNotation(0, 0),
				"Lower bound should be A1"
			);

			Assert.AreEqual(
				"Z26",
				IntHelpers.IntToChessNotation(25, 25),
				"Upper bound should be Z26"
			);
		}

		[Test]
		public void NegativeInputs_ThrowsArgumentOutOfRangeException([Values(-1, -5, -10, -50)] int negativeValue)
		{
			// Act & Assert
			Assert.Throws<ArgumentOutOfRangeException>(
				() => IntHelpers.IntToChessNotation(negativeValue, 0),
				$"Expected ArgumentOutOfRangeException for x = {negativeValue}"
			);

			Assert.Throws<ArgumentOutOfRangeException>(
				() => IntHelpers.IntToChessNotation(0, negativeValue),
				$"Expected ArgumentOutOfRangeException for y = {negativeValue}"
			);
		}

		[Test]
		public void TooLargeInputs_ThrowsArgumentOutOfRangeException([Values(26, 50, 100)] int largeValue)
		{
			// Act & Assert
			Assert.Throws<ArgumentOutOfRangeException>(
				() => IntHelpers.IntToChessNotation(largeValue, 0),
				$"Expected ArgumentOutOfRangeException for x = {largeValue}"
			);

			Assert.Throws<ArgumentOutOfRangeException>(
				() => IntHelpers.IntToChessNotation(0, largeValue),
				$"Expected ArgumentOutOfRangeException for y = {largeValue}"
			);
		}

		[TestCase(0,  0,  "A1")]
		[TestCase(3,  5,  "D6")]
		[TestCase(7,  7,  "H8")]
		[TestCase(12, 0,  "M1")]
		[TestCase(0,  15, "A16")]
		[TestCase(25, 25, "Z26")]
		public void ValidInputs_ReturnsCorrectNotation(int x, int y, string expectedNotation)
		{
			// Act
			string actualNotation = IntHelpers.IntToChessNotation(x, y);

			// Assert
			Assert.AreEqual(
				expectedNotation,
				actualNotation,
				$"For input ({x}, {y}), expected {expectedNotation}, but got {actualNotation}"
			);
		}
	}
}
