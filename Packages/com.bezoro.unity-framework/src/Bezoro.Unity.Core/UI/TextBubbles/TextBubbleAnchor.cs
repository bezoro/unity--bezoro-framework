#nullable enable

using UnityEngine;

namespace Bezoro.Unity.Core.UI
{
	public readonly struct TextBubbleAnchor
	{
		private TextBubbleAnchor(
			TextBubbleAnchorKind kind,
			Vector2              screenPosition,
			Vector3              worldPosition,
			Transform?           target,
			Vector2              screenOffset)
		{
			Kind           = kind;
			ScreenPosition = screenPosition;
			WorldPosition  = worldPosition;
			Target         = target;
			ScreenOffset   = screenOffset;
		}

		public TextBubbleAnchorKind Kind           { get; }
		public Vector2              ScreenPosition { get; }
		public Vector3              WorldPosition  { get; }
		public Transform?           Target         { get; }
		public Vector2              ScreenOffset   { get; }

		public bool IsValid =>
			Kind switch
			{
				TextBubbleAnchorKind.None           => false,
				TextBubbleAnchorKind.ScreenPosition => true,
				TextBubbleAnchorKind.WorldPosition  => true,
				TextBubbleAnchorKind.Transform      => Target != null,
				_                                   => false
			};

		public static TextBubbleAnchor ForScreenPosition(Vector2 screenPosition, Vector2 screenOffset = default) =>
			new(TextBubbleAnchorKind.ScreenPosition, screenPosition, default, null, screenOffset);

		public static TextBubbleAnchor ForWorldPosition(Vector3 worldPosition, Vector2 screenOffset = default) =>
			new(TextBubbleAnchorKind.WorldPosition, default, worldPosition, null, screenOffset);

		public static TextBubbleAnchor ForTransform(Transform? target, Vector2 screenOffset = default) =>
			new(TextBubbleAnchorKind.Transform, default, default, target, screenOffset);
	}
}
