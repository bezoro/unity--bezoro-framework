using UnityEngine;

public static class RectTransformExtensions
{
	public static void UpdateUIElementPositionFromWorld
	(
		this RectTransform uiElement,
		Transform target,
		Canvas canvas = null,
		Camera camera = null
	)
	{
		if ( uiElement == null || target == null ) { return; }

		canvas ??= uiElement.GetComponentInParent<Canvas>();
		if ( canvas == null )
		{
			BDebug.LogError( "Canvas not found!" );
		}

		camera ??= Camera.main;
		if ( camera == null )
		{
			BDebug.LogError( "No main camera found!" );

			return;
		}

		var worldPosition = target.position;
		Vector2 screenPoint = camera.WorldToScreenPoint( worldPosition );

		var canvasRectTransform = canvas.GetComponent<RectTransform>();
		if ( canvasRectTransform == null )
		{
			BDebug.LogError( "Canvas RectTransform not found!" );

			return;
		}

		RectTransformUtility.ScreenPointToLocalPointInRectangle(
			canvasRectTransform,
			screenPoint,
			null,
			out var localPoint
		);
		uiElement.anchoredPosition = localPoint;
	}
}
