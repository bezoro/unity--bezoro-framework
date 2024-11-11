using UnityEngine;
using UnityEngine.Assertions;

namespace Bezoro.Framework.Unity.Utilities.Helpers
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
			if (camera == null)
			{
				camera = Camera.main;
			}

			Assert.IsNotNull(camera);

			var viewportPosition = camera.WorldToViewportPoint
				(worldTransform.position + Vector3.up * offset.y + Vector3.right * offset.x);

			var sizeDelta = canvasTransform.sizeDelta;

			var worldObjectScreenPosition = new Vector2(
				viewportPosition.x * sizeDelta.x - sizeDelta.x * 0.5f,
				viewportPosition.y * sizeDelta.y - sizeDelta.y * 0.5f
			);

			screenTransform.anchoredPosition = worldObjectScreenPosition;
		}
	}
}
