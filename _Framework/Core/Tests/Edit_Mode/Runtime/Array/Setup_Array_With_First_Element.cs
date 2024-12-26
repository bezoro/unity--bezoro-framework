using NUnit.Framework;

namespace Bezoro.Framework.Core.Tests.Edit_Mode.Array_Helpers
{
	[TestFixture]
	[TestOf(typeof(Core.Array_Helpers))]
	public class Setup_Array_With_First_Element
	{
		#region Happy Path

		[Test]
		public void Creates_Array_When_Null()
		{
			// Arrange
			int[] array   = null;
			var   element = 42;

			// Act
			var result = Core.Array_Helpers.Setup_Array_With_First_Element(ref array, element);

			// Assert
			NUnit.Framework.Assert.IsTrue(result, "Expected the method to return true when initializing the array.");

			NUnit.Framework.Assert.NotNull(array, "Expected the array to be initialized.");

			NUnit.Framework.Assert.AreEqual(
				1, array.Length, "Array length should be exactly 1 after initialization."
			);

			NUnit.Framework.Assert.AreEqual(
				element, array[0], "First element of the array should match the provided element."
			);
		}

		[Test]
		public void Creates_Array_When_Empty()
		{
			// Arrange
			var array   = new int[0];
			var element = 42;

			// Act
			var result = Core.Array_Helpers.Setup_Array_With_First_Element(ref array, element);

			// Assert
			NUnit.Framework.Assert.IsTrue(
				result, "Expected the method to return true when reinitializing the array."
			);

			NUnit.Framework.Assert.NotNull(array, "Expected the array to be initialized.");

			NUnit.Framework.Assert.AreEqual(
				1, array.Length, "Array length should be exactly 1 after initialization."
			);

			NUnit.Framework.Assert.AreEqual(
				element, array[0], "First element of the array should match the provided element."
			);
		}

		[Test]
		public void Returns_False_When_Array_Is_Not_Empty()
		{
			// Arrange
			int[] array   = { 10, 20 };
			var   element = 42;

			// Act
			var result = Core.Array_Helpers.Setup_Array_With_First_Element(ref array, element);

			// Assert
			NUnit.Framework.Assert.IsFalse(
				result, "Expected the method to return false when the array is already initialized."
			);
		}

		[Test]
		public void Does_Not_Modify_Array_When_Already_Initialized()
		{
			// Arrange
			int[] array   = { 10, 20, 30 };
			var   element = 42;

			// Act
			var result = Core.Array_Helpers.Setup_Array_With_First_Element(ref array, element);

			// Assert
			NUnit.Framework.Assert.IsFalse(
				result, "Expected the method to return false when the array is not empty."
			);

			NUnit.Framework.Assert.AreEqual(3, array.Length, "Array length should remain unchanged.");

			NUnit.Framework.Assert.AreEqual(10, array[0], "First element should remain unchanged.");

			NUnit.Framework.Assert.AreEqual(20, array[1], "Second element should remain unchanged.");

			NUnit.Framework.Assert.AreEqual(30, array[2], "Third element should remain unchanged.");
		}

		#endregion
	}
}
