using System;
using Bezoro.Unity.Core.Common.Helpers;
using NUnit.Framework;
using UnityEngine;

namespace Bezoro.Unity.Core.Tests.EditMode
{
	/// <summary>
	///     Verifies shared Vector3 calculation helpers.
	/// </summary>
	[TestFixture]
	public sealed class VectorHelpersTests
	{
		[Test]
		public void AngleBetween_ReturnsUnityVectorAngle()
		{
			float angle = VectorHelpers.AngleBetween(Vector3.forward, Vector3.right);

			Assert.That(angle, Is.EqualTo(90f).Within(0.0001f));
		}

		[Test]
		public void Average_WhenNoVectorsAreProvided_ThrowsArgumentException()
		{
			Assert.Throws<ArgumentException>(() => VectorHelpers.Average());
		}

		[Test]
		public void Average_WhenVectorsAreProvided_ReturnsComponentAverage()
		{
			var average = VectorHelpers.Average(new Vector3(1f, 2f, 3f), new Vector3(3f, 4f, 5f));

			Assert.AreEqual(new Vector3(2f, 3f, 4f), average);
		}

		[Test]
		public void CalculateCellCenter_ReturnsCenterOffsetFromBoundsMinimum()
		{
			var bounds = new Bounds(Vector3.zero, new(4f, 2f, 4f));
			var center = VectorHelpers.CalculateCellCenter(1, 0, new(1f, 1f), bounds);

			Assert.AreEqual(new Vector3(-0.5f, 1f, -1.5f), center);
		}

		[Test]
		public void ComponentOperations_ReturnExpectedPerAxisResults()
		{
			var value = new Vector3(2f, 4f, 6f);

			Assert.AreEqual(new Vector3(2f, 3f,  4f),  value.ClampComponents(Vector3.one, new(2f, 3f, 4f)));
			Assert.AreEqual(new Vector3(4f, 12f, 24f), value.MultiplyComponents(new(2f, 3f, 4f)));
			Assert.AreEqual(new Vector3(1f, 2f,  3f),  value.DivideComponents(new(2f, 2f, 2f)));
		}

		[Test]
		public void Perpendicular_ReturnsCrossWithUpVector()
		{
			var perpendicular = VectorHelpers.Perpendicular(Vector3.forward);

			Assert.AreEqual(-Vector3.right, perpendicular);
		}

		[Test]
		public void RotateAroundPivot_RotatesPointAroundPivotByEulerAngles()
		{
			var point   = new Vector3(1f, 0f, 0f);
			var rotated = point.RotateAroundPivot(Vector3.zero, new(0f, 90f, 0f));

			Assert.That(rotated.x, Is.EqualTo(0f).Within(0.0001f));
			Assert.That(rotated.z, Is.EqualTo(-1f).Within(0.0001f));
		}

		[Test]
		public void ValidateVector_WhenVectorContainsNaN_ThrowsArgumentException()
		{
			Assert.Throws<ArgumentException>(() => VectorHelpers.ValidateVector2(new(float.NaN, 0f)));
			Assert.Throws<ArgumentException>(() => VectorHelpers.ValidateVector3(new(0f, float.NaN, 0f)));
		}
	}
}
