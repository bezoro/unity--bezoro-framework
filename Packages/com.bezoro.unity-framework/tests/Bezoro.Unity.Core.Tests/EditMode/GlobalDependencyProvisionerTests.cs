using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Bezoro.Unity.Core.Dependencies;
using Bezoro.Unity.Core.Editor.Dependencies;
using Bezoro.Unity.Core.MonoBehaviours;
using Bezoro.Unity.Core.Scenes;
using Bezoro.Unity.Core.ScriptableObjects;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;

namespace Bezoro.Unity.Core.Tests.EditMode
{
	[TestFixture]
	public sealed class GlobalDependencyProvisionerTests
	{
		private const string TEST_FOLDER = "Assets/__GlobalDependencyProvisionerTests";

		private readonly List<Scene> _scenes = new();

		[SetUp]
		public void SetUp()
		{
			if (!AssetDatabase.IsValidFolder(TEST_FOLDER))
				AssetDatabase.CreateFolder("Assets", "__GlobalDependencyProvisionerTests");

			Undo.ClearAll();
		}

		[TearDown]
		public void TearDown()
		{
			EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
			_scenes.Clear();
			Undo.ClearAll();
			AssetDatabase.DeleteAsset(TEST_FOLDER);
			AssetDatabase.Refresh();
		}

		[Test]
		public void ReconcileScene_WithCollectionRequirement_SyncsEntryAndCreatesVisiblePrefabProvider()
		{
			var scene      = CreateScene("CollectionRequirement");
			var definition = CreateDefinition("Audio", GlobalDependencyOrphanPolicy.Keep);
			var collection = CreateCollection(scene);
			CreateRequirement(scene, definition);

			var result = GlobalDependencyProvisioner.ReconcileScene(
				scene,
				new[] { collection },
				null
			);

			Assert.IsFalse(result.HasErrors, string.Join("\n", result.Diagnostics.Select(item => item.Message)));
			CollectionAssert.Contains(collection.Entries[0].Dependencies, definition);

			var services = scene.GetRootGameObjects().Single(root => root.name == "[Services]");
			var provider = services.GetComponentInChildren<AudioManager>(true);
			Assert.IsNotNull(provider);
			Assert.IsTrue(PrefabUtility.IsPartOfPrefabInstance(provider));
			Assert.AreSame(
				definition,
				provider.GetComponent<GeneratedGlobalDependency>().Definition
			);

			Undo.PerformUndo();

			CollectionAssert.DoesNotContain(collection.Entries[0].Dependencies, definition);
			Assert.IsFalse(scene.GetRootGameObjects().Any(root => root.name == "[Services]"));
		}

		[Test]
		public void ReconcileScene_WithCompatibleManualProvider_ReusesWithoutMovingOrMarkingIt()
		{
			var scene      = CreateScene("ManualProvider");
			var definition = CreateDefinition("Audio", GlobalDependencyOrphanPolicy.Keep);
			CreateRequirement(scene, definition);

			var manualProvider = (GameObject)PrefabUtility.InstantiatePrefab(
				definition.ProviderPrefab!.gameObject,
				scene
			);

			var result = GlobalDependencyProvisioner.ReconcileScene(
				scene,
				Array.Empty<SceneCollection>(),
				null
			);

			Assert.IsFalse(result.HasErrors);
			Assert.AreEqual(1, FindProviders<AudioManager>(scene).Count);
			Assert.IsNull(manualProvider.transform.parent);
			Assert.IsNull(manualProvider.GetComponent<GeneratedGlobalDependency>());
		}

		[TestCase(false)]
		[TestCase(true)]
		public void ReconcileScene_WithInactiveManualProvider_ReportsIncompatibility(bool parentIsInactive)
		{
			var scene      = CreateScene("InactiveManualProvider");
			var definition = CreateDefinition("Audio", GlobalDependencyOrphanPolicy.Keep);
			CreateRequirement(scene, definition);

			var manualProvider = (GameObject)PrefabUtility.InstantiatePrefab(
				definition.ProviderPrefab!.gameObject,
				scene
			);
			SetProviderInactive(manualProvider, scene, parentIsInactive);

			var result = GlobalDependencyProvisioner.ReconcileScene(
				scene,
				Array.Empty<SceneCollection>(),
				null
			);

			Assert.IsTrue(result.HasErrors);
			Assert.AreEqual(1, FindProviders<AudioManager>(scene).Count);
			Assert.IsNull(manualProvider.GetComponent<GeneratedGlobalDependency>());
		}

