using Bezoro.Unity.Core.Common.Helpers;
using NUnit.Framework;
using Unity.Mathematics;

namespace Bezoro.Unity.Core.Tests.EditMode
{
	[TestFixture]
	public sealed class MathHelpersTests
	{
		[Test]
		public void CalculateFOV_ReturnsExpectedAngleFromWidthAndDistance()
		{
			float fov = MathHelpers.CalculateFOV(2f, 4f);

			Assert.AreEqual(29.260f, fov, 0.001f);
		}

		[Test]
		public void IsInVisionCone_WhenTargetIsBehindOutsideAngleOrTooFar_ReturnsFalse()
		{
			var origin  = float3.zero;
			var forward = new float3(0f, 0f, 1f);

			Assert.IsFalse(MathHelpers.IsInVisionCone(origin, forward, 45u, 10u, new(0f, 0f, -1f)));
			Assert.IsFalse(MathHelpers.IsInVisionCone(origin, forward, 45u, 10u, new(5f, 0f, 0f)));
			Assert.IsFalse(MathHelpers.IsInVisionCone(origin, forward, 45u, 10u, new(0f, 0f, 11f)));
		}

		[Test]
		public void IsInVisionCone_WhenTargetIsForwardWithinAngleAndDistance_ReturnsTrue()
		{
			bool result = MathHelpers.IsInVisionCone(
				float3.zero,
				new(0f, 0f, 1f),
				45u,
				10u,
				new(0f, 0f, 5f)
			);

			Assert.IsTrue(result);
		}

		[Test]
		public void RandomPointOnCircle_WhenSeedIsRepeated_ReturnsDeterministicPointAtRadius()
		{
			var origin = new float3(2f, 3f, 4f);

			var first  = MathHelpers.RandomPointOnCircle(origin, 5f, 123u);
			var second = MathHelpers.RandomPointOnCircle(origin, 5f, 123u);

			AssertFloat3(first, second);
			Assert.AreEqual(origin.y, first.y,                                                           0.0001f);
			Assert.AreEqual(25f,      math.lengthsq(new float2(first.x - origin.x, first.z - origin.z)), 0.0001f);
		}

		private static void AssertFloat3(float3 expected, float3 actual)
		{
			Assert.AreEqual(expected.x, actual.x, 0.0001f);
			Assert.AreEqual(expected.y, actual.y, 0.0001f);
			Assert.AreEqual(expected.z, actual.z, 0.0001f);
		}
	}
}
