using UnityEngine;
using Logger = Bezoro.Logging.Logger;

namespace Bezoro.Unity.Core.Common.Helpers;

/// <summary>
///     Finds and modifies Transform hierarchy data.
/// </summary>
public static class TransformHelpers
{
	public static void SetRectTransformPositionRelativeToWorldTransform(
		Camera        camera,
		RectTransform canvasTransform,
		RectTransform screenTransform,
		Transform     worldTransform,
		Vector2       offset
	)
	{
		if (camera == null) camera = Camera.main;

		if (camera == null || canvasTransform == null || screenTransform == null || worldTransform == null)
		{
			Logger.LogWarning("Cannot position RectTransform because one or more required references are null.");
			return;
		}

		var viewportPosition = camera.WorldToViewportPoint
		(
			worldTransform.position + Vector3.up * offset.y + Vector3.right * offset.x
		);

		var sizeDelta = canvasTransform.sizeDelta;

		var worldObjectScreenPosition = new Vector2(
			viewportPosition.x * sizeDelta.x - sizeDelta.x * 0.5f,
			viewportPosition.y * sizeDelta.y - sizeDelta.y * 0.5f
		);

		screenTransform.anchoredPosition = worldObjectScreenPosition;
	}
}
