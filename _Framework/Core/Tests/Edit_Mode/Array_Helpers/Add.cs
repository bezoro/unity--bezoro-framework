using System.Linq;
using NUnit.Framework;

namespace Bezoro.Framework.Core.Tests.EditMode.Array_Helpers
{
	[TestFixture]
	[TestOf(typeof(Core.Array_Helpers))]
	public class Add
	{
		private class Test_Object { }

	#region Happy Path

		[Test]
		public void Adds_Element_To_Null_Array()
		{
			// Arrange
			Test_Object[] array          = null;
			var           element_to_add = new Test_Object();

			// Act
			Core.Array_Helpers.Add(ref array, element_to_add);

			// Assert
			NUnit.Framework.Assert.That(array, Is.Not.Null, "Array should be initialized.");

			NUnit.Framework.Assert.That(
				array.Length, Is.GreaterThanOrEqualTo(1), "Array should have at least 1 length."
			);

			NUnit.Framework.Assert.That(
				array[0], Is.EqualTo(element_to_add), "Element should be added to the array."
			);
		}

		[Test]
		public void Adds_Element_To_Empty_Array()
		{
			// Arrange
			var array          = new Test_Object[] { };
			var element_to_add = new Test_Object();
			var initial_length = array.Length;

			// Act
			Core.Array_Helpers.Add(ref array, element_to_add);

			// Assert
			NUnit.Framework.Assert.That(
				array, Is.Not.Null, "Array should not be null."
			);

			NUnit.Framework.Assert.That(
				array.Length, Is.GreaterThan(0), "Array should not be empty."
			);

			NUnit.Framework.Assert.That(
				array.Contains(element_to_add), "Element should be present in the array."
			);

			NUnit.Framework.Assert.That(
				array.Length, Is.GreaterThan(initial_length), "Array length should increase."
			);
		}

		[Test]
		public void Adds_Element_To_Existing_Array()
		{
			// Arrange
			var array          = new[] { new Test_Object(), new Test_Object() };
			var element_to_add = new Test_Object();
			var initial_length = array.Length;

			// Act
			Core.Array_Helpers.Add(ref array, element_to_add);

			// Assert
			NUnit.Framework.Assert.That(
				array, Is.Not.Null, "Array should not be null."
			);

			NUnit.Framework.Assert.That(
				array.Length, Is.GreaterThan(0), "Array should not be empty."
			);

			NUnit.Framework.Assert.That(
				array.Contains(element_to_add), Is.True, "Element should be present in the array."
			);

			NUnit.Framework.Assert.That(
				array.Length, Is.GreaterThan(initial_length), "Array length should increase."
			);
		}

		[Test]
		public void Returns_Correct_Index()
		{
			// Arrange
			var array          = new[] { new Test_Object(), new Test_Object(), new Test_Object() };
			var element_to_add = new Test_Object();
			var initial_length = array.Length;

			// Act
			Core.Array_Helpers.Add(ref array, element_to_add, out var index);

			// Assert
			NUnit.Framework.Assert.That(
				array.Contains(element_to_add), "Element should be present in the array."
			);

			NUnit.Framework.Assert.That(
				index, Is.EqualTo(initial_length), "Index should match the position of the newly added element."
			);
		}

		[Test]
		public void Does_Nothing_When_Element_Is_Null()
		{
			// Arrange
			var array          = new[] { new Test_Object(), new Test_Object() };
			var initial_length = array.Length;

			// Act
			Core.Array_Helpers.Add(ref array, null);

			// Assert
			NUnit.Framework.Assert.That(
				array.Length, Is.EqualTo(initial_length), "Array length should remain unchanged."
			);

			NUnit.Framework.Assert.That(
				array.Contains(null), Is.False, "Null element should not be added to the array."
			);
		}

		[Test]
		public void Adds_Element_At_First_Empty_Slot()
		{
			// Arrange
			var array          = new[] { new Test_Object(), null, new Test_Object() };
			var element_to_add = new Test_Object();

			// Act
			Core.Array_Helpers.Add(ref array, element_to_add);

			// Assert
			NUnit.Framework.Assert.That(
				array.Contains(element_to_add), Is.True, "Element should be added to the first empty slot."
			);

			NUnit.Framework.Assert.That(
				array.Count(obj => obj == null), Is.EqualTo(0), "There should be no empty slots left."
			);
		}

	#endregion
	}
}
