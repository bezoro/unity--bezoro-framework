using Bezoro.Unity.Core.Common.Primitives;
using NUnit.Framework;
using UnityColor = UnityEngine.Color;

namespace Bezoro.Unity.Core.Tests.EditMode
{
	/// <summary>
	///     Verifies color-with-intensity value conversion and equality behavior.
	/// </summary>
	[TestFixture]
	public sealed class ColorWithIntensityTests
	{
		[Test]
		public void Equals_WhenColorAndIntensityMatch_ReturnsTrueAndHashCodesMatch()
		{
			var left  = new ColorWithIntensity(UnityColor.cyan, 2.5f);
			var right = new ColorWithIntensity(UnityColor.cyan, 2.5f);

			Assert.IsTrue(left.Equals(right));
			Assert.IsTrue(left.Equals((object)right));
			Assert.AreEqual(left.GetHashCode(), right.GetHashCode());
		}

		[Test]
		public void Equals_WhenIntensityDiffers_ReturnsFalse()
		{
			var left  = new ColorWithIntensity(UnityColor.cyan, 2.5f);
			var right = new ColorWithIntensity(UnityColor.cyan);

			Assert.IsFalse(left.Equals(right));
		}

		[Test]
		public void ImplicitColorConversion_DropsIntensityAndPreservesColor()
		{
			var colorWithIntensity = new ColorWithIntensity(UnityColor.magenta, 3f);

			UnityColor color = colorWithIntensity;

			Assert.AreEqual(UnityColor.magenta, color);
		}

		[Test]
		public void ImplicitColorWithIntensityConversion_UsesDefaultIntensity()
		{
			ColorWithIntensity colorWithIntensity = UnityColor.yellow;

			Assert.AreEqual(UnityColor.yellow, colorWithIntensity.color);
			Assert.AreEqual(1f,                colorWithIntensity.intensity);
		}
	}
}