		[Test]
		public void UndoingReconciliation_SuppressesPendingAndHierarchyTriggeredReconciliation()
		{
			RuntimeHelpers.RunClassConstructor(typeof(GlobalDependencyReconciler).TypeHandle);
			ResetReconcilerState();
			try
			{
				var scene      = CreateScene("UndoReconciliation");
				var definition = CreateDefinition("Audio", GlobalDependencyOrphanPolicy.Keep);
				CreateRequirement(scene, definition);
				GlobalDependencyProvisioner.ReconcileScene(
					scene,
					Array.Empty<SceneCollection>(),
					null
				);
				Assert.AreEqual(1, FindProviders<AudioManager>(scene).Count);

				InvokeReconcilerMethod("Schedule");
				Assert.IsTrue(GetReconcilerFlag("_isScheduled"));
				var capturedCallback = (EditorApplication.CallbackFunction)Delegate.CreateDelegate(
					typeof(EditorApplication.CallbackFunction),
					typeof(GlobalDependencyReconciler).GetMethod("RunScheduled", BindingFlags.NonPublic | BindingFlags.Static)!
				);

				Undo.PerformUndo();
				Assert.IsEmpty(FindProviders<AudioManager>(scene));
				Assert.IsFalse(GetReconcilerFlag("_isScheduled"));
				Assert.IsTrue(GetReconcilerFlag("_isUndoRedoInProgress"));
				capturedCallback();
				Assert.IsEmpty(FindProviders<AudioManager>(scene));

				InvokeReconcilerMethod("Schedule");
				Assert.IsFalse(GetReconcilerFlag("_isScheduled"));

				InvokeReconcilerMethod("EndUndoRedoSuppression");
				InvokeReconcilerMethod("Schedule");
				Assert.IsTrue(GetReconcilerFlag("_isScheduled"));
			}
			finally
			{
				ResetReconcilerState();
			}
		}

		[TestCase(false, false, false, true)]
		[TestCase(true, false, false, false)]
		[TestCase(false, true, false, false)]
		[TestCase(false, false, true, false)]
		public void CanMutateScene_BlocksEveryPlayModeState(
			bool applicationIsPlaying,
			bool editorIsPlaying,
			bool editorIsPlayingOrWillChangePlaymode,
			bool expected)
		{
			Assert.AreEqual(
				expected,
				GlobalDependencyReconcilerModel.CanMutateScene(
					applicationIsPlaying,
					editorIsPlaying,
					editorIsPlayingOrWillChangePlaymode
				)
			);
		}

		[TestCase(GlobalDependencyOrphanPolicy.Keep, false)]
		[TestCase(GlobalDependencyOrphanPolicy.RemoveGeneratedWhenUnused, true)]
		public void ReconcileScene_WhenStandaloneRequirementIsRemoved_AppliesOrphanPolicy(
			GlobalDependencyOrphanPolicy orphanPolicy,
			bool                         providerIsRemoved)
		{
			var scene       = CreateScene($"Orphan{orphanPolicy}");
			var definition  = CreateDefinition("Audio", orphanPolicy);
			var requirement = CreateRequirement(scene, definition);

			GlobalDependencyProvisioner.ReconcileScene(
				scene,
				Array.Empty<SceneCollection>(),
				null
			);
			Object.DestroyImmediate(requirement.gameObject);

			GlobalDependencyProvisioner.ReconcileScene(
				scene,
				Array.Empty<SceneCollection>(),
				null
			);

			Assert.AreEqual(providerIsRemoved ? 0 : 1, FindProviders<AudioManager>(scene).Count);
		}

