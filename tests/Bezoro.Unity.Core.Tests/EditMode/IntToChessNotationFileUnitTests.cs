using System;
using Bezoro.Unity.Core.Common.Helpers;
using NUnit.Framework;

namespace Bezoro.Unity.Core.Tests.EditMode
{
	/// <summary>
	///     Verifies conversion from zero-based indices to chess notation file letters.
	/// </summary>
	[TestFixture]
	[TestOf(typeof(IntHelpers))]
	public class IntToChessNotationFileUnitTests
	{
		// Negative tests

		[Test]
		public void NegativeInput_ThrowsArgumentOutOfRangeException(
			[Values(
				-1,
				-50,
				-100,
				-500
			)]
			int negativeValue
		) =>
			// Act & Assert
			Assert.Throws<ArgumentOutOfRangeException>(
				() => IntHelpers.IntToChessNotationFile(
					negativeValue
				),
				$"Expected ArgumentOutOfRangeException for x = {negativeValue}"
			);

		[Test]
		public void TooLargeInput_ThrowsArgumentOutOfRangeException(
			[Values(
				26,
				50,
				100,
				500
			)]
			int largeValue
		) =>
			// Act & Assert
			Assert.Throws<ArgumentOutOfRangeException>(
				() => IntHelpers.IntToChessNotationFile(
					largeValue
				),
				$"Expected ArgumentOutOfRangeException for x = {largeValue}"
			);
		// Positive tests

		[TestCase(0,  "A")]
		[TestCase(7,  "H")]
		[TestCase(25, "Z")]
		public void ValidInput_ReturnsCorrectNotation(int input, string expected)
		{
			// Act
			string actual = IntHelpers.IntToChessNotationFile(input);

			// Assert
			Assert.AreEqual(
				expected,
				actual,
				$"For input {input}, expected {expected}, but got {actual}"
			);
		}

		[Test]
		public void ValidInput_ReturnsCorrectNotation_RandomSampling()
		{
			var random = new Random(42); // Seed for reproducibility

			for (var i = 0; i < 100; i++) // Test 10 random cases
			{
				int input = random.Next(0, 26);

				var expected = ((char)('A' + input)).ToString();

				// Act
				string actual = IntHelpers.IntToChessNotationFile(input);

				// Assert
				Assert.AreEqual(
					expected,
					actual,
					$"For input {input}, expected {expected}, but got {actual}"
				);
			}
		}
	}
}
