using Bezoro.Unity.Core.Common.Helpers;
using Bezoro.Unity.Core.Common.Primitives;
using NUnit.Framework;
using CoreColor = Bezoro.Unity.Core.Common.Primitives.Color;
using UnityColor = UnityEngine.Color;

namespace Bezoro.Unity.Core.Tests.EditMode
{
	/// <summary>
	///     Verifies serializable color primitives and HSL conversion helpers.
	/// </summary>
	[TestFixture]
	public sealed class ColorPrimitiveTests
	{
		[Test]
		public void ColorConstructor_ClampsAndRoundsFloatChannelsToBytes()
		{
			var color = new CoreColor(-1f, 0.5f, 2f, 0.25f);

			Assert.AreEqual(0,   color.R);
			Assert.AreEqual(128, color.G);
			Assert.AreEqual(255, color.B);
			Assert.AreEqual(64,  color.A);
		}

		[TestCase(0f,   "#FF0000", 1f, 0f, 0f)]
		[TestCase(120f, "#00FF00", 0f, 1f, 0f)]
		[TestCase(240f, "#0000FF", 0f, 0f, 1f)]
		public void Colour_WhenSaturatedPrimaryHue_ReturnsExpectedRgbAndHex(
			float  hue,
			string expectedHex,
			float  expectedR,
			float  expectedG,
			float  expectedB
		)
		{
			var colour = new Colour(hue, 100f, 50f);

			UnityColor rgb = colour;

			Assert.That(rgb.r, Is.EqualTo(expectedR).Within(0.0001f));
			Assert.That(rgb.g, Is.EqualTo(expectedG).Within(0.0001f));
			Assert.That(rgb.b, Is.EqualTo(expectedB).Within(0.0001f));
			Assert.AreEqual(expectedHex, colour.Hex);
		}

		[Test]
		public void Colour_WhenSaturationIsZero_ReturnsGrayRgbAndHex()
		{
			var colour = new Colour(180f, 0f, 50f);

			UnityColor rgb = colour;

			Assert.That(rgb.r, Is.EqualTo(0.5f).Within(0.0001f));
			Assert.That(rgb.g, Is.EqualTo(0.5f).Within(0.0001f));
			Assert.That(rgb.b, Is.EqualTo(0.5f).Within(0.0001f));
			Assert.AreEqual("#7F7F7F", colour.Hex);
		}

		[Test]
		public void Gray_ReturnsHalfIntensityRgbWithFullAlpha()
		{
			var color = CoreColor.Gray();

			Assert.AreEqual(128, color.R);
			Assert.AreEqual(128, color.G);
			Assert.AreEqual(128, color.B);
			Assert.AreEqual(255, color.A);
		}

		[TestCase(0f,   1f, 0f, 0f)]
		[TestCase(120f, 0f, 1f, 0f)]
		[TestCase(240f, 0f, 0f, 1f)]
		[TestCase(360f, 1f, 0f, 0f)]
		public void HslToRgb_WhenSaturatedPrimaryHue_ReturnsExpectedRgb(
			float hue,
			float expectedR,
			float expectedG,
			float expectedB
		)
		{
			var color = ColorHelper.HslToRGB(hue, 100f, 50f);

			Assert.That(color.r, Is.EqualTo(expectedR).Within(0.0001f));
			Assert.That(color.g, Is.EqualTo(expectedG).Within(0.0001f));
			Assert.That(color.b, Is.EqualTo(expectedB).Within(0.0001f));
		}

		[Test]
		public void HslToRgb_WhenSaturationIsZero_ReturnsGray()
		{
			var color = ColorHelper.HslToRGB(90f, 0f, 25f);

			Assert.That(color.r, Is.EqualTo(0.25f).Within(0.0001f));
			Assert.That(color.g, Is.EqualTo(0.25f).Within(0.0001f));
			Assert.That(color.b, Is.EqualTo(0.25f).Within(0.0001f));
		}

		[Test]
		public void HslToRgb_WhenValuesAreOutOfRange_ClampsBeforeConversion()
		{
			var color = ColorHelper.HslToRGB(720f, 150f, 50f);

			Assert.That(color.r, Is.EqualTo(1f).Within(0.0001f));
			Assert.That(color.g, Is.EqualTo(0f).Within(0.0001f));
			Assert.That(color.b, Is.EqualTo(0f).Within(0.0001f));
		}
	}
}
