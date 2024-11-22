using UnityEngine;

namespace Bezoro.Framework.Unity
{
	public static class CanvasUtilities
	{
		public static Vector3 CalculateCanvasPosition( Vector2 normalizedPosition, Canvas canvas, Camera camera = null )
		{
			var rectTransform = canvas.GetComponent<RectTransform>();
			switch ( canvas.renderMode )
			{
				case RenderMode.ScreenSpaceOverlay:
					return new(
						( normalizedPosition.x - 0.5f ) * rectTransform.rect.width,
						( normalizedPosition.y - 0.5f ) * rectTransform.rect.height,
						0);
				case RenderMode.ScreenSpaceCamera when camera != null: {
					var screenPoint = camera.ViewportToScreenPoint( normalizedPosition );
					RectTransformUtility.ScreenPointToLocalPointInRectangle(
						rectTransform,
						screenPoint,
						camera,
						out var localPoint
					);
					return localPoint;
				}
				case RenderMode.WorldSpace: {
					var worldPosition = new Vector3(
						( normalizedPosition.x - 0.5f ) * rectTransform.rect.width,
						( normalizedPosition.y - 0.5f ) * rectTransform.rect.height,
						0 );
					return rectTransform.TransformPoint( worldPosition );
				}
				default: return Vector3.zero;
			}
		}
	}
}
