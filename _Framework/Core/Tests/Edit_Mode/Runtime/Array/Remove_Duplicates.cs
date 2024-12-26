using System;
using NUnit.Framework;

namespace Bezoro.Framework.Core.Tests.Edit_Mode.Array_Helpers
{
	[TestFixture]
	[TestOf(typeof(Core.Array_Helpers))]
	public class Remove_Duplicates
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
				if (obj is TestObject other)
					return Id == other.Id && Name == other.Name;

				return false;
			}

			public override int GetHashCode()
			{
				return HashCode.Combine(Id, Name);
			}
		}

		#region Happy Path

		[TestCase(new object[] { 1, 1, 1, 1 }, new object[] { 1 })]
		[TestCase(new object[] { 1, 2, 3, 3, 4, 5 }, new object[] { 1, 2, 3, 4, 5 })]
		[TestCase(new object[] { "A", "A", "B", "B", "C" }, new object[] { "A", "B", "C" })]
		public void Removes_Duplicates_From_Existing_Array(object[] array, object[] expectedArray)
		{
			// Act
			Core.Array_Helpers.Remove_Duplicates(ref array);

			// Assert
			CollectionAssert.AreEqual(expectedArray, array);
		}

		[TestCase(new object[] { 1, 2, 2, null, 1, 3, null }, new object[] { 1, 2, 3 })]
		[TestCase(new object[] { null, null, null }, new object[] { })]
		[TestCase(new object[] { "X", "Y", "X", null, "Y", null }, new object[] { "X", "Y" })]
		public void Removes_Duplicate_Nulls(object[] array, object[] expected)
		{
			// Act
			Core.Array_Helpers.Remove_Duplicates(ref array);

			// Assert
			CollectionAssert.AreEqual(expected, array);
		}

		[TestCase(new object[] { 1, 1, 2, 2, 3, 3 }, new object[] { 1, 2, 3 })]
		[TestCase(new object[] { 1.1, 1.1, 2.2, 3.3, 3.3 }, new object[] { 1.1, 2.2, 3.3 })]
		[TestCase(new object[] { 'a', 'a', 'b', 'c', 'c' }, new object[] { 'a', 'b', 'c' })]
		public void Removes_Duplicates_From_Simple_Types(object[] array, object[] expectedArray)
		{
			// Act
			Core.Array_Helpers.Remove_Duplicates(ref array);

			// Assert
			CollectionAssert.AreEqual(expectedArray, array);
		}

		[Test]
		public void Removes_Duplicates_From_Custom_Types()
		{
			// Arrange
			var array = new[]
			{
				new TestObject(1, "Object1"),
				new TestObject(2, "Object2"),
				new TestObject(1, "Object1"), // Duplicate
				new TestObject(3, "Object3"),
				new TestObject(2, "Object2") // Duplicate
			};

			var expected_array = new[]
			{
				new TestObject(1, "Object1"),
				new TestObject(2, "Object2"),
				new TestObject(3, "Object3")
			};

			// Act
			Core.Array_Helpers.Remove_Duplicates(ref array);

			// Assert
			CollectionAssert.AreEqual(expected_array, array);
		}

		[Test]
		public void Does_Nothing_When_Array_Is_Null()
		{
			// Arrange
			int[] array = null;

			// Act
			Core.Array_Helpers.Remove_Duplicates(ref array);

			// Assert
			NUnit.Framework.Assert.IsNull(array);
		}

		[Test]
		public void Does_Nothing_When_Array_Has_Single_Element()
		{
			// Arrange
			var array = new[] { 42 };

			// Act
			Core.Array_Helpers.Remove_Duplicates(ref array);

			// Assert
			CollectionAssert.AreEqual(new[] { 42 }, array);
		}

		[Test]
		public void Does_Nothing_When_Array_Is_Empty()
		{
			// Arrange
			var array = System.Array.Empty<int>();

			// Act
			Core.Array_Helpers.Remove_Duplicates(ref array);

			// Assert
			CollectionAssert.IsEmpty(array);
		}

		#endregion
	}
}
