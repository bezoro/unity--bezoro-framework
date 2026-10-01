using Bezoro.Unity.Core.Common.Primitives;
using NUnit.Framework;
using UnityEngine;

namespace Bezoro.Unity.Core.Tests.EditMode
{
	/// <summary>
	///     Verifies transform data snapshots capture world pose, local scale, and parent.
	/// </summary>
	[TestFixture]
	public sealed class TransformDataTests
	{
		private GameObject _child;
		private GameObject _parent;

		[SetUp]
		public void SetUp()
		{
			_parent = new("TransformDataTestsParent");
			_child  = new("TransformDataTestsChild");
		}

		[TearDown]
		public void TearDown()
		{
			if (_child != null)
				Object.DestroyImmediate(_child);

			if (_parent != null)
				Object.DestroyImmediate(_parent);
		}

		[Test]
		public void Constructor_AfterTransformChanges_KeepsSnapshotValues()
		{
			_child.transform.position   = Vector3.one;
			_child.transform.rotation   = Quaternion.Euler(0f, 90f, 0f);
			_child.transform.localScale = new(2f, 2f, 2f);

			var data = new TransformData(_child.transform);

			_child.transform.position   = Vector3.zero;
			_child.transform.rotation   = Quaternion.identity;
			_child.transform.localScale = Vector3.one;

			Assert.AreEqual(Vector3.one, data.Position);
			Assert.That(Quaternion.Angle(Quaternion.Euler(0f, 90f, 0f), data.Rotation), Is.LessThan(0.001f));
			Assert.AreEqual(new Vector3(2f, 2f, 2f), data.Scale);
		}

		[Test]
		public void Constructor_CapturesCurrentTransformValues()
		{
			_parent.transform.position = new(10f, 0f, -2f);
			_child.transform.SetParent(_parent.transform, false);
			_child.transform.localPosition = new(1f, 2f, 3f);
			_child.transform.localRotation = Quaternion.Euler(15f, 30f, 45f);
			_child.transform.localScale    = new(2f, 3f, 4f);

			var data = new TransformData(_child.transform);

			Assert.AreEqual(_child.transform.position,   data.Position);
			Assert.AreEqual(_child.transform.rotation,   data.Rotation);
			Assert.AreEqual(_child.transform.localScale, data.Scale);
			Assert.AreSame(_parent.transform, data.Parent);
		}
	}
}
