using UnityEngine;

namespace Bezoro.Framework.Unity.Utilities
{
	public static class Vector_Extensions
	{
		public static Vector3[] Calculate_Grid_Positions(
			this Vector2Int grid_size,
			Vector2 cell_size,
			Bounds local_bounds
		)
		{
			var positions = new Vector3[grid_size.x * grid_size.y];

			for ( var y = 0 ; y < grid_size.y ; y++ )
			{
				for ( var x = 0 ; x < grid_size.x ; x++ )
				{
					var index = y * grid_size.x + x;
					positions[ index ] = VectorHelpers.CalculateCellCenter( x, y, cell_size, local_bounds );
				}
			}

			return positions;
		}

		public static Vector2Int To_Grid_Position( this Vector3 world_position, Vector2 cell_size )
		{
			return new(
				Mathf.FloorToInt( world_position.x / cell_size.x ),
				Mathf.FloorToInt( world_position.z / cell_size.y ) );
		}
	}
}
