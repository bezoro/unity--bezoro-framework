using System;
using System.Collections.Generic;
using System.Reflection;
using Bezoro.Unity.Core.Dependencies;
using Bezoro.Unity.Core.MonoBehaviours;
using Bezoro.Unity.Core.ScriptableObjects;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Bezoro.Unity.Core.Tests.EditMode
{
	[TestFixture]
	public sealed class GlobalDependencyTests
	{
		private readonly List<Object> _objects = new();

		[TearDown]
		public void TearDown()
		{
			GlobalReferenceRegistry.ResetForTests();
			SingletonScopeRegistry.ResetForTests();

			foreach (var obj in _objects)
			{
				if (obj != null)
					Object.DestroyImmediate(obj);
			}

			_objects.Clear();
		}

		[Test]
		public void Definition_WithConfiguredProvider_ExposesTypeLifetimeAndOrphanPolicy()
		{
			var definition = CreateDefinition<TestProvider>(
				"Provider",
				SingletonLifetime.SceneSession,
				GlobalDependencyOrphanPolicy.RemoveGeneratedWhenUnused
			);

			Assert.IsTrue(definition.TryValidateConfiguration(out var error), error);
			Assert.AreEqual(typeof(TestProvider), definition.ProviderType);
			Assert.AreEqual(SingletonLifetime.SceneSession, definition.Lifetime);
			Assert.AreEqual(
				GlobalDependencyOrphanPolicy.RemoveGeneratedWhenUnused,
				definition.OrphanPolicy
			);
		}

		[Test]
		public void Definition_WithoutProvider_IsInvalid()
		{
			var definition = ScriptableObject.CreateInstance<GlobalDependencyDefinition>();
			_objects.Add(definition);

			Assert.IsFalse(definition.TryValidateConfiguration(out var error));
			StringAssert.Contains("provider prefab", error);
		}

		[Test]
		public void Definition_WithInactiveProvider_IsInvalid()
		{
			var definition = CreateDefinition<TestProvider>("Provider", SingletonLifetime.Scene);
			definition.ProviderPrefabRoot!.SetActive(false);

			Assert.IsFalse(definition.TryValidateConfiguration(out var error));
			StringAssert.Contains("active", error);
		}

		[Test]
		public void Requirement_WhenConfigured_ExposesDefinitionsInOrder()
		{
			var first  = CreateDefinition<TestProvider>("First",  SingletonLifetime.Scene);
			var second = CreateDefinition<SecondTestProvider>("Second", SingletonLifetime.Scene);
			var owner  = new GameObject("Requirement");
			_objects.Add(owner);

			var requirement = owner.AddComponent<GlobalDependencyRequirement>();
			requirement.Configure(first, second);

			CollectionAssert.AreEqual(new[] { first, second }, requirement.Dependencies);
		}

		[Test]
		public void ApplicationBootstrap_WithDuplicateProviderTypes_Throws()
		{
			var first  = CreateDefinition<TestProvider>("First", SingletonLifetime.Application);
			var second = CreateDefinition<TestProvider>("Second", SingletonLifetime.Application);

			Assert.Throws<InvalidOperationException>(
				() => GlobalDependencyBootstrapper.BootstrapApplicationDependencies(
					new[] { first, second },
					_ => { }
				)
			);
		}

		[Test]
		public void ApplicationBootstrap_WithSceneLifetime_Throws()
		{
			var definition = CreateDefinition<TestProvider>("Provider", SingletonLifetime.Scene);

			Assert.Throws<InvalidOperationException>(
				() => GlobalDependencyBootstrapper.BootstrapApplicationDependencies(
					new[] { definition },
					_ => { }
				)
			);
		}

		[Test]
		public void BeforeSceneLoad_ExecutesApplicationDependenciesBeforeInitializationEntries()
		{
			var first  = CreateDefinition<TestProvider>("First", SingletonLifetime.Application);
			var second = CreateDefinition<SecondTestProvider>("Second", SingletonLifetime.Application);
			var trace  = new List<string>();
			var entries = new IInitializationObject[]
			{
				new RecordingInitializationObject(trace, "Entry")
			};

			GameInitializationConfigModel.ExecuteBeforeSceneLoad(
				new[] { first, second },
				entries,
				definition => trace.Add(definition.name)
			);

			CollectionAssert.AreEqual(new[] { "First", "Second", "Entry" }, trace);
		}

		private GlobalDependencyDefinition CreateDefinition<TProvider>(
			string                       name,
			SingletonLifetime            lifetime,
			GlobalDependencyOrphanPolicy orphanPolicy = GlobalDependencyOrphanPolicy.Keep)
			where TProvider : Component
		{
			var providerObject = new GameObject($"{name} Prefab");
			_objects.Add(providerObject);

			var provider  = providerObject.AddComponent<TProvider>();
			var scope     = providerObject.AddComponent<SingletonScope>();
			var reference = providerObject.AddComponent<GlobalReference>();
			SetField(scope,     "identity", provider);
			SetField(scope,     "lifetime", lifetime);
			SetField(reference, "target",   provider);

			var definition = ScriptableObject.CreateInstance<GlobalDependencyDefinition>();
			definition.name = name;
			definition.Configure(provider, orphanPolicy);
			_objects.Add(definition);
			return definition;
		}

		private static void SetField(object target, string fieldName, object value)
		{
			target
				.GetType()
				.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)!
				.SetValue(target, value);
		}

		private sealed class RecordingInitializationObject : IInitializationObject
		{
			private readonly string       _value;
			private readonly List<string> _trace;

			public RecordingInitializationObject(List<string> trace, string value)
			{
				_trace = trace;
				_value = value;
			}

			public void Initialize() =>
				_trace.Add(_value);
		}

		private sealed class TestProvider : MonoBehaviour { }

		private sealed class SecondTestProvider : MonoBehaviour { }
	}
}
