using Bezoro.Unity.Core.Common.Extensions.Vector;
using NUnit.Framework;
using UnityEngine;

namespace Bezoro.Unity.Core.Tests.EditMode
{
	/// <summary>
	///     Verifies Vector2 angle conversion helpers.
	/// </summary>
	[TestFixture]
	public sealed class Vector2ExtensionsTests
	{
		[TestCase(0f,   0f,  1f)]
		[TestCase(90f,  -1f, 0f)]
		[TestCase(180f, 0f,  -1f)]
		[TestCase(270f, 1f,  0f)]
		public void FromAngle_PreservesMagnitudeAndReturnsExpectedDirection(
			float angle,
			float expectedX,
			float expectedY)
		{
			var result = new Vector2(0f, 2f).FromAngle(angle);

			Assert.That(result.x, Is.EqualTo(expectedX * 2f).Within(0.0001f));
			Assert.That(result.y, Is.EqualTo(expectedY * 2f).Within(0.0001f));
		}

		[Test]
		public void SmallestDeltaAngle_WhenAnglesCrossZero_ReturnsShortestSignedDelta()
		{
			float delta = Vector2.up.SmallestDeltaAngle(Vector2.right);

			Assert.That(delta, Is.EqualTo(90f).Within(0.0001f));
		}

		[TestCase(0f,  1f,  0f)]
		[TestCase(-1f, 0f,  90f)]
		[TestCase(0f,  -1f, 180f)]
		[TestCase(1f,  0f,  270f)]
		public void ToAngle_ReturnsUnityRotationAngle(float x, float y, float expectedAngle)
		{
			float angle = new Vector2(x, y).ToAngle();

			Assert.That(angle, Is.EqualTo(expectedAngle).Within(0.0001f));
		}
	}
}
