using Bezoro.Unity.Core.Common.Primitives;
using NUnit.Framework;
using UnityEngine;

namespace Bezoro.Unity.Core.Tests.EditMode
{
	public class Position3DTests
	{
		[Test]
		public void Equals_WhenComponentsDifferWithinFloatTolerance_ReturnsFalse()
		{
			var left  = new Position3D(0f,            1f, 2f);
			var right = new Position3D(float.Epsilon, 1f, 2f);

			Assert.IsFalse(left.Equals(right));
			Assert.IsTrue(left != right);
		}

		[Test]
		public void Equals_WhenComponentsMatchExactly_ReturnsTrueAndHashCodesMatch()
		{
			var left  = new Position3D(1f, 10_000f, -2f);
			var right = new Position3D(1f, 10_000f, -2f);

			Assert.IsTrue(left.Equals(right));
			Assert.IsTrue(left == right);
			Assert.AreEqual(left.GetHashCode(), right.GetHashCode());
		}

		[Test]
		public void ImplicitPosition3DConversion_FromVector3PreservesComponents()
		{
			Position3D position = new Vector3(7f, 8f, 9f);

			Assert.AreEqual(7f, position.X);
			Assert.AreEqual(8f, position.Y);
			Assert.AreEqual(9f, position.Z);
		}

		[Test]
		public void ImplicitVector3Conversion_PreservesComponents()
		{
			var     position = new Position3D(10f, 11f, 12f);
			Vector3 vector   = position;

			Assert.AreEqual(new Vector3(10f, 11f, 12f), vector);
		}
	}
}
