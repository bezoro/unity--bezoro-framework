using Bezoro.Unity.Core.Common.Extensions.Vector;
using NUnit.Framework;
using UnityEngine;

namespace Bezoro.Unity.Core.Tests.EditMode
{
	/// <summary>
	///     Verifies Vector3 grid conversion helpers.
	/// </summary>
	[TestFixture]
	public sealed class VectorExtensionsTests
	{
		[Test]
		public void CalculateGridPositions_ReturnsCellCentersInRowMajorOrder()
		{
			var bounds = new Bounds(Vector3.zero, new(2f, 1f, 2f));

			var positions = new Vector2Int(2, 2).Calculate_Grid_Positions(new(1f, 1f), bounds);

			CollectionAssert.AreEqual(
				new[]
				{
					new Vector3(-0.5f, 0.5f, -0.5f),
					new Vector3(0.5f,  0.5f, -0.5f),
					new Vector3(-0.5f, 0.5f, 0.5f),
					new Vector3(0.5f,  0.5f, 0.5f)
				},
				positions
			);
		}

		[Test]
		public void ToGridPosition_UsesWorldXAndZWithCellSize()
		{
			var gridPosition = new Vector3(2.9f, 10f, 4.1f).To_Grid_Position(new(1f, 2f));

			Assert.AreEqual(new Vector2Int(2, 2), gridPosition);
		}
	}
}
