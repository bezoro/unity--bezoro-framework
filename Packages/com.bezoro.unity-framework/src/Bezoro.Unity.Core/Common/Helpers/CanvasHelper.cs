using Bezoro.Unity.Core.Common.Extensions;
using UnityEngine;
using Logger = Bezoro.Logging.Logger;

namespace Bezoro.Unity.Core.Common.Helpers;

/// <summary>
///     Resolves canvas-related references for UI objects.
/// </summary>
public static class CanvasHelper
{
	/// <summary>
	///     Calculates the world position on a canvas based on normalized viewport coordinates.
	/// </summary>
	/// <param name="normalizedPosition">Normalized coordinates of the point to locate.</param>
	/// <param name="canvas">The canvas to evaluate the position on.</param>
	/// <param name="camera">Optional: The camera associated with the canvas, necessary for RenderMode.ScreenSpaceCamera.</param>
	/// <returns>Returns the world position on the canvas.</returns>
	public static Vector3 CalculatePositionOnCanvas(
		Vector2 normalizedPosition,
		Canvas  canvas,
		Camera? camera = null
	)
	{
		if (canvas == null)
		{
			Logger.LogWarning("Cannot calculate canvas position because canvas is null.");
			return Vector3.zero;
		}

		if (normalizedPosition.x < 0 ||
			normalizedPosition.x > 1 ||
			normalizedPosition.y < 0 ||
			normalizedPosition.y > 1)
		{
			normalizedPosition.x = Mathf.Clamp01(
				normalizedPosition.x
			);

			normalizedPosition.y = Mathf.Clamp01(
				normalizedPosition.y
			);

			Logger.LogWarning(
				$"{nameof(normalizedPosition)} was out of [0..1] range. Clamping."
			);
		}

		var rectTransform = canvas.GetComponent<RectTransform>();

		if (rectTransform.IsUnityNull())
		{
			Logger.LogException(new("There is no RectTransform in the Canvas."));
			return Vector3.zero;
		}

		switch (canvas.renderMode)
		{
			case RenderMode.ScreenSpaceOverlay:
				return new(
					(normalizedPosition.x - 0.5f) * rectTransform.rect.width,
					(normalizedPosition.y - 0.5f) * rectTransform.rect.height,
					0
				);

			case RenderMode.ScreenSpaceCamera when camera != null:
			{
				var screenPoint = camera.ViewportToScreenPoint(
					normalizedPosition
				);

				RectTransformUtility.ScreenPointToLocalPointInRectangle(
					rectTransform,
					screenPoint,
					camera,
					out var localPoint
				);

				return localPoint;
			}

			case RenderMode.WorldSpace:
			{
				var worldPosition = new Vector3(
					(normalizedPosition.x - 0.5f) * rectTransform.rect.width,
					(normalizedPosition.y - 0.5f) * rectTransform.rect.height,
					0
				);

				return rectTransform.TransformPoint(
					worldPosition
				);
			}

			default:
				Logger.LogException(
					new(
						$"Unknown canvas render mode: {canvas.renderMode}"
					)
				);

				return Vector3.zero;
		}
	}
}
