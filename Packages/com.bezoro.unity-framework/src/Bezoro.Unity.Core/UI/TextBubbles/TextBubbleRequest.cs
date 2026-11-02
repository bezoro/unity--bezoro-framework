#nullable enable

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Bezoro.Unity.Core.UI
{
	public readonly struct TextBubbleRequest
	{
		public TextBubbleRequest(
			string           text,
			TextBubbleAnchor anchor,
			bool             visibleOnCreate = true,
			float            lifetimeSeconds = 0f,
			Camera?          cameraOverride = null,
			bool             allowEmptyText = false,
			string?          styleId = null)
		{
			Text            = text ?? string.Empty;
			Anchor          = anchor;
			VisibleOnCreate = visibleOnCreate;
			LifetimeSeconds = lifetimeSeconds;
			CameraOverride  = cameraOverride;
			AllowEmptyText  = allowEmptyText;
			StyleId         = styleId ?? string.Empty;
		}

		public string           Text            { get; }
		public TextBubbleAnchor Anchor          { get; }
		public bool             VisibleOnCreate { get; }
		public float            LifetimeSeconds { get; }
		public Camera?          CameraOverride  { get; }
		public bool             AllowEmptyText  { get; }
		public string           StyleId         { get; }

		public TextBubbleRequestValidationResult Validate()
		{
			var messages = new List<string>();

			if (!AllowEmptyText && string.IsNullOrWhiteSpace(Text))
				messages.Add("Text bubble text is required.");

			if (!Anchor.IsValid)
			{
				if (Anchor.Kind == TextBubbleAnchorKind.Transform)
					messages.Add("Text bubble transform anchor requires a target.");
				else
					messages.Add("Text bubble anchor is invalid.");
			}

			if (float.IsNaN(LifetimeSeconds) || float.IsInfinity(LifetimeSeconds))
				messages.Add("Text bubble lifetime must be finite.");
			else if (LifetimeSeconds < 0f)
				messages.Add("Text bubble lifetime cannot be negative.");

			return new(messages);
		}

		public TextBubbleRequest WithText(string text) =>
			new(
				text,
				Anchor,
				VisibleOnCreate,
				LifetimeSeconds,
				CameraOverride,
				AllowEmptyText,
				StyleId
			);

		public TextBubbleRequest WithAnchor(TextBubbleAnchor anchor) =>
			new(
				Text,
				anchor,
				VisibleOnCreate,
				LifetimeSeconds,
				CameraOverride,
				AllowEmptyText,
				StyleId
			);
	}

	public readonly struct TextBubbleRequestValidationResult
	{
		public TextBubbleRequestValidationResult(IReadOnlyList<string> messages)
		{
			Messages = messages ?? Array.Empty<string>();
		}

		public IReadOnlyList<string> Messages { get; }
		public bool HasErrors => Messages.Count > 0;
	}
}
