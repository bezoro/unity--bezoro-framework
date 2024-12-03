using UnityEngine;

namespace Bezoro.Framework.Unity.Editor.Utilities.Helpers
{
	public static class ExtraGizmos
	{
		public static void DrawWireSquare(
			Vector3 localPosition,
			Quaternion localRotation,
			Vector3 localScale,
			Color color
		)
		{
			var originalMatrix = Gizmos.matrix;
			Gizmos.matrix = Matrix4x4.TRS(
				localPosition,
				localRotation,
				localScale );

			Gizmos.color = color;
			DrawWireSquareInternal();
			Gizmos.matrix = originalMatrix;
		}

		public static void DrawWireSquare(
			Transform transform,
			Vector3 localPositionOffset = default,
			Vector3 localScaleOffset = default,
			Color color = default
		)
		{
			var originalMatrix = Gizmos.matrix;

			Gizmos.matrix = transform.localToWorldMatrix
							* Matrix4x4.TRS(
								localPositionOffset,
								Quaternion.identity,
								Vector3.one + localScaleOffset
							);

			Gizmos.color = color == default ? Color.white : color;
			DrawWireSquareInternal();
			Gizmos.matrix = originalMatrix;
		}

		private static void DrawWireSquareInternal()
		{
			// Draw a wire square instead of a cube
			Gizmos.DrawLine(
				new(
					-0.5f,
					0,
					-0.5f ),
				new(
					0.5f,
					0,
					-0.5f ) ); // Bottom

			Gizmos.DrawLine(
				new(
					0.5f,
					0,
					-0.5f ),
				new(
					0.5f,
					0,
					0.5f ) ); // Right

			Gizmos.DrawLine(
				new(
					0.5f,
					0,
					0.5f ),
				new(
					-0.5f,
					0,
					0.5f ) ); // Top

			Gizmos.DrawLine(
				new(
					-0.5f,
					0,
					0.5f ),
				new(
					-0.5f,
					0,
					-0.5f ) ); // Left
		}
	}
}
