using Bezoro.Unity.Core.Common.Extensions;
using NUnit.Framework;
using UnityEngine;

namespace Bezoro.Unity.Core.Tests.EditMode
{
	/// <summary>
	///     Verifies raycast extension guard behavior and simple physics hits.
	/// </summary>
	[TestFixture]
	public sealed class RaycastExtensionsTests
	{
		private GameObject _hitObject;

		[TearDown]
		public void TearDown()
		{
			if (_hitObject != null)
				Object.DestroyImmediate(_hitObject);
		}

		[Test]
		public void Raycast2D_WhenCameraOrBufferIsMissing_ReturnsFalseAndEmptyObjects()
		{
			bool hit = Vector2.zero.Raycast2D(null, new RaycastHit2D[1], out var hitObjects);

			Assert.IsFalse(hit);
			Assert.IsEmpty(hitObjects);
		}

		[Test]
		public void Raycast3D_WhenColliderIsHit_ReturnsHitObject()
		{
			_hitObject                    = GameObject.CreatePrimitive(PrimitiveType.Cube);
			_hitObject.transform.position = new(0f, 0f, 5f);
			Physics.SyncTransforms();

			bool hit = new Ray(Vector3.zero, Vector3.forward).Raycast3D(new RaycastHit[4], out var hitGameObjects);

			Assert.IsTrue(hit);
			CollectionAssert.Contains(hitGameObjects, _hitObject);
		}

		[Test]
		public void Raycast3D_WhenHitBufferIsNull_ReturnsFalseAndEmptyObjects()
		{
			bool hit = new Ray(Vector3.zero, Vector3.forward).Raycast3D(null, out var hitGameObjects);

			Assert.IsFalse(hit);
			Assert.IsEmpty(hitGameObjects);
		}
	}
}
