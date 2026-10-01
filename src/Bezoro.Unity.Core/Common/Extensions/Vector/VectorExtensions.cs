using Bezoro.Unity.Core.Common.Helpers;
using UnityEngine;

namespace Bezoro.Unity.Core.Common.Extensions.Vector;

/// <summary>
///     Contains extension methods for Vector.
/// </summary>
public static class VectorExtensions
{
	public static Vector2Int To_Grid_Position(this Vector3 worldPosition, Vector2 cellSize) =>
		new(
			Mathf.FloorToInt(
				worldPosition.x / cellSize.x
			),
			Mathf.FloorToInt(
				worldPosition.z / cellSize.y
			)
		);

	public static Vector3[] Calculate_Grid_Positions(
		this Vector2Int gridSize,
		Vector2         cellSize,
		Bounds          localBounds
	)
	{
		var positions = new Vector3[gridSize.x * gridSize.y];

		for (var y = 0; y < gridSize.y; y++)
		{
			for (var x = 0; x < gridSize.x; x++)
			{
				int index = y * gridSize.x + x;
				positions[index] = VectorHelpers.CalculateCellCenter(
					x,
					y,
					cellSize,
					localBounds
				);
			}
		}

		return positions;
	}
}
