using Bezoro.Unity.Core.Common.Extensions;
using NUnit.Framework;
using UnityEngine;

namespace Bezoro.Unity.Core.Tests.EditMode
{
	/// <summary>
	///     Verifies component extension methods for enabled state, hierarchy lookup, and hide flags.
	/// </summary>
	[TestFixture]
	public sealed class ComponentExtensionsTests
	{
		private GameObject _root;

		[SetUp]
		public void SetUp()
		{
			_root = new("ComponentExtensionsTestsRoot");
		}

		[TearDown]
		public void TearDown()
		{
			if (_root != null)
				Object.DestroyImmediate(_root);
		}

		[Test]
		public void DisableAndEnable_WhenArrayContainsMissingComponent_IgnoresMissingEntry()
		{
			var first  = _root.AddComponent<TestBehaviour>();
			var second = _root.AddComponent<OtherTestBehaviour>();
			Object.DestroyImmediate(second);

			var components = new Component[] { first, second };

			components.Disable();
			Assert.IsFalse(first.enabled);

			components.Enable();
			Assert.IsTrue(first.enabled);
		}

		[Test]
		public void DisableAndEnable_WhenComponentIsBehaviour_TogglesEnabledState()
		{
			var behaviour = _root.AddComponent<TestBehaviour>();

			behaviour.Disable();

			Assert.IsFalse(behaviour.enabled);
			Assert.IsFalse(behaviour.IsEnabled());

			behaviour.Enable();

			Assert.IsTrue(behaviour.enabled);
			Assert.IsTrue(behaviour.IsEnabled());
		}

		[Test]
		public void SetHideFlagsRecursive_AppliesFlagsToComponentObjectAndChildren()
		{
			var child = new GameObject("ComponentExtensionsTestsChild");
			child.transform.SetParent(_root.transform);

			var behaviour = _root.AddComponent<TestBehaviour>();

			behaviour.SetHideFlagsRecursive(HideFlags.HideInHierarchy);

			Assert.AreEqual(HideFlags.HideInHierarchy, _root.hideFlags);
			Assert.AreEqual(HideFlags.HideInHierarchy, child.hideFlags);
		}

		[Test]
		public void TryGetComponentInChildren_WhenChildHasComponent_ReturnsTrue()
		{
			var child = new GameObject("ComponentExtensionsTestsChild");
			child.transform.SetParent(_root.transform);
			var childComponent = child.AddComponent<OtherTestBehaviour>();

			var rootComponent = _root.AddComponent<TestBehaviour>();

			bool found = rootComponent.TryGetComponentInChildren<OtherTestBehaviour>(out var component);

			Assert.IsTrue(found);
			Assert.AreSame(childComponent, component);
		}

		private sealed class OtherTestBehaviour : MonoBehaviour { }

		private sealed class TestBehaviour : MonoBehaviour { }
	}
}
