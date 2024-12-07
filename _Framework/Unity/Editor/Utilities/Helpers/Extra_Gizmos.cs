using UnityEngine;

namespace Bezoro.Framework.Unity.Editor.Utilities.Helpers
{
	public static class Extra_Gizmos
	{
		public static void DrawWireSquare(
			Vector3 localPosition,
			Quaternion localRotation,
			Vector3 localScale,
			Color color
		)
		{
			var original_matrix = Gizmos.matrix;
			Gizmos.matrix = Matrix4x4.TRS( localPosition, localRotation, localScale );

			Gizmos.color = color;
			Draw_Wire_Square_Internal( localPosition, localRotation, localScale.x );
			Gizmos.matrix = original_matrix;
		}

		public static void DrawWireSquare(
			Transform transform,
			Vector3 localPositionOffset = default,
			Vector3 localScaleOffset = default,
			Color color = default
		)
		{
			var original_matrix = Gizmos.matrix;

			Gizmos.matrix = transform.localToWorldMatrix * Matrix4x4.TRS(
				localPositionOffset, Quaternion.identity, Vector3.one + localScaleOffset
			);

			Gizmos.color = color == default ? Color.white : color;
			Draw_Wire_Square_Internal(
				transform.position,
				transform.rotation,
				transform.lossyScale.x + localScaleOffset.x
			);

			Gizmos.matrix = original_matrix;
		}

		private static void Draw_Wire_Square_Internal(
			Vector3 world_position = default,
			Quaternion world_rotation = default,
			float world_scale = 1.0f
		)
		{
			Gizmos.matrix = Matrix4x4.TRS( world_position, world_rotation, Vector3.one );

			Gizmos.DrawLine(
				new( 0.5f * world_scale, 0, 0.5f * world_scale ),
				new( -0.5f * world_scale, 0, 0.5f * world_scale )
			); // Top

			Gizmos.DrawLine(
				new( -0.5f * world_scale, 0, -0.5f * world_scale ),
				new( 0.5f * world_scale, 0, -0.5f * world_scale )
			); // Bottom

			Gizmos.DrawLine(
				new( -0.5f * world_scale, 0, 0.5f * world_scale ),
				new( -0.5f * world_scale, 0, -0.5f * world_scale )
			); // Left

			Gizmos.DrawLine(
				new( 0.5f * world_scale, 0, -0.5f * world_scale ),
				new( 0.5f * world_scale, 0, 0.5f * world_scale )
			); // Right

			Gizmos.matrix = Matrix4x4.identity;
		}
	}
}
