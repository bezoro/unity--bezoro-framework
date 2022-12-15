using UnityEngine;

namespace Bezoro.Core.Helpers
{
	public static class TransformHelpers
	{
		public static void SetRectTransformPositionRelativeToWorldTransform
		(
			Camera camera,
			RectTransform canvasTransform,
			RectTransform screenTransform,
			Transform worldTransform,
			Vector2 offset
		)
		{
			var viewportPosition = camera.WorldToViewportPoint
				(worldTransform.position + Vector3.up * offset.y + Vector3.right * offset.x);

			var worldObjectScreenPosition = new Vector2
			(
				viewportPosition.x          * canvasTransform.sizeDelta.x -
				canvasTransform.sizeDelta.x * 0.5f,
				viewportPosition.y          * canvasTransform.sizeDelta.y -
				canvasTransform.sizeDelta.y * 0.5f
			);

			screenTransform.anchoredPosition = worldObjectScreenPosition;
		}
	}
}