		[Test]
		public void ReconcileScene_WhenGeneratedOrphanIsRemoved_PreservesExistingServicesRoot()
		{
			var scene       = CreateScene("ExistingServicesRoot");
			var services    = new GameObject("[Services]");
			var definition  = CreateDefinition("Audio", GlobalDependencyOrphanPolicy.RemoveGeneratedWhenUnused);
			var requirement = CreateRequirement(scene, definition);
			UnitySceneManager.MoveGameObjectToScene(services, scene);

			GlobalDependencyProvisioner.ReconcileScene(
				scene,
				Array.Empty<SceneCollection>(),
				null
			);
			Object.DestroyImmediate(requirement.gameObject);

			GlobalDependencyProvisioner.ReconcileScene(
				scene,
				Array.Empty<SceneCollection>(),
				null
			);

			Assert.IsTrue(services != null);
			Assert.AreEqual(0, services.transform.childCount);
		}

		[Test]
		public void ReconcileScene_WithUnresolvedCollectionEntry_DoesNotRemoveGeneratedProvider()
		{
			var scene       = CreateScene("UnresolvedCollectionEntry");
			var definition  = CreateDefinition("Audio", GlobalDependencyOrphanPolicy.RemoveGeneratedWhenUnused);
			var requirement = CreateRequirement(scene, definition);
			var collection = CreateCollection(
				"UnresolvedCollection",
				new SceneCollectionEntry(new SceneReference(scene.path)),
				new SceneCollectionEntry(new SceneReference(string.Empty))
			);

			GlobalDependencyProvisioner.ReconcileScene(
				scene,
				new[] { collection },
				null
			);
			Object.DestroyImmediate(requirement.gameObject);

			GlobalDependencyProvisioner.ReconcileScene(
				scene,
				new[] { collection },
				null
			);

			Assert.AreEqual(1, FindProviders<AudioManager>(scene).Count);
			CollectionAssert.Contains(collection.Entries[0].Dependencies, definition);
		}

		[Test]
		public void ReconcileScene_WithApplicationRequirementAndNoBootstrapConfig_ReportsErrorWithoutProvisioning()
		{
			var scene      = CreateScene("MissingApplicationBootstrap");
			var definition = CreateDefinition("Audio", GlobalDependencyOrphanPolicy.Keep, SingletonLifetime.Application);
			CreateRequirement(scene, definition);

			var result = GlobalDependencyProvisioner.ReconcileScene(
				scene,
				Array.Empty<SceneCollection>(),
				null
			);

			Assert.IsTrue(result.HasErrors);
			StringAssert.Contains("GameInitializationConfig", result.Diagnostics.Single().Message);
			Assert.IsEmpty(FindProviders<AudioManager>(scene));
		}

		[Test]
		public void ReconcileScene_WithConflictingDefinitionsForProviderType_ReportsError()
		{
			var scene      = CreateScene("ConflictingDefinitions");
			var first      = CreateDefinition("FirstAudio", GlobalDependencyOrphanPolicy.Keep);
			var second     = CreateDefinition("SecondAudio", GlobalDependencyOrphanPolicy.Keep);
			var collection = CreateCollection(scene, first, second);

			var result = GlobalDependencyProvisioner.ReconcileScene(
				scene,
				new[] { collection },
				null
			);

			Assert.IsTrue(result.HasErrors);
			Assert.IsTrue(
				result.Diagnostics.Any(
					item => item.Message.Contains("conflicting", StringComparison.OrdinalIgnoreCase)
				)
			);
		}

		[Test]
		public void ReconcileScene_WithRequirementConflictingWithAnotherEntry_ReportsBeforeMutation()
		{
			var scene      = CreateScene("CrossEntryConflict");
			var configured = CreateDefinition("ConfiguredAudio", GlobalDependencyOrphanPolicy.Keep);
			var required   = CreateDefinition("RequiredAudio", GlobalDependencyOrphanPolicy.Keep);
			var collection = CreateCollection(
				"CrossEntryConflict",
				new SceneCollectionEntry(new SceneReference(scene.path)),
				new SceneCollectionEntry(
					new SceneReference("Assets/Scenes/Other.unity"),
					configured
				)
			);
			CreateRequirement(scene, required);

			var result = GlobalDependencyProvisioner.ReconcileScene(
				scene,
				new[] { collection },
				null
			);

			Assert.IsTrue(result.HasErrors);
			Assert.IsEmpty(collection.Entries[0].Dependencies);
			Assert.IsEmpty(FindProviders<AudioManager>(scene));
		}

