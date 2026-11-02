using System;
using System.Collections;
using System.Reflection;
using Bezoro.Unity.Core.MonoBehaviours;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Bezoro.Unity.Core.Tests.PlayMode
{
	/// <summary>
	///     Verifies typed global component access through <see cref="GlobalReference" />.
	/// </summary>
	[TestFixture]
	public sealed class GlobalReferencePlayModeTests
	{
		[UnityTearDown]
		public IEnumerator TearDown()
		{
			DestroyAll<TestGlobalComponent>();
			DestroyAll<TestScopedComponent>();
			yield return null;
		}

		[Test]
		public void Instance_WhenGlobalReferenceIsMissing_ThrowsInvalidOperationException()
		{
			Assert.Throws<InvalidOperationException>(() =>
														_ = GlobalReferenceAccess<TestGlobalComponent>.Instance
			);
		}

		[UnityTest]
		public IEnumerator GlobalReference_WhenActivated_RegistersTargetForTypedAccess()
		{
			var gameObject = CreateInactiveRoot("Global Reference");
			var target     = gameObject.AddComponent<TestGlobalComponent>();
			var reference  = gameObject.AddComponent<GlobalReference>();
			SetField(reference, "target", target);

			gameObject.SetActive(true);
			yield return null;

			Assert.IsTrue(GlobalReferenceAccess<TestGlobalComponent>.TryGetInstance(out var instance));
			Assert.AreSame(target, instance);
			Assert.AreSame(target, GlobalReferenceAccess<TestGlobalComponent>.Instance);
			Assert.IsTrue(GlobalReferenceAccess<TestGlobalComponent>.IsCurrent(target));
		}

		[UnityTest]
		public IEnumerator GlobalReference_WhenDuplicateScopeIsRejected_DoesNotReplaceAcceptedReference()
		{
			var first = CreateScopedGlobalReferenceRoot(
				"First Root",
				"First Scope"
			);

			var duplicate = CreateScopedGlobalReferenceRoot(
				"Duplicate Root",
				"Duplicate Scope"
			);

			first.Root.SetActive(true);
			yield return null;

			duplicate.Root.SetActive(true);
			yield return null;

			Assert.IsTrue(GlobalReferenceAccess<TestScopedComponent>.TryGetInstance(out var instance));
			Assert.AreSame(first.Target, instance);
			Assert.IsTrue(GlobalReferenceAccess<TestScopedComponent>.IsCurrent(first.Target));
			Assert.IsFalse(GlobalReferenceAccess<TestScopedComponent>.IsCurrent(duplicate.Target));
		}

		private static ScopedFixture CreateScopedGlobalReferenceRoot(
			string rootName,
			string scopeName
		)
		{
			var root = CreateInactiveRoot(rootName);

			var scopeObject = new GameObject(scopeName);
			scopeObject.transform.SetParent(root.transform);

			var target    = scopeObject.AddComponent<TestScopedComponent>();
			var scope     = scopeObject.AddComponent<SingletonScope>();
			var reference = scopeObject.AddComponent<GlobalReference>();

			SetField(scope,     "identity", target);
			SetField(reference, "target",   target);

			return new(root, target);
		}

		private static GameObject CreateInactiveRoot(string name)
		{
			var gameObject = new GameObject(name);
			gameObject.SetActive(false);
			return gameObject;
		}

		private static void DestroyAll<T>()
			where T : Component
		{
			foreach (var component in Object.FindObjectsByType<T>(FindObjectsInactive.Include))
			{
				if (component != null)
					Object.Destroy(component.gameObject);
			}
		}

		private static void SetField(object target, string fieldName, object value)
		{
			target
				.GetType()
				.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)!
				.SetValue(target, value);
		}

		private readonly struct ScopedFixture
		{
			public ScopedFixture(GameObject root, TestScopedComponent target)
			{
				Root   = root;
				Target = target;
			}

			public GameObject Root { get; }
			public TestScopedComponent Target { get; }
		}

		private sealed class TestGlobalComponent : MonoBehaviour { }

		private sealed class TestScopedComponent : MonoBehaviour { }
	}
}
