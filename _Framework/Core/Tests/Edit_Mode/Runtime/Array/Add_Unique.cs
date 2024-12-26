using NUnit.Framework;

namespace Bezoro.Framework.Core.Tests.Edit_Mode.Array_Helpers
{
	[TestFixture]
	[TestOf(typeof(Core.Array_Helpers))]
	public class Add_Unique
	{
		#region Happy Path

		[Test]
		public void Does_Not_Add_Duplicate_Element()
		{
			// Arrange
			string[] array   = { "Element1" };
			var      element = "Element1";
			int      index;

			// Act
			Core.Array_Helpers.Add_Unique(ref array, element, out index);

			// Assert
			NUnit.Framework.Assert.AreEqual(
				1, array.Length, "The array length should remain unchanged for a duplicate element."
			);

			NUnit.Framework.Assert.AreEqual(-1, index, "The index should indicate that no element was added.");
		}

		[Test]
		public void Adds_Element_To_Null_Array()
		{
			// Arrange
			string[]     array   = null;
			const string element = "TestElement";
			int          index;

			// Act
			Core.Array_Helpers.Add_Unique(ref array, element, out index);

			// Assert
			NUnit.Framework.Assert.IsNotNull(array, "Array should be initialized.");
			NUnit.Framework.Assert.AreEqual(1, array.Length, "Array length should be 1 after adding the element.");
			NUnit.Framework.Assert.AreEqual(element, array[0], "The added element should match the provided value.");
			NUnit.Framework.Assert.AreEqual(0, index, "The index of the added element should be 0.");
		}

		[Test]
		public void Adds_Element_To_Empty_Array()
		{
			// Arrange
			string[]     array   = null;
			const string element = "Element1";
			int          index;

			// Act
			Core.Array_Helpers.Add_Unique(ref array, element, out index);

			// Assert
			NUnit.Framework.Assert.IsNotNull(array, "Array should be initialized.");
			NUnit.Framework.Assert.AreEqual(1, array.Length, "Array length should be 1 after adding the element.");
			NUnit.Framework.Assert.AreEqual(element, array[0], "The added element should match the provided value.");
			NUnit.Framework.Assert.AreEqual(0, index, "The index of the added element should be correct.");
		}

		[Test]
		public void Adds_Element_To_Existing_Array()
		{
			// Arrange
			string[] array   = { "Element1" };
			var      element = "Element2";
			int      index;

			// Act
			Core.Array_Helpers.Add_Unique(ref array, element, out index);

			// Assert
			NUnit.Framework.Assert.IsNotNull(array, "Array should not be null after adding an element.");
			NUnit.Framework.Assert.AreEqual(2, array.Length, "Array length should be updated correctly.");
			NUnit.Framework.Assert.AreEqual(element, array[1], "New element should be added at the correct position.");
			NUnit.Framework.Assert.AreEqual(1, index, "Index of added element should be accurate.");
		}

		[Test]
		public void Resizes_Array_When_Full()
		{
			// Arrange
			string[] array   = { "Element1", "Element2" };
			var      element = "Element3";
			int      index;

			// Act
			Core.Array_Helpers.Add_Unique(ref array, element, out index);

			// Assert
			NUnit.Framework.Assert.AreEqual(4, array.Length, "Array length did not resize correctly.");
			NUnit.Framework.Assert.AreEqual(element, array[2], "Element not added at the correct position.");
			NUnit.Framework.Assert.AreEqual(2, index, "Index of inserted element is incorrect.");
		}

		[Test]
		public void Adds_Unique_Element_Only()
		{
			// Arrange
			string[]     array          = { "Element1", "Element2" };
			var          initial_length = array.Length;
			const string element        = "Element3";

			// Act
			Core.Array_Helpers.Add_Unique(ref array, element);

			// Assert
			NUnit.Framework.Assert.IsNotNull(array, "Array should not be null after adding an element.");

			NUnit.Framework.Assert.That(
				array.Length, Is.GreaterThan(initial_length),
				"The array length should increase after adding a new element."
			);

			NUnit.Framework.Assert.That(
				array[initial_length], Is.EqualTo(element),
				"The added unique element should be at the correct position."
			);

			// Attempt to add a duplicate element
			var new_length = array.Length;
			Core.Array_Helpers.Add_Unique(ref array, element);

			// Assert that duplicate is not added
			NUnit.Framework.Assert.That(
				array.Length, Is.EqualTo(new_length), "Array length should remain unchanged for a duplicate element."
			);
		}

		[Test]
		public void Resizes_When_Array_Is_Full()
		{
			// Arrange
			var          array          = new[] { "Element1", "Element2" };
			var          initial_length = array.Length;
			const string element_to_add = "Element3";

			// Act
			Core.Array_Helpers.Add_Unique(ref array, element_to_add, out var index);

			// Assert
			NUnit.Framework.Assert.NotNull(array, "Array should not be null after resizing.");

			NUnit.Framework.Assert.That(
				array.Length, Is.GreaterThan(initial_length),
				"Array length should increase after adding an element to a full array."
			);

			NUnit.Framework.Assert.AreEqual(
				element_to_add, array[initial_length], "New element should be added at the correct position."
			);

			NUnit.Framework.Assert.AreEqual(initial_length, index, "Index of the added element is not correct.");
		}

		[Test]
		public void Does_Nothing_When_Element_Is_Null()
		{
			// Arrange
			string[] array = { "Element1" };

			// Act
			Core.Array_Helpers.Add_Unique(ref array, null, out var index);

			// Assert
			NUnit.Framework.Assert.AreEqual(
				1, array.Length, "The array length should remain unchanged when the element to add is null."
			);

			NUnit.Framework.Assert.AreEqual(-1, index, "The index should indicate that no element was added.");
		}

		[Test]
		public void Adds_Element_Using_Parallel_Method_When_Array_Length_Is_Larger_Than_Threshold()
		{
			// Arrange
			var          array          = new string[Core.Array_Helpers.Parallel_Threshold * 2];
			var          initial_length = array.Length;
			const string element_to_add = "Element999";

			for (var i = 0 ; i < array.Length ; i++)
				array[i] = $"Element{i + 1}";

			// Act
			Core.Array_Helpers.Add_Unique(ref array, element_to_add, out var index);

			// Assert
			NUnit.Framework.Assert.IsNotNull(array, "Array should not be null after adding the element.");

			NUnit.Framework.Assert.That(
				array, Does.Contain(element_to_add), "The new element should be correctly added to the array."
			);

			NUnit.Framework.Assert.AreEqual(initial_length, index, "Index of the added element should be accurate.");

			NUnit.Framework.Assert.That(
				array.Length, Is.GreaterThan(initial_length),
				"Array length should increase after adding a unique element."
			);
		}

		[Test]
		public void Adds_Element_Using_Sequential_Method_When_Array_Length_Is_Smaller_Than_Threshold()
		{
			// Arrange
			string[]     array   = { "Element1" };
			const string element = "Element2";

			// Act
			Core.Array_Helpers.Add_Unique(ref array, element, out var index);

			// Assert
			NUnit.Framework.Assert.IsNotNull(array, "Array should not be null after adding the element.");

			NUnit.Framework.Assert.AreEqual(
				2, array.Length, "Array length should increase after adding a unique element."
			);

			NUnit.Framework.Assert.AreEqual(
				element, array[1], "The new element should be correctly added to the array."
			);

			NUnit.Framework.Assert.AreEqual(1, index, "Index of the added element should be accurate.");
		}
	}

	#endregion
}
