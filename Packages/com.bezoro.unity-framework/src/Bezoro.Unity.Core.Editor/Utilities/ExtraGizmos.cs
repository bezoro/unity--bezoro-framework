using UnityEngine;

namespace Bezoro.Unity.Core.Editor.Utilities
{
	/// <summary>
	///     Draws additional editor gizmo shapes.
	/// </summary>
	public static class ExtraGizmos
	{
		public static void DrawWireSquare(
			Vector3    localPosition,
			Quaternion localRotation,
			Vector3    localScale,
			Color      color
		)
		{
			var originalMatrix = Gizmos.matrix;
			Gizmos.matrix = Matrix4x4.TRS(
				localPosition,
				localRotation,
				localScale
			);

			Gizmos.color = color;
			Draw_Wire_Square_Internal(
				localPosition,
				localRotation,
				localScale.x
			);

			Gizmos.matrix = originalMatrix;
		}

		public static void DrawWireSquare(
			Transform transform,
			Vector3   localPositionOffset = default,
			Vector3   localScaleOffset    = default,
			Color     color               = default
		)
		{
			var originalMatrix = Gizmos.matrix;

			Gizmos.matrix = transform.localToWorldMatrix *
							Matrix4x4.TRS(
								localPositionOffset,
								Quaternion.identity,
								Vector3.one + localScaleOffset
							);

			Gizmos.color = color == default ? Color.white : color;

			Draw_Wire_Square_Internal(
				transform.position,
				transform.rotation,
				transform.lossyScale.x + localScaleOffset.x
			);

			Gizmos.matrix = originalMatrix;
		}

		private static void Draw_Wire_Square_Internal(
			Vector3    worldPosition = default,
			Quaternion worldRotation = default,
			float      worldScale    = 1.0f
		)
		{
			Gizmos.matrix = Matrix4x4.TRS(
				worldPosition,
				worldRotation,
				Vector3.one
			);

			Gizmos.DrawLine(
				new(
					0.5f * worldScale,
					0,
					0.5f * worldScale
				),
				new(
					-0.5f * worldScale,
					0,
					0.5f * worldScale
				)
			); // Top

			Gizmos.DrawLine(
				new(
					-0.5f * worldScale,
					0,
					-0.5f * worldScale
				),
				new(
					0.5f * worldScale,
					0,
					-0.5f * worldScale
				)
			); // Bottom

			Gizmos.DrawLine(
				new(
					-0.5f * worldScale,
					0,
					0.5f * worldScale
				),
				new(
					-0.5f * worldScale,
					0,
					-0.5f * worldScale
				)
			); // Left

			Gizmos.DrawLine(
				new(
					0.5f * worldScale,
					0,
					-0.5f * worldScale
				),
				new(
					0.5f * worldScale,
					0,
					0.5f * worldScale
				)
			); // Right

			Gizmos.matrix = Matrix4x4.identity;
		}
	}
}
