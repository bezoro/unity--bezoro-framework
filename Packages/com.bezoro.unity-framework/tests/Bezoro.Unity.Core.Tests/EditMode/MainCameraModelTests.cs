using Bezoro.Unity.Core.MonoBehaviours;
using NUnit.Framework;

namespace Bezoro.Unity.Core.Tests.EditMode
{
	[TestFixture]
	public sealed class MainCameraModelTests
	{
		[Test]
		public void GetFadeInAlpha_ReturnsOpaqueAlpha()
		{
			var model = new MainCameraModel();

			Assert.AreEqual(1f, model.GetFadeInAlpha());
		}

		[TestCase(true,  true)]
		[TestCase(false, false)]
		public void ShouldActivateCanvas_ReflectsCanvasGameObjectAvailability(bool hasCanvasGameObject, bool expected)
		{
			var model = new MainCameraModel();

			Assert.AreEqual(expected, model.ShouldActivateCanvas(hasCanvasGameObject));
		}

		[TestCase(true,  true)]
		[TestCase(false, false)]
		public void ShouldActivateFadeImage_ReflectsFadeImageAvailability(bool hasFadeImage, bool expected)
		{
			var model = new MainCameraModel();

			Assert.AreEqual(expected, model.ShouldActivateFadeImage(hasFadeImage));
		}

		[TestCase(true,  true)]
		[TestCase(false, false)]
		public void ShouldApplyFadeColor_ReflectsFadeImageAvailability(bool hasFadeImage, bool expected)
		{
			var model = new MainCameraModel();

			Assert.AreEqual(expected, model.ShouldApplyFadeColor(hasFadeImage));
		}

		[TestCase(true,  true)]
		[TestCase(false, false)]
		public void ShouldFadeImage_ReflectsFadeImageAvailability(bool hasFadeImage, bool expected)
		{
			var model = new MainCameraModel();

			Assert.AreEqual(expected, model.ShouldFadeImage(hasFadeImage));
		}

		[TestCase(true,  true,  true)]
		[TestCase(true,  false, false)]
		[TestCase(false, true,  false)]
		[TestCase(false, false, false)]
		public void ShouldFadeOut_RequiresCanvasAndCanvasGameObject(
			bool hasCanvas,
			bool hasCanvasGameObject,
			bool expected)
		{
			var model = new MainCameraModel();

			Assert.AreEqual(expected, model.ShouldFadeOut(hasCanvas, hasCanvasGameObject));
		}

		[TestCase(true,  true)]
		[TestCase(false, false)]
		public void ShouldResetFadeAlpha_ReflectsFadeImageAvailability(bool hasFadeImage, bool expected)
		{
			var model = new MainCameraModel();

			Assert.AreEqual(expected, model.ShouldResetFadeAlpha(hasFadeImage));
		}
	}
}
