using NUnit.Framework;

namespace Bezoro.Framework.Core.Tests.Edit_Mode.Array_Helpers
{
	[TestFixture]
	[TestOf(typeof(ArrayHelpers))]
	public class Initialize_Array_If_Null
	{
		#region Happy Path

		[TestCase(null, new int[0])]
		[TestCase(new[] { 1, 2, 3 }, new[] { 1, 2, 3 })]
		public void Initializes_Null_Array(
			int[] array,
			int[] expected
		)
		{
			// Act
			ArrayHelpers.Initialize_Null_Array(ref array);

			// Assert
			NUnit.Framework.Assert.That(array, Is.EqualTo(expected));
		}

		[Test]
		public void Does_Nothing_If_Array_Is_Not_Null()
		{
			// Arrange
			int[] array    = { 1, 2, 3 };
			int[] expected = { 1, 2, 3 };

			// Act
			ArrayHelpers.Initialize_Null_Array(ref array);

			// Assert
			NUnit.Framework.Assert.That(array, Is.EqualTo(expected));
		}

		#endregion
	}
}
