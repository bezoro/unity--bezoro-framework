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
	///     Verifies singleton hierarchy ownership through <see cref="SingletonScope" />.
	/// </summary>
	[TestFixture]
	public sealed class SingletonScopePlayModeTests
	{
		[UnityTearDown]
		public IEnumerator TearDown()
		{
			DestroyAll<TestRootMarker>();
			DestroyAll<TestScopedComponent>();
			yield return null;
		}

		[UnityTest]
		public IEnumerator GlobalAccess_WhenNoInstanceExists_DoesNotCreateProvider()
		{
			Assert.IsFalse(TestManagedSingleton.TryGetInstance(out var instance));
			yield return null;

			Assert.IsNull(instance);
			Assert.IsFalse(TestManagedSingleton.IsAvailable);
			Assert.IsEmpty(
				Object.FindObjectsByType<TestManagedSingleton>(FindObjectsInactive.Include)
			);
		}

		[UnityTest]
		public IEnumerator ScopedGlobalReference_WhenActivated_RegistersExplicitProvider()
		{
			var gameObject = CreateInactiveRoot("Explicit Provider");
			var instance   = gameObject.AddComponent<TestManagedSingleton>();
			var scope      = gameObject.AddComponent<SingletonScope>();
			var reference  = gameObject.AddComponent<GlobalReference>();
			SetField(scope,     "identity", instance);
			SetField(reference, "target",   instance);

			gameObject.SetActive(true);
			yield return null;

			Assert.IsTrue(TestManagedSingleton.TryGetInstance(out var registered));
			Assert.IsTrue(TestManagedSingleton.IsAvailable);
			Assert.AreSame(instance, registered);
			Assert.AreSame(instance, TestManagedSingleton.Instance);
		}

		[UnityTest]
		public IEnumerator SingletonScope_WhenDuplicateIdentityIsChildScope_DestroysOnlyDuplicateScopeHierarchy()
		{
			var first = CreateScopedRoot(
				"First Root",
				"First Scope"
			);

			var duplicate = CreateScopedRoot(
				"Duplicate Root",
				"Duplicate Scope"
			);

			first.Root.SetActive(true);
			yield return null;

			duplicate.Root.SetActive(true);
			yield return null;

			Assert.IsTrue(first.Root != null);
			Assert.IsTrue(first.ScopeObject != null);
			Assert.IsTrue(duplicate.Root != null);
			Assert.IsTrue(duplicate.ScopeObject == null);
		}

		[UnityTest]
		public IEnumerator SingletonScope_WithSceneLifetime_RemainsInOriginScene()
		{
			var fixture   = CreateScopedRoot("Scene Root", "Scene Scope");
			var sceneName = fixture.ScopeObject.scene.name;
			SetField(fixture.ScopeObject.GetComponent<SingletonScope>(), "lifetime", SingletonLifetime.Scene);

			fixture.Root.SetActive(true);
			yield return null;

			Assert.AreEqual(sceneName, fixture.ScopeObject.scene.name);
		}

		[UnityTest]
		public IEnumerator SingletonScope_WithApplicationLifetime_MovesRootToPersistentScene()
		{
			var fixture = CreateRootScope("Application Root");
			SetField(fixture.ScopeObject.GetComponent<SingletonScope>(), "lifetime", SingletonLifetime.Application);

			fixture.Root.SetActive(true);
			yield return null;

			Assert.AreEqual("DontDestroyOnLoad", fixture.Root.scene.name);
		}

		[UnityTest]
		public IEnumerator SingletonScope_WithApplicationLifetimeOnChild_PersistsOnlyScopedHierarchy()
		{
			var fixture = CreateScopedRoot("Scene Services", "Application Scope");
			SetField(fixture.ScopeObject.GetComponent<SingletonScope>(), "lifetime", SingletonLifetime.Application);

			fixture.Root.SetActive(true);
			yield return null;

			Assert.AreNotEqual("DontDestroyOnLoad", fixture.Root.scene.name);
			Assert.AreEqual("DontDestroyOnLoad", fixture.ScopeObject.scene.name);
			Assert.IsNull(fixture.ScopeObject.transform.parent);
		}

		[UnityTest]
		public IEnumerator SingletonScope_WithSceneSessionLifetime_DestroysRootWhenOriginSceneUnloads()
		{
			var fixture = CreateRootScope("Session Root");
			var scope   = fixture.ScopeObject.GetComponent<SingletonScope>();
			SetField(scope, "lifetime", SingletonLifetime.SceneSession);

			fixture.Root.SetActive(true);
			yield return null;

			Assert.AreEqual("DontDestroyOnLoad", fixture.Root.scene.name);

			const string originScenePath = "Assets/Tests/Origin.unity";
			SetField(scope, "_originScenePath", originScenePath);
			scope
				.GetType()
				.GetMethod("OnSceneUnloaded", BindingFlags.NonPublic | BindingFlags.Instance)!
				.Invoke(scope, new object[] { originScenePath });
			yield return null;

			Assert.IsTrue(fixture.Root == null);
		}

		private static ScopedFixture CreateScopedRoot(
			string rootName,
			string scopeName
		)
		{
			var root = CreateInactiveRoot(rootName);

			var scopeObject = new GameObject(scopeName);
			scopeObject.transform.SetParent(root.transform);

			var target = scopeObject.AddComponent<TestScopedComponent>();
			var scope  = scopeObject.AddComponent<SingletonScope>();
			SetField(scope, "identity", target);

			return new(root, scopeObject);
		}

		private static ScopedFixture CreateRootScope(string rootName)
		{
			var root = CreateInactiveRoot(rootName);

			var targetObject = new GameObject("Identity");
			targetObject.transform.SetParent(root.transform);

			var target = targetObject.AddComponent<TestScopedComponent>();
			var scope  = root.AddComponent<SingletonScope>();
			SetField(scope, "identity", target);

			return new(root, root);
		}

		private static GameObject CreateInactiveRoot(string name)
		{
			var gameObject = new GameObject(name);
			gameObject.SetActive(false);
			gameObject.AddComponent<TestRootMarker>();
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
			public ScopedFixture(GameObject root, GameObject scopeObject)
			{
				Root        = root;
				ScopeObject = scopeObject;
			}

			public GameObject Root { get; }
			public GameObject ScopeObject { get; }
		}

		private sealed class TestScopedComponent : MonoBehaviour { }

		private sealed class TestRootMarker : MonoBehaviour { }

		private sealed class TestManagedSingleton : MonoBehaviour
		{
			public static bool IsAvailable => TryGetInstance(out _);

			public static TestManagedSingleton Instance =>
				GlobalReferenceAccess<TestManagedSingleton>.Instance;

			public static bool TryGetInstance(out TestManagedSingleton instance) =>
				GlobalReferenceAccess<TestManagedSingleton>.TryGetInstance(out instance);
		}
	}
}
