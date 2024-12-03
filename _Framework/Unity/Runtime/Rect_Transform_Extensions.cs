using Bezoro.Framework.Unity.Utilities;
using UnityEngine;

namespace Bezoro.Framework.Unity
{
	public static class Rect_Transform_Extensions
	{
		public static void UpdateUIElementPositionFromWorld(
			this RectTransform rect_transform,
			Transform target,
			Canvas canvas = null,
			Camera camera = null
		)
		{
			if ( rect_transform == null || target == null ) { return; }

			canvas ??= rect_transform.GetComponentInParent<Canvas>();

			if ( canvas == null )
			{
				BDebug.Log_Error( "Canvas not found!" );
			}

			camera ??= Camera.main;

			if ( camera == null )
			{
				BDebug.Log_Error( "No main camera found!" );

				return;
			}

			var     world_position = target.position;
			Vector2 screen_point   = camera.WorldToScreenPoint( world_position );

			var canvas_rect_transform = canvas.GetComponent<RectTransform>();

			if ( canvas_rect_transform == null )
			{
				BDebug.Log_Error( "Canvas RectTransform not found!" );

				return;
			}

			RectTransformUtility.ScreenPointToLocalPointInRectangle(
				canvas_rect_transform,
				screen_point,
				null,
				out var local_point
			);

			rect_transform.anchoredPosition = local_point;
		}
	}
}
