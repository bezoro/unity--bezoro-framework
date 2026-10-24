#nullable enable

using Bezoro.Unity.Core.UI;
using NUnit.Framework;
using UnityEngine;

namespace Bezoro.Unity.Core.Tests.EditMode
{
	[TestFixture]
	public sealed class TextBubbleRequestTests
	{
		[Test]
		public void Validate_WhenScreenRequestHasText_ReturnsNoErrors()
		{
			var request = new TextBubbleRequest(
				"Show the hint",
				TextBubbleAnchor.ForScreenPosition(new(120f, 240f))
			);

			var result = request.Validate();

			Assert.IsFalse(result.HasErrors, string.Join("\n", result.Messages));
		}

		[Test]
		public void Validate_WhenTextIsEmptyAndEmptyTextIsNotAllowed_ReturnsError()
		{
			var request = new TextBubbleRequest(
				string.Empty,
				TextBubbleAnchor.ForScreenPosition(new(120f, 240f))
			);

			var result = request.Validate();

			Assert.IsTrue(result.HasErrors);
			AssertMessage(result, "Text bubble text is required.");
		}

		[Test]
		public void Validate_WhenEmptyTextIsAllowed_ReturnsNoTextError()
		{
			var request = new TextBubbleRequest(
				string.Empty,
				TextBubbleAnchor.ForScreenPosition(new(120f, 240f)),
				allowEmptyText: true
			);

			var result = request.Validate();

			Assert.IsFalse(result.HasErrors, string.Join("\n", result.Messages));
		}

		[Test]
		public void Validate_WhenLifetimeIsNegative_ReturnsError()
		{
			var request = new TextBubbleRequest(
				"Temporary bubble",
				TextBubbleAnchor.ForScreenPosition(new(120f, 240f)),
				lifetimeSeconds: -1f
			);

			var result = request.Validate();

			Assert.IsTrue(result.HasErrors);
			AssertMessage(result, "Text bubble lifetime cannot be negative.");
		}

		[TestCase(float.NaN)]
		[TestCase(float.PositiveInfinity)]
		public void Validate_WhenLifetimeIsNotFinite_ReturnsError(float lifetimeSeconds)
		{
			var request = new TextBubbleRequest(
				"Temporary bubble",
				TextBubbleAnchor.ForScreenPosition(new(120f, 240f)),
				lifetimeSeconds: lifetimeSeconds
			);

			var result = request.Validate();

			Assert.IsTrue(result.HasErrors);
			AssertMessage(result, "Text bubble lifetime must be finite.");
		}

		[Test]
		public void Validate_WhenAnchorIsDefault_ReturnsError()
		{
			var request = new TextBubbleRequest(
				"Invalid anchor",
				default
			);

			var result = request.Validate();

			Assert.IsTrue(result.HasErrors);
			AssertMessage(result, "Text bubble anchor is invalid.");
		}

		[Test]
		public void Validate_WhenTransformAnchorHasNoTarget_ReturnsError()
		{
			var request = new TextBubbleRequest(
				"Follow target",
				TextBubbleAnchor.ForTransform(null)
			);

			var result = request.Validate();

			Assert.IsTrue(result.HasErrors);
			AssertMessage(result, "Text bubble transform anchor requires a target.");
		}

		[Test]
		public void ForTransform_WhenTargetExists_StoresTargetAndOffset()
		{
			var target = new GameObject("TextBubbleAnchorTarget");

			try
			{
				var anchor = TextBubbleAnchor.ForTransform(target.transform, new(8f, 12f));

				Assert.AreEqual(TextBubbleAnchorKind.Transform, anchor.Kind);
				Assert.AreSame(target.transform, anchor.Target);
				Assert.AreEqual(new Vector2(8f, 12f), anchor.ScreenOffset);
				Assert.IsTrue(anchor.IsValid);
			}
			finally
			{
				Object.DestroyImmediate(target);
			}
		}

		[Test]
		public void WithText_ReplacesTextAndPreservesOtherFields()
		{
			var camera = new GameObject("TextBubbleRequestCamera").AddComponent<Camera>();

			try
			{
				var anchor = TextBubbleAnchor.ForWorldPosition(new(1f, 2f, 3f), new(4f, 5f));
				var request = new TextBubbleRequest(
					"Original",
					anchor,
					visibleOnCreate: false,
					lifetimeSeconds: 2.5f,
					cameraOverride: camera,
					allowEmptyText: true,
					styleId: "hint"
				);

				var updated = request.WithText("Updated");

				Assert.AreEqual("Updated", updated.Text);
				Assert.AreEqual(request.Anchor.Kind, updated.Anchor.Kind);
				Assert.AreEqual(request.Anchor.WorldPosition, updated.Anchor.WorldPosition);
				Assert.AreEqual(request.Anchor.ScreenOffset, updated.Anchor.ScreenOffset);
				Assert.AreEqual(request.VisibleOnCreate, updated.VisibleOnCreate);
				Assert.AreEqual(request.LifetimeSeconds, updated.LifetimeSeconds);
				Assert.AreSame(request.CameraOverride, updated.CameraOverride);
				Assert.AreEqual(request.AllowEmptyText, updated.AllowEmptyText);
				Assert.AreEqual(request.StyleId, updated.StyleId);
			}
			finally
			{
				Object.DestroyImmediate(camera.gameObject);
			}
		}

		[Test]
		public void WithAnchor_ReplacesAnchorAndPreservesOtherFields()
		{
			var camera = new GameObject("TextBubbleRequestCamera").AddComponent<Camera>();

			try
			{
				var request = new TextBubbleRequest(
					"Original",
					TextBubbleAnchor.ForScreenPosition(new(1f, 2f)),
					visibleOnCreate: false,
					lifetimeSeconds: 2.5f,
					cameraOverride: camera,
					allowEmptyText: true,
					styleId: "hint"
				);
				var replacement = TextBubbleAnchor.ForWorldPosition(new(3f, 4f, 5f), new(6f, 7f));

				var updated = request.WithAnchor(replacement);

				Assert.AreEqual(request.Text, updated.Text);
				Assert.AreEqual(TextBubbleAnchorKind.WorldPosition, updated.Anchor.Kind);
				Assert.AreEqual(replacement.WorldPosition, updated.Anchor.WorldPosition);
				Assert.AreEqual(replacement.ScreenOffset, updated.Anchor.ScreenOffset);
				Assert.AreEqual(request.VisibleOnCreate, updated.VisibleOnCreate);
				Assert.AreEqual(request.LifetimeSeconds, updated.LifetimeSeconds);
				Assert.AreSame(request.CameraOverride, updated.CameraOverride);
				Assert.AreEqual(request.AllowEmptyText, updated.AllowEmptyText);
				Assert.AreEqual(request.StyleId, updated.StyleId);
			}
			finally
			{
				Object.DestroyImmediate(camera.gameObject);
			}
		}

		private static void AssertMessage(TextBubbleRequestValidationResult result, string expected)
		{
			if (ContainsMessage(result, expected))
				return;

			Assert.Fail($"Expected validation message '{expected}'. Actual:\n{string.Join("\n", result.Messages)}");
		}

		private static bool ContainsMessage(TextBubbleRequestValidationResult result, string expected)
		{
			for (var i = 0; i < result.Messages.Count; i++)
			{
				if (result.Messages[i] == expected)
					return true;
			}

			return false;
		}
	}
}
