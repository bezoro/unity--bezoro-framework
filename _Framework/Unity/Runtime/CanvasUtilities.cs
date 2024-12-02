using Bezoro.Framework.Unity.Utilities;
using UnityEngine;

namespace Bezoro.Framework.Unity
{
	public static class CanvasUtilities
	{
		public static Vector3 CalculateCanvasPosition( Vector2 normalizedPosition, Canvas canvas, Camera camera = null )
		{
			if ( normalizedPosition.x < 0 || normalizedPosition.x > 1
			  || normalizedPosition.y < 0 || normalizedPosition.y > 1 )
			{
				normalizedPosition.x = Mathf.Clamp01( normalizedPosition.x );
				normalizedPosition.y = Mathf.Clamp01( normalizedPosition.y );
				BDebug.Log_Warning( $"{nameof(normalizedPosition)} was out of [0..1] range. Clamping." );
			}
			var rect_transform = canvas.GetComponent<RectTransform>();
			switch ( canvas.renderMode )
			{
				case RenderMode.ScreenSpaceOverlay:
					return new(( normalizedPosition.x - 0.5f ) * rect_transform.rect.width,
								  ( normalizedPosition.y - 0.5f ) * rect_transform.rect.height,
								  0);
				case RenderMode.ScreenSpaceCamera when camera != null: {
					var screen_point = camera.ViewportToScreenPoint( normalizedPosition );
					RectTransformUtility.ScreenPointToLocalPointInRectangle(
						rect_transform,
						screen_point,
						camera,
						out var local_point );

					return local_point;
				}
				case RenderMode.WorldSpace: {
					var world_position = new Vector3(
						( normalizedPosition.x - 0.5f ) * rect_transform.rect.width,
						( normalizedPosition.y - 0.5f ) * rect_transform.rect.height,
						0 );
					return rect_transform.TransformPoint( world_position );
				}
				default: return Vector3.zero;
			}
		}
	}
}
