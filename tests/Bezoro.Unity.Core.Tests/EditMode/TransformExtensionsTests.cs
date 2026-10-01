using Bezoro.Unity.Core.Common.Extensions;
using NUnit.Framework;
using UnityEngine;

namespace Bezoro.Unity.Core.Tests.EditMode
{
	/// <summary>
	///     Verifies Transform extension helpers that are deterministic in edit mode.
	/// </summary>
	[TestFixture]
	public sealed class TransformExtensionsTests
	{
		private GameObject _gameObject;

		[TearDown]
		public void TearDown()
		{
			if (_gameObject != null)
				Object.DestroyImmediate(_gameObject);
		}

		[Test]
		public void CalculateBoundsRecursive_WhenNoRenderers_ReturnsEmptyBounds()
		{
			_gameObject                    = new("EmptyBoundsTestsObject");
			_gameObject.transform.position = new(4f, 5f, 6f);

			var bounds = _gameObject.transform.CalculateBoundsRecursive();

			Assert.AreEqual(Vector3.zero, bounds.center);
			Assert.AreEqual(Vector3.zero, bounds.size);
		}

		[Test]
		public void ResetAll_RestoresLocalIdentity()
		{
			_gameObject                         = new("TransformExtensionsTestsObject");
			_gameObject.transform.localPosition = new(1f, 2f, 3f);
			_gameObject.transform.localRotation = Quaternion.Euler(10f, 20f, 30f);
			_gameObject.transform.localScale    = new(2f, 3f, 4f);

			_gameObject.transform.ResetAll();

			Assert.AreEqual(Vector3.zero,        _gameObject.transform.localPosition);
			Assert.AreEqual(Quaternion.identity, _gameObject.transform.localRotation);
			Assert.AreEqual(Vector3.one,         _gameObject.transform.localScale);
		}

		[Test]
		public void TransformBounds_TransformsCenterAndScaledExtents()
		{
			_gameObject                      = new("TransformBoundsTestsObject");
			_gameObject.transform.position   = new(10f, 0f, -2f);
			_gameObject.transform.localScale = new(2f, 3f, 4f);
			var localBounds = new Bounds(new(1f, 1f, 1f), new(2f, 2f, 2f));

			var bounds = _gameObject.transform.TransformBounds(localBounds);

			Assert.AreEqual(new Vector3(12f, 3f, 2f), bounds.center);
			Assert.AreEqual(new Vector3(2f,  3f, 4f), bounds.extents);
		}
	}
}
