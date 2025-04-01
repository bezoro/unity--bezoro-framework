using System;
using NUnit.Framework;

namespace Bezoro.Framework.Core.Tests.Edit_Mode.Array
{
	[TestFixture]
	public class Merge
	{
		// Test class for complex type checking
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
				{
					return Id == other.Id && Name == other.Name;
				}

				return false;
			}

			public override int GetHashCode()
			{
				return HashCode.Combine(Id, Name);
			}
		}

		[Test]
		public void Merges_When_To_Is_Null()
		{
			// Arrange
			var      from = new[] { "A", "B", "C" };
			string[] to   = null;

			// Act
			ArrayHelpers.Merge(from, ref to);

			// Assert
			CollectionAssert.AreEqual(from, to);
		}

		[Test]
		public void Merges_When_To_Is_Empty()
		{
			// Arrange
			var from = new[] { "A", "B", "C" };
			var to   = System.Array.Empty<string>();

			// Act
			ArrayHelpers.Merge(from, ref to);

			// Assert
			CollectionAssert.AreEqual(from, to);
		}

		[Test]
		public void Copies_Elements_When_To_Is_Smaller()
		{
			// Arrange
			var from = new[] { 1, 2, 3 };
			var to   = new int[2]; // Smaller array than 'from'

			// Act
			ArrayHelpers.Merge(from, ref to);

			// Assert
			CollectionAssert.AreEqual(new[] { 1, 2, 3 }, to);
		}

		[Test]
		public void Does_Not_Copy_If_To_Already_Has_Data()
		{
			// Arrange
			var from = new[] { "X", "Y", "Z" };
			var to   = new[] { "A", "B", null };

			// Act
			ArrayHelpers.Merge(from, ref to);

			// Assert
			CollectionAssert.AreEqual(new[] { "A", "B", "Z" }, to);
		}

		[Test]
		public void Merges_Custom_Object_Array()
		{
			// Arrange
			var from = new[]
			{
				new TestObject(1, "Object1"),
				new TestObject(2, "Object2"),
				new TestObject(3, "Object3")
			};

			var to = new TestObject[2]; // Smaller array than 'from'

			// Act
			ArrayHelpers.Merge(from, ref to);

			// Assert
			CollectionAssert.AreEqual(from, to);
		}

		[Test]
		public void Does_Nothing_When_From_Array_Is_Null()
		{
			// Arrange
			string[] to = { "Existing" };

			// Act
			ArrayHelpers.Merge(null, ref to);

			// Assert
			CollectionAssert.AreEqual(new[] { "Existing" }, to);
		}

		[Test]
		public void Does_Nothing_When_From_Array_Is_Empty()
		{
			// Arrange
			var from = System.Array.Empty<string>();
			var to   = new[] { "Existing" };

			// Act
			ArrayHelpers.Merge(from, ref to);

			// Assert
			CollectionAssert.AreEqual(new[] { "Existing" }, to);
		}

		[Test]
		public void Handles_Both_Arrays_Being_Null()
		{
			// Arrange
			string[] to = null;

			// Act
			ArrayHelpers.Merge(null, ref to);

			// Assert
			NUnit.Framework.Assert.IsNull(to);
		}

		[Test]
		public void Does_Nothing_When_To_Is_Larger_And_Contains_Elements()
		{
			// Arrange
			var from = new[] { 1, 2 };
			var to   = new[] { 3, 4, 5 };

			// Act
			ArrayHelpers.Merge(from, ref to);

			// Assert
			CollectionAssert.AreEqual(new[] { 3, 4, 5 }, to);
		}

		[Test]
		public void Partial_Copy_When_To_Contains_Partial_Nulls()
		{
			// Arrange
			var from = new[] { "A", "B", "C" };
			var to   = new[] { "X", null, "Z" };

			// Act
			ArrayHelpers.Merge(from, ref to);

			// Assert
			CollectionAssert.AreEqual(new[] { "X", "B", "Z" }, to);
		}

		[Test]
		public void Expands_To_If_Required()
		{
			// Arrange
			var from = new[] { 100, 200, 300 };
			var to   = new int[1]; // Smaller array than 'from'

			// Act
			ArrayHelpers.Merge(from, ref to);

			// Assert
			CollectionAssert.AreEqual(from, to);
		}
	}
}
