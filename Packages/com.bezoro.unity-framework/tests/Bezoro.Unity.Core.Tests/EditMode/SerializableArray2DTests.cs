using System;
using Bezoro.Unity.Core.Collections;
using Bezoro.Unity.Core.Common.Extensions;
using NUnit.Framework;

namespace Bezoro.Unity.Core.Tests.EditMode
{
	/// <summary>
	///     Verifies SerializableArray2D indexing, copying, direction lookup, and extension behavior.
	/// </summary>
	[TestFixture]
	public sealed class SerializableArray2DTests
	{
		[Test]
		public void ClearAndFill_UpdateAllElements()
		{
			var array = new SerializableArray2D<int>(new[] { 1, 2, 3, 4 }, 2, 2);

			array.Clear();

			CollectionAssert.AreEqual(new[] { 0, 0, 0, 0 }, array.GetDataCopy());

			array.Fill(7);

			CollectionAssert.AreEqual(new[] { 7, 7, 7, 7 }, array.GetDataCopy());
		}

		[Test]
		public void Constructor_FromOneDimensionalArray_LoadsElementsInRowMajorOrder()
		{
			var array = new SerializableArray2D<int>(
				new[] { 1, 2, 3, 4 },
				2,
				2
			);

			Assert.AreEqual(1, array[0, 0]);
			Assert.AreEqual(2, array[1, 0]);
			Assert.AreEqual(3, array[0, 1]);
			Assert.AreEqual(4, array[1, 1]);
		}

		[Test]
		public void Constructor_FromTwoDimensionalArray_UsesRowsThenColumns()
		{
			var array = new SerializableArray2D<int>(
				new[,]
				{
					{ 1, 2 },
					{ 3, 4 }
				}
			);

			Assert.AreEqual(2, array.Width);
			Assert.AreEqual(2, array.Height);
			Assert.AreEqual(4, array[1, 1]);
		}

		[Test]
		public void GetDataCopy_ReturnsCopyOfInternalData()
		{
			var array = new SerializableArray2D<int>(new[] { 1, 2, 3, 4 }, 2, 2);

			int[] copy = array.GetDataCopy();
			copy[0] = 99;

			Assert.AreEqual(1, array[0, 0]);
		}

		[Test]
		public void GetRowAndGetColumn_ReturnExpectedSlices()
		{
			var array = new SerializableArray2D<int>(new[] { 1, 2, 3, 4, 5, 6 }, 3, 2);

			CollectionAssert.AreEqual(new[] { 4, 5, 6 }, array.GetRow(1));
			CollectionAssert.AreEqual(new[] { 2, 5 },    array.GetColumn(1));
		}

		[Test]
		public void ImplicitConversion_ReturnsCopyAndEmptyArrayForNull()
		{
			SerializableArray2D<int> nullArray = null;
			var                      array     = new SerializableArray2D<int>(new[] { 1, 2, 3, 4 }, 2, 2);

			int[] empty = nullArray;
			int[] copy  = array;
			copy[0] = 99;

			CollectionAssert.IsEmpty(empty);
			CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, array.GetDataCopy());
		}

		[Test]
		public void Indexer_WhenIndicesAreOutOfRange_ThrowsIndexOutOfRangeException()
		{
			var array = new SerializableArray2D<int>(1, 1);

			Assert.Throws<IndexOutOfRangeException>(() => _ = array[1, 0]);
		}

		[Test]
		public void IsNullOrEmpty_ReturnsTrueOnlyForMissingOrEmptyBackingElements()
		{
			SerializableArray2D<int> nullArray     = null;
			var                      nonEmptyArray = new SerializableArray2D<int>(1, 1);

			Assert.IsTrue(nullArray.IsNullOrEmpty());
			Assert.IsFalse(nonEmptyArray.IsNullOrEmpty());
		}

		[Test]
		public void LoadFromOneDimensionalArray_WhenDimensionsChange_UpdatesSizeAndData()
		{
			var array = new SerializableArray2D<int>(1, 1);

			array.LoadFrom1DArray(new[] { 1, 2, 3, 4, 5, 6 }, 3, 2);

			Assert.AreEqual(3, array.Width);
			Assert.AreEqual(2, array.Height);
			CollectionAssert.AreEqual(new[] { 1, 2, 3, 4, 5, 6 }, array.GetDataCopy());
		}

		[Test]
		public void Reinitialize_WithDefaultValue_ReplacesExistingData()
		{
			var array = new SerializableArray2D<int>(new[] { 1, 2, 3, 4 }, 2, 2);

			array.Reinitialize(5);

			Assert.AreEqual(2, array.Width);
			Assert.AreEqual(2, array.Height);
			CollectionAssert.AreEqual(new[] { 5, 5, 5, 5 }, array.GetDataCopy());
		}

		[Test]
		public void Resize_PreservesExistingOverlappingValues()
		{
			var array = new SerializableArray2D<int>(new[] { 1, 2, 3, 4 }, 2, 2);

			array.Resize(3, 3);

			Assert.AreEqual(3,            array.Width);
			Assert.AreEqual(3,            array.Height);
			Assert.AreEqual(1,            array[0, 0]);
			Assert.AreEqual(4,            array[1, 1]);
			Assert.AreEqual(default(int), array[2, 2]);
		}

		[Test]
		public void SetElementAt_WithVector2Int_SetsExpectedElement()
		{
			var array = new SerializableArray2D<int>(2, 2);

			array.SetElementAt(new(1, 1), 7);

			Assert.AreEqual(7, array[1, 1]);
		}

		[Test]
		public void TryGetElementInDirection_WhenDirectionIsUnknown_ReturnsFalse()
		{
			var array = new SerializableArray2D<int>(new[] { 1, 2, 3, 4 }, 2, 2);

			bool found = array.TryGetElementInDirection(0, 0, (Direction)999, out int element);

			Assert.IsFalse(found);
			Assert.AreEqual(default(int), element);
		}

		[Test]
		public void TryGetElementInDirection_WhenTargetIsInsideBounds_ReturnsElement()
		{
			var array = new SerializableArray2D<int>(new[] { 1, 2, 3, 4 }, 2, 2);

			bool found = array.TryGetElementInDirection(0, 0, Direction.BottomRight, out int element);

			Assert.IsTrue(found);
			Assert.AreEqual(4, element);
		}

		[Test]
		public void TryGetElementInDirection_WhenTargetIsOutsideBounds_ReturnsFalse()
		{
			var array = new SerializableArray2D<int>(new[] { 1, 2, 3, 4 }, 2, 2);

			bool found = array.TryGetElementLeft(0, 0, out int element);

			Assert.IsFalse(found);
			Assert.AreEqual(default(int), element);
		}
	}
}
