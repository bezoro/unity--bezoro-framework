using System;
using Bezoro.Unity.Core.AsyncLoading;
using Bezoro.Unity.Core.Common.Extensions;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

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

		[TestCase(typeof(MeshRenderer), true)]
		[TestCase(typeof(MeshRenderer), false)]
		[TestCase(typeof(BoxCollider), true)]
		[TestCase(typeof(BoxCollider), false)]
		[TestCase(typeof(BoxCollider2D), true)]
		[TestCase(typeof(BoxCollider2D), false)]
		[TestCase(typeof(LODGroup), true)]
		[TestCase(typeof(LODGroup), false)]
		public void NativeEnabledProperties_CaptureAndToggleTheActualInitialState(Type type, bool initiallyEnabled)
		{
			var component = _root.AddComponent(type);
			SetNativeEnabled(component, initiallyEnabled);

			Assert.AreEqual(initiallyEnabled, ReadNativeEnabled(component));
			Assert.AreEqual(initiallyEnabled, component.IsEnabled());
			Assert.AreEqual(initiallyEnabled, new ComponentState(component).WasInitiallyEnabled);

			component.Disable();
			Assert.IsFalse(ReadNativeEnabled(component));
			Assert.IsFalse(component.IsEnabled());

			component.Enable();
			Assert.IsTrue(ReadNativeEnabled(component));
			Assert.IsTrue(component.IsEnabled());
		}

		[Test]
		public void EnabledState_WhenUnsupportedMissingOrDestroyed_DoesNotChangeOtherState()
		{
			_root.transform.localPosition = new Vector3(1, 2, 3);
			_root.transform.Disable();
			_root.transform.Enable();
			Assert.IsFalse(_root.transform.IsEnabled());
			Assert.AreEqual(new Vector3(1, 2, 3), _root.transform.localPosition);
			Assert.IsTrue(_root.activeSelf);

			Component missing = null;
			missing.Disable();
			missing.Enable();
			Assert.IsFalse(missing.IsEnabled());

			var destroyed = _root.AddComponent<MeshRenderer>();
			Object.DestroyImmediate(destroyed);
			destroyed.Disable();
			destroyed.Enable();
			Assert.IsFalse(destroyed.IsEnabled());
		}

		private static bool ReadNativeEnabled(Component component) => component switch
		{
			Renderer renderer => renderer.enabled,
			Collider collider => collider.enabled,
			Collider2D collider => collider.enabled,
			LODGroup lod => lod.enabled,
			_ => throw new ArgumentException("This regression requires a native enabled property.")
		};

		private static void SetNativeEnabled(Component component, bool enabled)
		{
			switch (component)
			{
				case Renderer renderer: renderer.enabled = enabled; break;
				case Collider collider: collider.enabled = enabled; break;
				case Collider2D collider: collider.enabled = enabled; break;
				case LODGroup lod: lod.enabled = enabled; break;
				default: throw new ArgumentException("This regression requires a native enabled property.");
			}
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