		[Test]
		public void ReconcileScene_WithProviderConfiguredInEarlierEntry_ReusesCollectionProvider()
		{
			var scene      = CreateScene("SharedCollectionProvider");
			var definition = CreateDefinition("SharedAudio", GlobalDependencyOrphanPolicy.Keep);
			var collection = CreateCollection(
				"SharedCollectionProvider",
				new SceneCollectionEntry(
					new SceneReference("Assets/Scenes/Provider.unity"),
					definition
				),
				new SceneCollectionEntry(new SceneReference(scene.path))
			);
			CreateRequirement(scene, definition);

			var result = GlobalDependencyProvisioner.ReconcileScene(
				scene,
				new[] { collection },
				null
			);

			Assert.IsFalse(result.HasErrors, string.Join("\n", result.Diagnostics.Select(item => item.Message)));
			Assert.IsEmpty(collection.Entries[1].Dependencies);
			Assert.IsEmpty(FindProviders<AudioManager>(scene));
		}

		[Test]
		public void BuildValidation_WithProviderEntryAfterDependentScene_ReportsUnsafeOrdering()
		{
			var definition = CreateDefinition("OrderedAudio", GlobalDependencyOrphanPolicy.Keep);
			string definitionPath = AssetDatabase.GetAssetPath(definition);

			var dependentScene = CreateScene("Dependent");
			string dependentPath = dependentScene.path;
			definition = AssetDatabase.LoadAssetAtPath<GlobalDependencyDefinition>(definitionPath);
			CreateRequirement(dependentScene, definition);
			EditorSceneManager.SaveScene(dependentScene);

			var providerScene = CreateScene("Provider");
			string providerPath = providerScene.path;
			definition = AssetDatabase.LoadAssetAtPath<GlobalDependencyDefinition>(definitionPath);
			PrefabUtility.InstantiatePrefab(definition.ProviderPrefab!.gameObject, providerScene);
			EditorSceneManager.SaveScene(providerScene);

			var collection = CreateCollection(
				"UnsafeOrdering",
				new SceneCollectionEntry(new SceneReference(dependentPath)),
				new SceneCollectionEntry(new SceneReference(providerPath), definition)
			);

			var result = GlobalDependencyBuildValidator.ValidateProject(
				null,
				new[] { collection },
				new[] { dependentPath, providerPath }
			);

			Assert.IsTrue(result.HasErrors);
			Assert.IsTrue(
				result.Diagnostics.Any(item => item.Message.Contains("before")),
				string.Join("\n", result.Diagnostics.Select(item => item.Message))
			);
		}

		[Test]
		public void BuildValidation_WithProviderEntryBeforeDependentScene_AcceptsOrdering()
		{
			var definition = CreateDefinition("OrderedAudio", GlobalDependencyOrphanPolicy.Keep);
			string definitionPath = AssetDatabase.GetAssetPath(definition);

			var providerScene = CreateScene("Provider");
			string providerPath = providerScene.path;
			definition = AssetDatabase.LoadAssetAtPath<GlobalDependencyDefinition>(definitionPath);
			PrefabUtility.InstantiatePrefab(definition.ProviderPrefab!.gameObject, providerScene);
			EditorSceneManager.SaveScene(providerScene);

			var dependentScene = CreateScene("Dependent");
			string dependentPath = dependentScene.path;
			definition = AssetDatabase.LoadAssetAtPath<GlobalDependencyDefinition>(definitionPath);
			CreateRequirement(dependentScene, definition);
			EditorSceneManager.SaveScene(dependentScene);

			var collection = CreateCollection(
				"SafeOrdering",
				new SceneCollectionEntry(new SceneReference(providerPath), definition),
				new SceneCollectionEntry(new SceneReference(dependentPath))
			);

			var result = GlobalDependencyBuildValidator.ValidateProject(
				null,
				new[] { collection },
				new[] { providerPath, dependentPath }
			);

			Assert.IsFalse(result.HasErrors, string.Join("\n", result.Diagnostics.Select(item => item.Message)));
		}

