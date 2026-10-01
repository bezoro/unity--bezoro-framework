using Bezoro.Unity.Core.Common.Helpers;
using NUnit.Framework;
using UnityEngine;

namespace Bezoro.Unity.Core.Tests.EditMode
{
	/// <summary>
	///     Verifies canvas position calculations that do not require a rendered scene.
	/// </summary>
	[TestFixture]
	public sealed class CanvasHelperTests
	{
		private GameObject _canvasObject;

		[TearDown]
		public void TearDown()
		{
			if (_canvasObject != null)
				Object.DestroyImmediate(_canvasObject);
		}

		[Test]
		public void CalculatePositionOnCanvas_ClampsOutOfRangeNormalizedPosition()
		{
			var canvas = CreateCanvas(RenderMode.ScreenSpaceOverlay, new(200f, 100f));

			var position        = CanvasHelper.CalculatePositionOnCanvas(new(2f, -1f), canvas);
			var clampedPosition = CanvasHelper.CalculatePositionOnCanvas(new(1f, 0f),  canvas);

			Assert.AreEqual(clampedPosition, position);
		}

		[Test]
		public void CalculatePositionOnCanvas_ForOverlayCanvas_MapsNormalizedPositionToLocalCanvasPoint()
		{
			var canvas = CreateCanvas(RenderMode.ScreenSpaceOverlay, new(200f, 100f));
			var rect   = canvas.GetComponent<RectTransform>().rect;

			var position = CanvasHelper.CalculatePositionOnCanvas(new(1f, 0f), canvas);

			Assert.AreEqual(new Vector3(rect.width * 0.5f, rect.height * -0.5f, 0f), position);
		}

		[Test]
		public void CalculatePositionOnCanvas_WhenCanvasIsNull_ReturnsZero()
		{
			var position = CanvasHelper.CalculatePositionOnCanvas(new(0.5f, 0.5f), null);

			Assert.AreEqual(Vector3.zero, position);
		}

		private Canvas CreateCanvas(RenderMode renderMode, Vector2 size)
		{
			_canvasObject = new("CanvasHelperTestsCanvas", typeof(RectTransform), typeof(Canvas));
			var rectTransform = _canvasObject.GetComponent<RectTransform>();
			rectTransform.sizeDelta = size;
			var canvas = _canvasObject.GetComponent<Canvas>();
			canvas.renderMode = renderMode;
			return canvas;
		}
	}
}
