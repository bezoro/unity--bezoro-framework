using System.Linq;
using NUnit.Framework;

namespace Bezoro.Framework.Core.Tests.Edit_Mode.Array_Helpers
{
	[TestFixture]
	[TestOf(typeof(Core.Array_Helpers))]
	public class Remove_Element
	{
		[Test]
		public void Removes_Element_Sequentially_When_Size_Is_Smaller_Than_Threshold()
		{
			// Arrange
			var       array             = new[] { 1, 2, 3, 4, 5 };
			const int element_to_remove = 3;

			// Act
			Core.Array_Helpers.Remove_Element(ref array, element_to_remove);

			// Assert
			NUnit.Framework.Assert.False(array.Contains(element_to_remove), "The element should be removed.");
		}

		[Test]
		public void Removes_Element_In_Parallel_When_Size_Is_Larger_Than_Threshold()
		{
			// Arrange
			var parallel_threshold = Core.Array_Helpers.Parallel_Threshold;
			var array              = Enumerable.Range(1, parallel_threshold * 2).ToArray();
			var element_to_remove  = parallel_threshold;

			// Act
			Core.Array_Helpers.Remove_Element(ref array, element_to_remove);

			// Assert
			NUnit.Framework.Assert.False(array.Contains(element_to_remove), "The element should be removed.");
		}

		[Test]
		public void Removes_Element_By_Resizing_Array_When_Chosen()
		{
			// Arrange
			var       array             = new[] { 1, 2, 3, 4, 5 };
			const int element_to_remove = 3;

			// Act
			Core.Array_Helpers.Remove_Element(ref array, element_to_remove);

			// Assert
			NUnit.Framework.Assert.AreEqual(
				new[] { 1, 2, 4, 5 },
				array,
				"The array should be resized and updated after element removal."
			);

			NUnit.Framework.Assert.AreEqual(
				4,
				array.Length,
				"The array length should reflect the resized content."
			);
		}

		[Test]
		public void Removes_Element_By_Nullifying_Element_When_Chosen()
		{
			// Arrange
			var       array             = new int?[] { 1, 2, 3, 4, 5 };
			var       initial_length    = array.Length;
			const int element_to_remove = 3;

			// Act
			Core.Array_Helpers.Remove_Element(ref array, element_to_remove, Array_Removal_Approach.Set_To_Null);

			// Assert
			NUnit.Framework.Assert.AreEqual(
				new int?[] { 1, 2, null, 4, 5 },
				array,
				"The element should be nullified in the array."
			);

			NUnit.Framework.Assert.AreEqual(
				initial_length,
				array.Length,
				"The length of the array should not change when nullifying an element."
			);
		}

		[Test]
		public void Removes_First_Element_Found_When_There_Are_Duplicates()
		{
			// Arrange
			var       array             = new[] { 1, 2, 3, 3, 4, 5 };
			const int element_to_remove = 3;

			// Act
			Core.Array_Helpers.Remove_Element(ref array, element_to_remove);

			// Assert

			NUnit.Framework.Assert.AreEqual(
				new[] { 1, 2, 3, 4, 5 },
				array,
				"The array content should match the expected sequence."
			);

			NUnit.Framework.Assert.AreEqual(
				5,
				array.Length,
				"The array length should be reduced by one."
			);
		}

		[Test]
		public void Does_Nothing_When_Array_Is_Null()
		{
			// Arrange
			int[]     array             = null;
			const int element_to_remove = 3;

			// Act
			Core.Array_Helpers.Remove_Element(ref array, element_to_remove);

			// Assert
			NUnit.Framework.Assert.IsNull(array, "The array should remain null.");
		}

		[Test]
		public void Does_Nothing_When_Array_Is_Empty()
		{
			// Arrange
			var       array             = new int[0];
			const int element_to_remove = 3;

			// Act
			Core.Array_Helpers.Remove_Element(ref array, element_to_remove);

			// Assert
			NUnit.Framework.Assert.IsEmpty(array, "The array should remain empty.");
		}

		[Test]
		public void Does_Nothing_When_Element_Not_Found()
		{
			// Arrange
			var       array                = new[] { 1, 2, 3, 4, 5 };
			const int element_not_in_array = 6;
			var       initial_length       = array.Length;

			// Act
			Core.Array_Helpers.Remove_Element(ref array, element_not_in_array);

			// Assert
			NUnit.Framework.Assert.AreEqual(
				new[] { 1, 2, 3, 4, 5 },
				array,
				"The array content should remain unchanged when the element is not found."
			);

			NUnit.Framework.Assert.AreEqual(
				initial_length,
				array.Length,
				"The array length should remain unchanged when the element is not found."
			);
		}

		[Test]
		public void Does_Nothing_When_Element_Is_Null()
		{
			// Arrange
			var array          = new int?[] { 1, 2, 3, 4, 5 };
			var initial_length = array.Length;

			// Act
			Core.Array_Helpers.Remove_Element(ref array, null);

			// Assert
			NUnit.Framework.Assert.AreEqual(
				new int?[] { 1, 2, 3, 4, 5 },
				array,
				"The array content should remain unchanged when the element is null."
			);

			NUnit.Framework.Assert.AreEqual(
				initial_length,
				array.Length,
				"The array length should remain unchanged when the element is null."
			);
		}
	}
}
