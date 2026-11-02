using Bezoro.Unity.Core.Common.Extensions;
using NUnit.Framework;
using UnityEngine;

namespace Bezoro.Unity.Core.Tests.EditMode
{
	/// <summary>
	///     Verifies RectTransform bounds aggregation helpers.
	/// </summary>
	[TestFixture]
	public sealed class RectTransformExtensionsTests
	{
		private GameObject _first;
		private GameObject _second;

		[TearDown]
		public void TearDown()
		{
			if (_first != null)
				Object.DestroyImmediate(_first);

			if (_second != null)
				Object.DestroyImmediate(_second);
		}

		[Test]
		public void ComputeWorldBounds_ForArray_IgnoresNullsAndEncapsulates()
		{
			var first  = CreateRectTransform(ref _first,  "FirstRect",  new(2f, 2f), Vector3.zero);
			var second = CreateRectTransform(ref _second, "SecondRect", new(2f, 2f), new(4f, 0f, 0f));

			var bounds = new[] { first, null, second }.ComputeWorldBounds();

			Assert.AreEqual(new Vector3(2f, 0f, 0f), bounds.center);
			Assert.AreEqual(new Vector3(6f, 2f, 0f), bounds.size);
		}

		[Test]
		public void ComputeWorldBounds_ForNullRectTransform_ReturnsEmptyBounds()
		{
			RectTransform rectTransform = null;

			var bounds = rectTransform.ComputeWorldBounds();

			Assert.AreEqual(Vector3.zero, bounds.center);
			Assert.AreEqual(Vector3.zero, bounds.size);
		}

		[Test]
		public void ComputeWorldBounds_ForSingleRectTransform_UsesWorldCorners()
		{
			var rectTransform = CreateRectTransform(ref _first, "SingleRect", new(10f, 20f), Vector3.zero);

			var bounds = rectTransform.ComputeWorldBounds();

			Assert.AreEqual(Vector3.zero,              bounds.center);
			Assert.AreEqual(new Vector3(10f, 20f, 0f), bounds.size);
		}

		private static RectTransform CreateRectTransform(
			ref GameObject gameObject,
			string         name,
			Vector2        size,
			Vector3        position)
		{
			gameObject = new(name, typeof(RectTransform));
			var rectTransform = gameObject.GetComponent<RectTransform>();
			rectTransform.sizeDelta = size;
			rectTransform.position  = position;
			return rectTransform;
		}
	}
}
