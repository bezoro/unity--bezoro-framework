using Bezoro.Framework.Unity.Utilities.Helpers;
using UnityEngine;

namespace Bezoro.Framework.Unity.Utilities.Extensions
{
	public static class VectorExtensions
	{
		public static Vector3[] CalculateGridPositions(this Vector2Int gridSize, Vector2 cellSize, Bounds localBounds)
		{
			var positions = new Vector3[gridSize.x * gridSize.y];

			for ( var y = 0 ; y < gridSize.y ; y++ )
			{
				for ( var x = 0 ; x < gridSize.x ; x++ )
				{
					var index = y * gridSize.x + x;
					positions[index] = VectorHelpers.CalculateLocalPosition(x, y, cellSize, localBounds);
				}
			}

			return positions;
		}

		public static Vector2Int ToGridPosition(this Vector3 worldPosition, Vector2 cellSize)
		{
			return new Vector2Int(
				Mathf.FloorToInt(worldPosition.x / cellSize.x),
				Mathf.FloorToInt(worldPosition.z / cellSize.y)
			);
		}
	}
}
