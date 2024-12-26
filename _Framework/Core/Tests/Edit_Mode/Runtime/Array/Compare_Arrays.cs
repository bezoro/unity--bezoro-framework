using NUnit.Framework;

namespace Bezoro.Framework.Core.Tests.Edit_Mode.Array_Helpers
{
	[TestFixture]
	[TestOf(typeof(Core.Array_Helpers))]
	public class Compare_Arrays
	{
		private class TestObject
		{
			public TestObject(int id, string name)
			{
				Id   = id;
				Name = name;
			}

			public int    Id   { get; }
			public string Name { get; }

			public override bool Equals(object obj)
			{
				return obj is TestObject other &&
					   Id == other.Id &&
					   Name == other.Name;
			}

			public override int GetHashCode()
			{
				return Id.GetHashCode() ^ Name.GetHashCode();
			}
		}

		#region Happy Path

		[TestCase(new object[] { 1, 2, 3 }, new object[] { 1, 2, 4 })]
		[TestCase(new object[] { "a", "b", "c" }, new object[] { "a", "b", "d" })]
		[TestCase(new object[0], new object[] { 0 })]
		public void Returns_False_For_Different_Arrays(object[] a, object[] b)
		{
			// Act
			var comparison_result = Core.Array_Helpers.Compare_Arrays(a, b);

			// Assert
			NUnit.Framework.Assert.AreEqual(
				comparison_result, false, "Comparing 2 arrays with different elements should be considered unequal."
			);
		}

		[TestCase(new object[] { 1, 2, 3 }, new object[] { 1, 2, 3 })]
		[TestCase(new object[] { "a", "b", "c" }, new object[] { "a", "b", "c" })]
		[TestCase(new object[0], new object[0])]
		public void Returns_True_For_Identical_Arrays(object[] a, object[] b)
		{
			// Act
			var result = Core.Array_Helpers.Compare_Arrays(a, b);

			// Assert
			NUnit.Framework.Assert.AreEqual(
				result, true, "Comparing 2 arrays with the same elements should be considered equal."
			);
		}

		[TestCase(new object[] { 1.1, 2.2, 3.3 }, new object[] { 1.1, 2.2, 3.3 })]
		[TestCase(new object[] { true, false }, new object[] { true, false })]
		public void Compares_Simple_Array_Types(object[] a, object[] b)
		{
			// Act
			var result = Core.Array_Helpers.Compare_Arrays(a, b);

			// Assert
			NUnit.Framework.Assert.AreEqual(
				result, true, "Comparing 2 arrays with the same elements should be considered equal."
			);
		}

		[Test]
		public void Compares_Custom_Array_Types()
		{
			// Arrange
			var obj1 = new TestObject(1, "Test1");
			var obj2 = new TestObject(2, "Test2");
			var obj3 = new TestObject(1, "Test1");

			var array1 = new[] { obj1, obj2 };
			var array2 = new[] { obj1, obj2 };
			var array3 = new[] { obj3, obj2 };

			// Act
			var result1 = Core.Array_Helpers.Compare_Arrays(array1, array2);
			var result2 = Core.Array_Helpers.Compare_Arrays(array1, array3);

			// Assert
			NUnit.Framework.Assert.IsTrue(
				result1, "Comparing 2 different arrays with the same elements should be considered equal."
			);

			NUnit.Framework.Assert.IsTrue(
				result2, "Comparing 2 different arrays with the same elements should be considered equal."
			);
		}

		[TestCase(new object[] { 1, 2, 3 }, null)]
		[TestCase(null, new object[] { 1, 2, 3 })]
		public void Returns_False_When_Comparing_With_Null(object[] a, object[] b)
		{
			// Act
			var result = Core.Array_Helpers.Compare_Arrays(a, b);

			// Assert
			NUnit.Framework.Assert.AreEqual(
				result, false, "Comparing a null awway with a valid array should return false."
			);
		}

		[Test]
		public void Returns_True_When_Comparing_Null_Arrays()
		{
			// Act
			var result = Core.Array_Helpers.Compare_Arrays<object>(null, null);

			// Assert
			NUnit.Framework.Assert.IsTrue(result, "Comparing 2 null arrays should be considered equal.");
		}

		[Test]
		public void Returns_False_For_Arrays_With_Different_Lengths()
		{
			// Arrange
			var array1 = new[] { 1, 2, 3 };
			var array2 = new[] { 1, 2, 3, 4 };

			// Act
			var result = Core.Array_Helpers.Compare_Arrays(array1, array2);

			// Assert
			NUnit.Framework.Assert.IsFalse(
				result, "Comparing 2 arrays with different lengths should be considered unequal."
			);
		}

		[Test]
		public void Returns_True_For_Identical_Arrays_With_Null_Elements()
		{
			// Arrange
			string[] array1 = { "a", null, "c" };
			string[] array2 = { "a", null, "c" };
			string[] array3 = { "a", null, "b" };

			// Act
			var result1 = Core.Array_Helpers.Compare_Arrays(array1, array2);
			var result2 = Core.Array_Helpers.Compare_Arrays(array1, array3);

			// Assert
			NUnit.Framework.Assert.IsTrue(result1);
			NUnit.Framework.Assert.IsFalse(result2);
		}

		[Test]
		public void Returns_True_For_Arrays_With_Only_Null_Elements()
		{
			// Arrange
			string[] array1 = { null, null, null };
			string[] array2 = { null, null, null };

			// Act
			var result = Core.Array_Helpers.Compare_Arrays(array1, array2);

			// Assert
			NUnit.Framework.Assert.IsTrue(
				result, "Comparing 2 arrays with only null elements should be considered equal."
			);
		}

		#endregion
	}
}
