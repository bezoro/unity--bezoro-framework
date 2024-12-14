using NUnit.Framework;

namespace Bezoro.Framework.Core.Tests.EditMode.Array_Helpers
{
	[TestFixture]
	[TestOf(typeof(Core.Array_Helpers))]
	public class Clear
	{
		private int _old_parallel_threshold;

		[SetUp]
		public void SetUp()
		{
			// Save the current parallelization threshold
			_old_parallel_threshold = Core.Array_Helpers.Parallel_Threshold;

			// Set a default parallelization threshold
			Core.Array_Helpers.Parallel_Threshold = 5;
		}

		[TearDown]
		public void TearDown()
		{
			// Restore the default parallelization threshold
			Core.Array_Helpers.Parallel_Threshold = _old_parallel_threshold;
		}

	#region Happy Path

		[Test]
		public void Clear__When_Valid_Size_Smaller_Than_Threshold__Sets_Array_Elements_To_Default()
		{
			// Arrange
			int[]     array      = { 1, 2, 3, 4, 5 };
			const int valid_size = 3;

			// Act
			Core.Array_Helpers.Clear(ref array, valid_size);

			// Assert
			NUnit.Framework.Assert.AreEqual(default( int ), array[0]); // Cleared indices
			NUnit.Framework.Assert.AreEqual(default( int ), array[1]);
			NUnit.Framework.Assert.AreEqual(default( int ), array[2]);
			NUnit.Framework.Assert.AreEqual(4, array[3]); // Unchanged
			NUnit.Framework.Assert.AreEqual(5, array[4]); // Unchanged
		}

		[Test]
		public void Clear__When_Valid_Size_Greater_Than_Threshold__Sets_Array_Elements_To_Default()
		{
			// Arrange
			int[]     array      = { 1, 2, 3, 4, 5, 6, 7, 8 };
			const int valid_size = 6;
			Core.Array_Helpers.Parallel_Threshold = 4;

			// Act
			Core.Array_Helpers.Clear(ref array, valid_size);

			// Assert
			NUnit.Framework.Assert.AreEqual(default( int ), array[0]); // Cleared indices
			NUnit.Framework.Assert.AreEqual(default( int ), array[1]);
			NUnit.Framework.Assert.AreEqual(default( int ), array[2]);
			NUnit.Framework.Assert.AreEqual(default( int ), array[3]);
			NUnit.Framework.Assert.AreEqual(default( int ), array[4]);
			NUnit.Framework.Assert.AreEqual(default( int ), array[5]);
			NUnit.Framework.Assert.AreEqual(7, array[6]); // Unchanged
			NUnit.Framework.Assert.AreEqual(8, array[7]); // Unchanged
		}

		[Test]
		public void Clear__When_Valid_Size_Is_Zero__Does_Not_Modify_Array()
		{
			// Arrange
			int[]     array      = { 1, 2, 3, 4, 5 };
			const int valid_size = 0;

			// Act
			Core.Array_Helpers.Clear(ref array, valid_size);

			// Assert
			NUnit.Framework.Assert.AreEqual(1, array[0]); // Unchanged
			NUnit.Framework.Assert.AreEqual(2, array[1]);
			NUnit.Framework.Assert.AreEqual(3, array[2]);
			NUnit.Framework.Assert.AreEqual(4, array[3]);
			NUnit.Framework.Assert.AreEqual(5, array[4]);
		}

	#endregion
	}
}