		[Test]
		public void BuildValidation_WithStandaloneRequirementAndNoProvider_ReportsError()
		{
			var definition = CreateDefinition("StandaloneAudio", GlobalDependencyOrphanPolicy.Keep);
			string definitionPath = AssetDatabase.GetAssetPath(definition);
			var scene      = CreateScene("Standalone");
			definition = AssetDatabase.LoadAssetAtPath<GlobalDependencyDefinition>(definitionPath);
			CreateRequirement(scene, definition);
			EditorSceneManager.SaveScene(scene);
			Assert.AreEqual(
				1,
				scene.GetRootGameObjects()
					 .SelectMany(root => root.GetComponentsInChildren<GlobalDependencyRequirement>(true))
					 .Count()
			);

			var result = GlobalDependencyBuildValidator.ValidateProject(
				null,
				Array.Empty<SceneCollection>(),
				new[] { scene.path }
			);

			Assert.IsTrue(result.HasErrors);
			Assert.IsTrue(
				result.Diagnostics.Any(
					item => item.Message.Contains("provider", StringComparison.OrdinalIgnoreCase)
				),
				string.Join("\n", result.Diagnostics.Select(item => item.Message))
			);
		}

		[Test]
		public void BuildValidation_WithDuplicateStandaloneProviders_ReportsError()
		{
			var definition = CreateDefinition("DuplicateStandaloneAudio", GlobalDependencyOrphanPolicy.Keep);
			string definitionPath = AssetDatabase.GetAssetPath(definition);
			var scene = CreateScene("DuplicateStandalone");
			definition = AssetDatabase.LoadAssetAtPath<GlobalDependencyDefinition>(definitionPath);
			CreateRequirement(scene, definition);
			PrefabUtility.InstantiatePrefab(definition.ProviderPrefab!.gameObject, scene);
			PrefabUtility.InstantiatePrefab(definition.ProviderPrefab.gameObject, scene);
			EditorSceneManager.SaveScene(scene);
			Assert.AreEqual(
				2,
				scene.GetRootGameObjects()
					 .SelectMany(root => root.GetComponentsInChildren<GlobalReference>(true))
					 .Count(reference => reference.Target is AudioManager)
			);

			var result = GlobalDependencyBuildValidator.ValidateProject(
				null,
				Array.Empty<SceneCollection>(),
				new[] { scene.path }
			);

			Assert.IsTrue(result.HasErrors);
			Assert.IsTrue(
				result.Diagnostics.Any(
					item => item.Message.Contains("duplicate", StringComparison.OrdinalIgnoreCase)
				),
				string.Join("\n", result.Diagnostics.Select(item => item.Message))
			);
		}

		[TestCase(false)]
		[TestCase(true)]
		public void BuildValidation_WithInactiveStandaloneProvider_ReportsError(bool parentIsInactive)
		{
			var definition = CreateDefinition("InactiveStandaloneAudio", GlobalDependencyOrphanPolicy.Keep);
			string definitionPath = AssetDatabase.GetAssetPath(definition);
			var scene = CreateScene("InactiveStandalone");
			definition = AssetDatabase.LoadAssetAtPath<GlobalDependencyDefinition>(definitionPath);
			CreateRequirement(scene, definition);
			var provider = (GameObject)PrefabUtility.InstantiatePrefab(
				definition.ProviderPrefab!.gameObject,
				scene
			);
			SetProviderInactive(provider, scene, parentIsInactive);
			EditorSceneManager.SaveScene(scene);

			var result = GlobalDependencyBuildValidator.ValidateProject(
				null,
				Array.Empty<SceneCollection>(),
				new[] { scene.path }
			);

			Assert.IsTrue(result.HasErrors);
			Assert.IsTrue(
				result.Diagnostics.Any(item => item.Message.Contains("compatible", StringComparison.OrdinalIgnoreCase))
			);
		}

		private SceneCollection CreateCollection(
			Scene scene,
			params GlobalDependencyDefinition[] dependencies)
		{
			return CreateCollection(
				scene.name,
				new SceneCollectionEntry(new SceneReference(scene.path), dependencies)
			);
		}

