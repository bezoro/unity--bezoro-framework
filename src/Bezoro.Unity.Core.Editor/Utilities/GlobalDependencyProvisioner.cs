using System;
using System.Collections.Generic;
using System.Linq;
using Bezoro.Unity.Core.Dependencies;
using Bezoro.Unity.Core.MonoBehaviours;
using Bezoro.Unity.Core.ScriptableObjects;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Bezoro.Unity.Core.Editor.Dependencies
{
	public enum GlobalDependencyDiagnosticSeverity
	{
		Warning,
		Error
	}

	public readonly struct GlobalDependencyDiagnostic
	{
		public GlobalDependencyDiagnostic(
			GlobalDependencyDiagnosticSeverity severity,
			string                             message,
			Object?                            context = null)
		{
			Severity = severity;
			Message  = message;
			Context  = context;
		}

		public Object? Context { get; }
		public string Message { get; }
		public GlobalDependencyDiagnosticSeverity Severity { get; }
	}

	public sealed class GlobalDependencyProvisioningResult
	{
		private readonly List<GlobalDependencyDiagnostic> _diagnostics = new();

		public int ChangeCount { get; internal set; }
		public IReadOnlyList<GlobalDependencyDiagnostic> Diagnostics => _diagnostics;
		public bool HasErrors => _diagnostics.Any(item => item.Severity == GlobalDependencyDiagnosticSeverity.Error);

		internal void AddError(string message, Object? context = null) =>
			_diagnostics.Add(
				new(GlobalDependencyDiagnosticSeverity.Error, message, context)
			);
	}

	public static class GlobalDependencyProvisioner
	{
		private const string SERVICES_ROOT_NAME = "[Services]";

		public static GlobalDependencyProvisioningResult ReconcileScene(
			Scene                           scene,
			IReadOnlyList<SceneCollection>  collections,
			GameInitializationConfig?       initializationConfig)
		{
			if (!scene.IsValid() || !scene.isLoaded)
				throw new ArgumentException("A valid loaded scene is required.", nameof(scene));

			if (collections == null)
				throw new ArgumentNullException(nameof(collections));

			var result = new GlobalDependencyProvisioningResult();
			if (!GlobalDependencyReconcilerModel.CanMutateScene(
					UnityEngine.Application.isPlaying,
					EditorApplication.isPlaying,
					EditorApplication.isPlayingOrWillChangePlaymode
				))
				return result;

			var matchingCollections = collections
									  .Where(collection => collection != null && ContainsScene(collection, scene.path))
									  .ToArray();

			if (matchingCollections.Length > 1)
			{
				result.AddError(
					$"Scene '{scene.path}' belongs to multiple SceneCollections.",
					matchingCollections[0]
				);
				return result;
			}

			var collection = matchingCollections.FirstOrDefault();
			var requiredDefinitions = FindRequirements(scene)
									  .SelectMany(requirement => requirement.Dependencies)
									  .Where(definition => definition != null)
									  .Distinct()
									  .ToArray();
			if (collection != null)
				ValidateCollectionDefinitions(collection, requiredDefinitions, result);

			var entry = collection != null
							? FindEntry(collection, scene.path)
							: null;
			var localRequirementDefinitions = new List<GlobalDependencyDefinition>();
			foreach (var definition in requiredDefinitions)
			{
				if (!ValidateDefinitionAsset(definition, result))
					continue;

				if (definition.Lifetime == SingletonLifetime.Application)
				{
					ValidateApplicationDependency(definition, initializationConfig, result);
					continue;
				}

				if (collection == null || entry == null)
				{
					localRequirementDefinitions.Add(definition);
					continue;
				}

				int providerEntryIndex = FindProviderEntryIndex(collection, definition.ProviderType!);
				if (providerEntryIndex < 0)
				{
					localRequirementDefinitions.Add(definition);
					continue;
				}

				int requirementEntryIndex = FindEntryIndex(collection, entry);
				if (providerEntryIndex > requirementEntryIndex)
				{
					result.AddError(
						$"Provider entry for {definition.name} must appear before scene '{scene.path}'.",
						collection
					);
					continue;
				}

				if (providerEntryIndex == requirementEntryIndex)
					localRequirementDefinitions.Add(definition);
			}

			if (result.HasErrors)
				return result;

			Undo.IncrementCurrentGroup();
			int undoGroup = Undo.GetCurrentGroup();
			Undo.SetCurrentGroupName("Reconcile Global Dependencies");

			try
			{
				RemoveUnusedManagedDependencies(collection, result);

				foreach (var definition in localRequirementDefinitions)
				{
					if (entry != null && !entry.Dependencies.Contains(definition))
					{
						Undo.RecordObject(collection, "Add Scene Global Dependency");
						entry.AddDependency(definition, true);
						EditorUtility.SetDirty(collection);
						result.ChangeCount++;
					}
				}

				var desiredDefinitions = (entry?.Dependencies ??
										  localRequirementDefinitions)
										 .Where(definition => definition != null &&
															  definition.Lifetime != SingletonLifetime.Application)
										 .Distinct()
										 .ToArray();

				ValidateDefinitionConflicts(desiredDefinitions, result, collection);
				if (!result.HasErrors)
				{
					foreach (var definition in desiredDefinitions)
					{
						if (ValidateDefinitionAsset(definition, result))
							EnsureProvider(scene, definition, result);
					}

					RemoveGeneratedOrphans(
						scene,
						collection,
						requiredDefinitions,
						result
					);
				}

				if (result.ChangeCount > 0)
					EditorSceneManager.MarkSceneDirty(scene);
			}
			finally
			{
				Undo.CollapseUndoOperations(undoGroup);
			}

			return result;
		}

		private static bool ContainsScene(SceneCollection collection, string scenePath) =>
			collection.Entries.Any(entry => entry.Scene?.Path == scenePath);

		private static SceneCollectionEntry? FindEntry(SceneCollection collection, string scenePath) =>
			collection.Entries.FirstOrDefault(entry => entry.Scene?.Path == scenePath);

		private static int FindEntryIndex(SceneCollection collection, SceneCollectionEntry entry)
		{
			for (var i = 0; i < collection.Entries.Count; i++)
			{
				if (ReferenceEquals(collection.Entries[i], entry))
					return i;
			}

			return -1;
		}

		private static int FindProviderEntryIndex(SceneCollection collection, Type providerType)
		{
			for (var i = 0; i < collection.Entries.Count; i++)
			{
				if (collection.Entries[i].Dependencies.Any(
						definition => definition != null && definition.ProviderType == providerType
					))
					return i;
			}

			return -1;
		}

		private static List<GlobalDependencyRequirement> FindRequirements(Scene scene)
		{
			var requirements = new List<GlobalDependencyRequirement>();
			foreach (var root in scene.GetRootGameObjects())
				requirements.AddRange(root.GetComponentsInChildren<GlobalDependencyRequirement>(true));

			return requirements;
		}

		private static List<Component> FindProviders(Scene scene, Type providerType)
		{
			var providers = new List<Component>();
			foreach (var root in scene.GetRootGameObjects())
			{
				foreach (var component in root.GetComponentsInChildren(providerType, true))
				{
					if (component is Component provider)
						providers.Add(provider);
				}
			}

			return providers;
		}

		private static GameObject GetOrCreateServicesRoot(
			Scene                                scene,
			GlobalDependencyProvisioningResult result)
		{
			var services = scene.GetRootGameObjects()
								.FirstOrDefault(root => root.name == SERVICES_ROOT_NAME);
			if (services != null)
				return services;

			services = new(SERVICES_ROOT_NAME);
			SceneManager.MoveGameObjectToScene(services, scene);
			Undo.RegisterCreatedObjectUndo(services, "Create Services Root");
			result.ChangeCount++;
			return services;
		}

		private static void EnsureProvider(
			Scene                                scene,
			GlobalDependencyDefinition           definition,
			GlobalDependencyProvisioningResult result)
		{
			var providerType = definition.ProviderType!;
			var providers    = FindProviders(scene, providerType);

			if (providers.Count > 1)
			{
				result.AddError(
					$"Scene '{scene.path}' contains duplicate providers for {providerType.Name}.",
					providers[1]
				);
				return;
			}

			if (providers.Count == 1)
			{
				if (!IsCompatibleProvider(providers[0], definition))
					result.AddError(
						$"Scene '{scene.path}' contains an incompatible manual provider for {providerType.Name}.",
						providers[0]
					);

				return;
			}

			var services = GetOrCreateServicesRoot(scene, result);
			var instance = PrefabUtility.InstantiatePrefab(
				definition.ProviderPrefab!.gameObject,
				scene
			) as GameObject;

			if (instance == null)
			{
				result.AddError(
					$"Could not instantiate provider prefab for {definition.name}.",
					definition
				);
				return;
			}

			Undo.RegisterCreatedObjectUndo(instance, "Create Global Dependency Provider");
			Undo.SetTransformParent(instance.transform, services.transform, "Parent Global Dependency Provider");
			var marker = Undo.AddComponent<GeneratedGlobalDependency>(instance);
			marker.Configure(definition);
			EditorUtility.SetDirty(marker);
			result.ChangeCount++;
		}

		private static bool IsCompatibleProvider(Component provider, GlobalDependencyDefinition definition)
		{
			var scope = provider.GetComponentInParent<SingletonScope>(true);
			var reference = provider.GetComponent<GlobalReference>();
			return provider.gameObject.activeInHierarchy &&
				   provider.GetType() == definition.ProviderType &&
				   scope != null &&
				   scope.Identity == provider &&
				   scope.Lifetime == definition.Lifetime &&
				   reference != null &&
				   reference.Target == provider;
		}

		private static bool ValidateDefinitionAsset(
			GlobalDependencyDefinition           definition,
			GlobalDependencyProvisioningResult result)
		{
			if (!definition.TryValidateConfiguration(out var error))
			{
				result.AddError(error, definition);
				return false;
			}

			if (!PrefabUtility.IsPartOfPrefabAsset(definition.ProviderPrefab))
			{
				result.AddError(
					$"{definition.name} provider must reference a prefab asset.",
					definition
				);
				return false;
			}

			return true;
		}

		private static void ValidateApplicationDependency(
			GlobalDependencyDefinition           definition,
			GameInitializationConfig?             initializationConfig,
			GlobalDependencyProvisioningResult result)
		{
			if (initializationConfig == null)
			{
				result.AddError(
					$"{definition.name} requires an explicit entry in {nameof(GameInitializationConfig)}.",
					definition
				);
				return;
			}

			var configured = initializationConfig.ApplicationDependencies
												 .Where(item => item != null)
												 .ToArray();
			if (configured.Contains(definition))
				return;

			if (configured.Any(item => item.ProviderType == definition.ProviderType))
				result.AddError(
					$"{nameof(GameInitializationConfig)} contains a conflicting definition for {definition.ProviderType!.Name}.",
					initializationConfig
				);
			else
				result.AddError(
					$"{definition.name} is missing from {nameof(GameInitializationConfig)}.",
					initializationConfig
				);
		}

		private static void ValidateCollectionDefinitions(
			SceneCollection                         collection,
			IEnumerable<GlobalDependencyDefinition> requiredDefinitions,
			GlobalDependencyProvisioningResult      result)
		{
			var configuredDefinitions = collection.Entries
												  .SelectMany(entry => entry.Dependencies)
												  .Where(definition => definition != null)
												  .ToArray();
			foreach (var group in configuredDefinitions
								  .Where(definition => definition.ProviderType != null)
								  .GroupBy(definition => definition.ProviderType))
			{
				if (group.Count() > 1)
					result.AddError(
						$"{collection.name} contains duplicate provider type {group.Key!.Name}.",
						collection
					);
			}

			var definitions = configuredDefinitions
							  .Concat(requiredDefinitions)
							  .ToArray();
			ValidateDefinitionConflicts(definitions, result, collection);
		}

		private static void ValidateDefinitionConflicts(
			IEnumerable<GlobalDependencyDefinition> definitions,
			GlobalDependencyProvisioningResult      result,
			Object?                                 context)
		{
			foreach (var group in definitions
								  .Where(definition => definition.ProviderType != null)
								  .GroupBy(definition => definition.ProviderType))
			{
				if (group.Distinct().Count() <= 1)
					continue;

				result.AddError(
					$"Global dependency definitions for {group.Key!.Name} are conflicting.",
					context
				);
			}
		}

		private static void RemoveUnusedManagedDependencies(
			SceneCollection?                   collection,
			GlobalDependencyProvisioningResult result)
		{
			if (collection == null || !AllCollectionScenesLoaded(collection))
				return;

			foreach (var entry in collection.Entries)
			{
				foreach (var definition in entry.RequirementManagedDependencies.ToArray())
				{
					if (definition == null ||
						definition.OrphanPolicy != GlobalDependencyOrphanPolicy.RemoveGeneratedWhenUnused ||
						IsRequiredInCollection(collection, definition))
						continue;

					Undo.RecordObject(collection, "Remove Unused Scene Global Dependency");
					if (!entry.RemoveManagedDependency(definition))
						continue;

					EditorUtility.SetDirty(collection);
					result.ChangeCount++;
				}
			}
		}

		private static void RemoveGeneratedOrphans(
			Scene                                scene,
			SceneCollection?                     collection,
			IReadOnlyCollection<GlobalDependencyDefinition> sceneRequirements,
			GlobalDependencyProvisioningResult result)
		{
			var markers = new List<GeneratedGlobalDependency>();
			foreach (var root in scene.GetRootGameObjects())
				markers.AddRange(root.GetComponentsInChildren<GeneratedGlobalDependency>(true));

			foreach (var marker in markers)
			{
				var definition = marker.Definition;
				if (definition == null ||
					definition.OrphanPolicy != GlobalDependencyOrphanPolicy.RemoveGeneratedWhenUnused)
					continue;

				bool isRequired = collection != null
									  ? !AllCollectionScenesLoaded(collection) ||
										IsRequiredInCollection(collection, definition) ||
										collection.Entries.Any(entry => entry.Dependencies.Contains(definition))
									  : sceneRequirements.Contains(definition);

				if (isRequired)
					continue;

				var providerRoot = marker.gameObject;
				Undo.DestroyObjectImmediate(providerRoot);
				result.ChangeCount++;
			}
		}

		private static bool AllCollectionScenesLoaded(SceneCollection collection)
		{
			foreach (var entry in collection.Entries)
			{
				string? path = entry.Scene?.Path;
				if (string.IsNullOrEmpty(path))
					return false;

				var scene = SceneManager.GetSceneByPath(path);
				if (!scene.IsValid() || !scene.isLoaded)
					return false;
			}

			return true;
		}

		private static bool IsRequiredInCollection(
			SceneCollection          collection,
			GlobalDependencyDefinition definition)
		{
			foreach (var entry in collection.Entries)
			{
				string? path = entry.Scene?.Path;
				if (string.IsNullOrEmpty(path))
					return true;

				var scene = SceneManager.GetSceneByPath(path);
				if (!scene.IsValid() || !scene.isLoaded)
					return true;

				if (FindRequirements(scene).Any(requirement => requirement.Dependencies.Contains(definition)))
					return true;
			}

			return false;
		}

	}
}
