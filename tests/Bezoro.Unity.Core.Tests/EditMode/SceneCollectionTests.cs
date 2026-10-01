using System.Collections.Generic;
using System.Reflection;
using Bezoro.Unity.Core.Dependencies;
using Bezoro.Unity.Core.Scenes;
using Bezoro.Unity.Core.ScriptableObjects;
using NUnit.Framework;
using UnityEngine;

namespace Bezoro.Unity.Core.Tests.EditMode
{
	[TestFixture]
	public sealed class SceneCollectionTests
	{
		private readonly List<Object> _objects = new();

		[TearDown]
		public void TearDown()
		{
			foreach (var obj in _objects)
			{
				if (obj != null)
					Object.DestroyImmediate(obj);
			}

			_objects.Clear();
		}

		[Test]
		public void OnAfterDeserialize_WithLegacyScenes_MigratesEntriesWithoutChangingOrder()
		{
			var collection = CreateCollection();
			var first      = new SceneReference("Assets/Scenes/First.unity");
			var second     = new SceneReference("Assets/Scenes/Second.unity");
			SetField(collection, "scenes", new List<SceneReference> { first, second });

			((ISerializationCallbackReceiver)collection).OnAfterDeserialize();

			Assert.AreEqual(2, collection.Entries.Count);
			Assert.AreSame(first, collection.Entries[0].Scene);
			Assert.AreSame(second, collection.Entries[1].Scene);
			CollectionAssert.AreEqual(new[] { first, second }, collection.Scenes);
		}

		[Test]
		public void SetScenes_ReplacesEntriesAndScenesProjectionInOrder()
		{
			var collection = CreateCollection();
			var first      = new SceneReference("Assets/Scenes/First.unity");
			var second     = new SceneReference("Assets/Scenes/Second.unity");

			collection.SetScenes(new List<SceneReference> { first, second });

			Assert.AreEqual(2, collection.Entries.Count);
			CollectionAssert.AreEqual(new[] { first, second }, collection.Scenes);
		}

		[Test]
		public void LoadPlan_PartitionsProviderEntriesBeforeRemainingScenes()
		{
			var definition = ScriptableObject.CreateInstance<GlobalDependencyDefinition>();
			_objects.Add(definition);

			var entries = new[]
			{
				new SceneCollectionEntry(
					new SceneReference("Assets/Scenes/Gameplay.unity")
				),
				new SceneCollectionEntry(
					new SceneReference("Assets/Scenes/ProviderA.unity"),
					definition
				),
				new SceneCollectionEntry(
					new SceneReference("Assets/Scenes/UI.unity")
				),
				new SceneCollectionEntry(
					new SceneReference("Assets/Scenes/ProviderB.unity"),
					definition
				)
			};

			var plan = SceneCollectionLoadPlan.Create(entries);

			CollectionAssert.AreEqual(
				new[]
				{
					"Assets/Scenes/ProviderA.unity",
					"Assets/Scenes/ProviderB.unity"
				},
				plan.ProviderScenePaths
			);
			CollectionAssert.AreEqual(
				new[]
				{
					"Assets/Scenes/Gameplay.unity",
					"Assets/Scenes/UI.unity"
				},
				plan.RemainingScenePaths
			);
		}

		[Test]
		public void LoadPlan_WhenLaterSceneProvidesDependencies_PreservesDeclaredFirstSceneAsActiveScene()
		{
			var definition = ScriptableObject.CreateInstance<GlobalDependencyDefinition>();
			_objects.Add(definition);

			var entries = new[]
			{
				new SceneCollectionEntry(
					new SceneReference("Assets/Scenes/Gameplay.unity")
				),
				new SceneCollectionEntry(
					new SceneReference("Assets/Scenes/Provider.unity"),
					definition
				)
			};

			var plan = SceneCollectionLoadPlan.Create(entries);

			Assert.AreEqual("Assets/Scenes/Gameplay.unity", plan.ActiveScenePath);
		}

		private SceneCollection CreateCollection()
		{
			var collection = ScriptableObject.CreateInstance<SceneCollection>();
			_objects.Add(collection);
			return collection;
		}

		private static void SetField(object target, string fieldName, object value)
		{
			target
				.GetType()
				.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)!
				.SetValue(target, value);
		}
	}
}
