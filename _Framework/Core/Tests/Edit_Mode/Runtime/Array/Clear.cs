using System.Linq;
using NUnit.Framework;

namespace Bezoro.Framework.Core.Tests.Edit_Mode.Array_Helpers
{
	[TestFixture]
	[TestOf(typeof(Core.Array_Helpers))]
	public class Clear
	{
		#region Happy Path

		[Test]
		public void Uses_Sequential_Method_When_Size_Is_Less_Than_Threshold()
		{
			// Arrange
			int[] array = { 1, 2, 3, 4, 5 };

			// Act
			Core.Array_Helpers.Clear(ref array);

			// Assert
			NUnit.Framework.Assert.IsTrue(array.All(element => element == default));
		}

		[Test]
		public void Uses_Parallel_Method_When_Size_Is_Greater_Than_Threshold()
		{
			// Arrange
			var threshold = Core.Array_Helpers.Parallel_Threshold;
			var array     = new int[threshold * 2];

			for (var i = 0 ; i < array.Length ; i++)
				array[i] = i + 1;

			// Act
			Core.Array_Helpers.Clear(ref array);

			// Assert
			NUnit.Framework.Assert.IsTrue(array.All(element => element == default));
		}

		#endregion
	}
}
