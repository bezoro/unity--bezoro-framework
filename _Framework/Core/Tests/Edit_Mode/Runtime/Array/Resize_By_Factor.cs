using Moq;
using NUnit.Framework;

namespace Bezoro.Framework.Core.Tests.Edit_Mode.Array_Helpers
{
	[TestFixture]
	[TestOf(typeof(Core.Array_Helpers))]
	public class Resize_By_Factor
	{
		private Mock<I_Logger> _mockLogger;

		[SetUp]
		public void SetUp()
		{
			_mockLogger = new();
		}

		#region Happy Path

		[Test]
		public void Increases_Array_Size_By_Factor__When_Factor_Is_Positive()
		{
			// Arrange
			int[] array  = { 1, 2, 3 };
			var   factor = 2;

			// Act
			Core.Array_Helpers.Resize_By_Factor(ref array, factor, _mockLogger.Object);

			// Assert
			NUnit.Framework.Assert.AreEqual(6, array.Length, "Array size should be multiplied by the factor.");

			_mockLogger.Verify(
				logger => logger.Log(It.Is<string>(msg => !string.IsNullOrEmpty(msg))),
				Times.Once
			);
		}

		[Test]
		public void Preserves_Array_Size__When_Array_Is_Empty()
		{
			// Arrange
			var       array  = System.Array.Empty<int>();
			const int factor = 3;

			// Act
			Core.Array_Helpers.Resize_By_Factor(ref array, factor, _mockLogger.Object);

			// Assert
			NUnit.Framework.Assert.AreEqual(0, array.Length, "Empty array multiplied should still stay empty.");

			_mockLogger.Verify(
				logger => logger.Log(It.Is<string>(msg => !string.IsNullOrEmpty(msg))),
				Times.Once
			);
		}

		[Test]
		public void Preserves_Array_Size__When_Factor_Is_One_Or_Less([Range(1, -5)] int factor)
		{
			// Arrange
			int[] array = { 1, 2, 3 };

			// Act
			Core.Array_Helpers.Resize_By_Factor(ref array, factor, _mockLogger.Object);

			// Assert
			NUnit.Framework.Assert.AreEqual(3, array.Length, "Array size should remain unchanged.");

			_mockLogger.Verify(
				logger => logger.Log_Warning(It.Is<string>(msg => !string.IsNullOrEmpty(msg))),
				Times.Once
			);
		}

		#endregion
	}
}
