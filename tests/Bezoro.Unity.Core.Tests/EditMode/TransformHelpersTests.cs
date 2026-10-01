using Bezoro.Unity.Core.Common.Helpers;
using NUnit.Framework;
using UnityEngine;

namespace Bezoro.Unity.Core.Tests.EditMode
{
	/// <summary>
	///     Verifies transform helper positioning behavior.
	/// </summary>
	[TestFixture]
	public sealed class TransformHelpersTests
	{
		private GameObject _cameraObject;
		private GameObject _canvasObject;
		private GameObject _screenObject;
		private GameObject _worldObject;

		[TearDown]
		public void TearDown()
		{
			if (_cameraObject != null)
				Object.DestroyImmediate(_cameraObject);

			if (_canvasObject != null)
				Object.DestroyImmediate(_canvasObject);

			if (_screenObject != null)
				Object.DestroyImmediate(_screenObject);

			if (_worldObject != null)
				Object.DestroyImmediate(_worldObject);
		}

		[Test]
		public void SetRectTransformPositionRelativeToWorldTransform_MapsWorldPositionIntoCanvasSpace()
		{
			_cameraObject = new("TransformHelpersTestsCamera", typeof(Camera));
			var camera = _cameraObject.GetComponent<Camera>();
			camera.orthographic       = true;
			camera.orthographicSize   = 5f;
			camera.transform.position = new(0f, 0f, -10f);

			var canvas = CreateRectTransform(ref _canvasObject, "CanvasTransform", new(200f, 100f));
			var screen = CreateRectTransform(ref _screenObject, "ScreenTransform", new(10f, 10f));
			_worldObject                    = new("WorldTransform");
			_worldObject.transform.position = Vector3.zero;

			TransformHelpers.SetRectTransformPositionRelativeToWorldTransform(
				camera,
				canvas,
				screen,
				_worldObject.transform,
				Vector2.zero
			);

			Assert.That(screen.anchoredPosition.x, Is.EqualTo(0f).Within(0.001f));
			Assert.That(screen.anchoredPosition.y, Is.EqualTo(0f).Within(0.001f));
		}

		[Test]
		public void SetRectTransformPositionRelativeToWorldTransform_WhenReferenceIsMissing_DoesNotMoveScreenTransform()
		{
			var screen = CreateRectTransform(ref _screenObject, "ScreenTransform", new(100f, 100f));
			screen.anchoredPosition = new(3f, 4f);

			TransformHelpers.SetRectTransformPositionRelativeToWorldTransform(
				null,
				null,
				screen,
				null,
				Vector2.zero
			);

			Assert.AreEqual(new Vector2(3f, 4f), screen.anchoredPosition);
		}

		private static RectTransform CreateRectTransform(ref GameObject gameObject, string name, Vector2 size)
		{
			gameObject = new(name, typeof(RectTransform));
			var rectTransform = gameObject.GetComponent<RectTransform>();
			rectTransform.sizeDelta = size;
			return rectTransform;
		}
	}
}
