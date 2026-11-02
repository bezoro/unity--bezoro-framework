using System.Linq;
using Bezoro.Unity.Core.Common.Extensions;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Bezoro.Unity.Core.Tests.EditMode
{
	/// <summary>
	///     Verifies synchronous GameObject hierarchy extension helpers.
	/// </summary>
	[TestFixture]
	public sealed class GameObjectExtensionsTests
	{
		private GameObject _child;
		private GameObject _root;

		[TearDown]
		public void TearDown()
		{
			if (_root != null)
				Object.DestroyImmediate(_root);

			if (_child != null)
				Object.DestroyImmediate(_child);
		}

		[Test]
		public void ActivateHierarchy_AppliesActiveStateToChildren()
		{
			_root  = new("HierarchyRoot");
			_child = new("HierarchyChild");
			_child.transform.SetParent(_root.transform);

			_root.ActivateHierarchy(false);

			Assert.IsFalse(_root.activeSelf);
			Assert.IsFalse(_child.activeSelf);
		}

		[Test]
		public void AddComponents_AddsRequestedComponentsAndDisablesBehaviours()
		{
			_root = new("AddComponentsTestsObject");

			var components = _root.AddComponents(new[] { typeof(BoxCollider), typeof(TestBehaviour) });

			Assert.AreEqual(2, components.Count);
			Assert.IsNotNull(_root.GetComponent<BoxCollider>());
			Assert.IsFalse(_root.GetComponent<TestBehaviour>().enabled);
		}

		[Test]
		public void GetHierarchyComponents_ReturnsRootAndChildTransforms()
		{
			_root  = new("HierarchyComponentsRoot", typeof(BoxCollider));
			_child = new("HierarchyComponentsChild", typeof(SphereCollider));
			_child.transform.SetParent(_root.transform);

			var transforms = _root.GetHierarchyComponents().Select(item => item.transform).ToArray();

			CollectionAssert.Contains(transforms, _root.transform);
			CollectionAssert.Contains(transforms, _child.transform);
		}

		[Test]
		public void GetOrAddComponent_ReturnsExistingComponent()
		{
			_root = new("GetOrAddTestsObject");
			var existing = _root.AddComponent<BoxCollider>();

			var component = _root.GetOrAddComponent<BoxCollider>();

			Assert.AreSame(existing, component);
			Assert.AreEqual(1, _root.GetComponents<BoxCollider>().Length);
		}

		[Test]
		public void HasComponent_ReturnsTrueOnlyWhenComponentExists()
		{
			_root = new("HasComponentTestsObject", typeof(BoxCollider));

			Assert.IsTrue(_root.HasComponent<BoxCollider>());
			Assert.IsFalse(_root.HasComponent<Rigidbody>());
		}

		[Test]
		public void SetParent_WithResetTransformState_ParentsAndResetsLocalTransform()
		{
			_root                          = new("NewParent");
			_child                         = new("ChildToParent");
			_child.transform.localPosition = Vector3.one;
			_child.transform.localScale    = Vector3.one * 3f;

			_child.SetParent(_root, TransformAdjustmentMethod.ResetTransformState);

			Assert.AreSame(_root.transform, _child.transform.parent);
			Assert.AreEqual(Vector3.zero, _child.transform.localPosition);
			Assert.AreEqual(Vector3.one,  _child.transform.localScale);
		}

		[Test]
		public void TryGetComponentInParents_FindsParentComponent()
		{
			_root  = new("ParentObject", typeof(BoxCollider));
			_child = new("ChildObject");
			_child.transform.SetParent(_root.transform);

			bool found = _child.TryGetComponentInParents<BoxCollider>(out var component);

			Assert.IsTrue(found);
			Assert.AreSame(_root.GetComponent<BoxCollider>(), component);
		}

		private sealed class TestBehaviour : MonoBehaviour { }
	}
}