		private SceneCollection CreateCollection(
			string name,
			params SceneCollectionEntry[] entries)
		{
			var collection = ScriptableObject.CreateInstance<SceneCollection>();
			collection.SetEntries(entries);
			AssetDatabase.CreateAsset(
				collection,
				$"{TEST_FOLDER}/SO_{name}_Collection.asset"
			);
			EditorUtility.SetDirty(collection);
			AssetDatabase.SaveAssets();
			return collection;
		}

		private GlobalDependencyDefinition CreateDefinition(
			string                       name,
			GlobalDependencyOrphanPolicy orphanPolicy,
			SingletonLifetime            lifetime = SingletonLifetime.Scene)
		{
			var source = new GameObject($"PFB_MGR_{name}");
			var audio  = source.AddComponent<AudioManager>();
			var scope  = source.GetComponent<SingletonScope>();
			var global = source.GetComponent<GlobalReference>();

			var scopeObject = new SerializedObject(scope);
			scopeObject.FindProperty("identity").objectReferenceValue = audio;
			scopeObject.FindProperty("lifetime").enumValueIndex       = (int)lifetime;
			scopeObject.ApplyModifiedPropertiesWithoutUndo();

			var referenceObject = new SerializedObject(global);
			referenceObject.FindProperty("target").objectReferenceValue = audio;
			referenceObject.ApplyModifiedPropertiesWithoutUndo();

			string prefabPath = $"{TEST_FOLDER}/PFB_MGR_{name}.prefab";
			PrefabUtility.SaveAsPrefabAsset(source, prefabPath);
			Object.DestroyImmediate(source);
			AssetDatabase.ImportAsset(
				prefabPath,
				ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate
			);

			var prefabProvider = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath)
											  .GetComponent<AudioManager>();
			var definition = ScriptableObject.CreateInstance<GlobalDependencyDefinition>();
			definition.Configure(prefabProvider, orphanPolicy);
			string definitionPath = $"{TEST_FOLDER}/SO_DEP_{name}.asset";
			AssetDatabase.CreateAsset(definition, definitionPath);
			EditorUtility.SetDirty(definition);
			AssetDatabase.SaveAssets();
			return AssetDatabase.LoadAssetAtPath<GlobalDependencyDefinition>(definitionPath);
		}

		private GlobalDependencyRequirement CreateRequirement(
			Scene                      scene,
			GlobalDependencyDefinition definition)
		{
			var owner = new GameObject("Requirement");
			UnitySceneManager.MoveGameObjectToScene(owner, scene);
			var requirement = owner.AddComponent<GlobalDependencyRequirement>();
			requirement.Configure(definition);
			return requirement;
		}

		private Scene CreateScene(string name)
		{
			var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
			EditorSceneManager.SaveScene(scene, $"{TEST_FOLDER}/SCN_{name}.unity");
			_scenes.Add(scene);
			return scene;
		}

		private static List<TProvider> FindProviders<TProvider>(Scene scene)
			where TProvider : Component
		{
			var providers = new List<TProvider>();
			foreach (var root in scene.GetRootGameObjects())
				providers.AddRange(root.GetComponentsInChildren<TProvider>(true));

			return providers;
		}

		private static bool GetReconcilerFlag(string name) =>
			(bool)typeof(GlobalDependencyReconciler)
				.GetField(name, BindingFlags.NonPublic | BindingFlags.Static)!
				.GetValue(null)!;

		private static void InvokeReconcilerMethod(string name) =>
			typeof(GlobalDependencyReconciler)
				.GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static)!
				.Invoke(null, null);

		private static void ResetReconcilerState()
		{
			InvokeReconcilerMethod("CancelScheduled");
			InvokeReconcilerMethod("EndUndoRedoSuppression");
		}

		private static void SetProviderInactive(GameObject provider, Scene scene, bool parentIsInactive)
		{
			if (!parentIsInactive)
			{
				provider.SetActive(false);
				return;
			}

			var parent = new GameObject("Inactive Provider Parent");
			UnitySceneManager.MoveGameObjectToScene(parent, scene);
			provider.transform.SetParent(parent.transform);
			parent.SetActive(false);
		}
	}
}
