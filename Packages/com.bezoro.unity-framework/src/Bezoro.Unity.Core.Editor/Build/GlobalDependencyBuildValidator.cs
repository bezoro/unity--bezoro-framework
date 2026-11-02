using System;
using System.Collections.Generic;
using System.Linq;
using Bezoro.Unity.Core.Dependencies;
using Bezoro.Unity.Core.MonoBehaviours;
using Bezoro.Unity.Core.ScriptableObjects;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Bezoro.Unity.Core.Editor.Dependencies
{
	public sealed class GlobalDependencyBuildValidationResult
	{
		private readonly List<GlobalDependencyDiagnostic> _diagnostics = new();

		public IReadOnlyList<GlobalDependencyDiagnostic> Diagnostics => _diagnostics;
		public bool HasErrors => _diagnostics.Any(item => item.Severity == GlobalDependencyDiagnosticSeverity.Error);

		internal void AddError(string message, Object? context = null) =>
			_diagnostics.Add(
				new(GlobalDependencyDiagnosticSeverity.Error, message, context)
			);
	}

	public sealed class GlobalDependencyBuildValidator : IPreprocessBuildWithReport
	{
		public static bool IsValidating { get; private set; }

		public int callbackOrder => int.MinValue;

		public void OnPreprocessBuild(BuildReport report)
		{
			var settings    = GameSettings.Instance;
			var config      = settings?.InitializationConfig ?? FindAssets<GameInitializationConfig>().FirstOrDefault();
			var collections = FindAssets<SceneCollection>();
			var scenePaths = EditorBuildSettings.scenes
											 .Where(scene => scene.enabled)
											 .Select(scene => scene.path)
											 .ToArray();
			var result = ValidateProject(config, collections, scenePaths);
			if (!result.HasErrors)
				return;

			throw new BuildFailedException(
				string.Join(
					Environment.NewLine,
					result.Diagnostics.Select(item => item.Message)
				)
			);
		}

		public static GlobalDependencyBuildValidationResult ValidateProject(
			GameInitializationConfig?      initializationConfig,
			IReadOnlyList<SceneCollection> collections,
			IReadOnlyCollection<string>    buildScenePaths)
		{
			if (collections == null)
				throw new ArgumentNullException(nameof(collections));

			if (buildScenePaths == null)
				throw new ArgumentNullException(nameof(buildScenePaths));

			var result = new GlobalDependencyBuildValidationResult();
			IsValidating = true;

			try
			{
				ValidateApplicationDependencies(initializationConfig, result);
				ValidateCollectionMembership(collections, result);

				var collectionScenePaths = new HashSet<string>(
					collections
						.Where(collection => collection != null)
						.SelectMany(collection => collection.Entries)
						.Select(entry => entry.Scene?.Path)
						.Where(path => !string.IsNullOrEmpty(path))!
				);

				foreach (var collection in collections.Where(collection => collection != null))
					ValidateCollection(
						collection,
						initializationConfig,
						buildScenePaths,
						result
					);

				foreach (string scenePath in buildScenePaths)
				{
					if (!collectionScenePaths.Contains(scenePath))
						ValidateStandaloneScene(scenePath, initializationConfig, result);
				}
			}
			finally
			{
				IsValidating = false;
			}

			return result;
		}

		private static void ValidateApplicationDependencies(
			GameInitializationConfig?              config,
			GlobalDependencyBuildValidationResult result)
		{
			if (config == null)
				return;

			var providerTypes = new HashSet<Type>();
			for (var i = 0; i < config.ApplicationDependencies.Count; i++)
			{
				var definition = config.ApplicationDependencies[i];
				if (!ValidateDefinition(definition, result))
					continue;

				if (definition.Lifetime != SingletonLifetime.Application)
					result.AddError(
						$"{definition.name} must use {nameof(SingletonLifetime.Application)} lifetime.",
						definition
					);

				if (definition.ProviderType != null && !providerTypes.Add(definition.ProviderType))
					result.AddError(
						$"{nameof(GameInitializationConfig)} contains duplicate provider type {definition.ProviderType.Name}.",
						config
					);
			}
		}

		private static void ValidateCollectionMembership(
			IReadOnlyList<SceneCollection>          collections,
			GlobalDependencyBuildValidationResult result)
		{
			var ownersByPath = new Dictionary<string, SceneCollection>();
			foreach (var collection in collections.Where(collection => collection != null))
			{
				var localPaths = new HashSet<string>();
				foreach (var entry in collection.Entries)
				{
					string? path = entry.Scene?.Path;
					if (path is not { Length: > 0 })
					{
						result.AddError($"{collection.name} contains an unresolved scene entry.", collection);
						continue;
					}

					if (!localPaths.Add(path))
						result.AddError($"{collection.name} contains duplicate scene '{path}'.", collection);

					if (ownersByPath.TryGetValue(path, out var existing) && existing != collection)
						result.AddError(
							$"Scene '{path}' belongs to both {existing.name} and {collection.name}.",
							collection
						);
					else
						ownersByPath[path] = collection;
				}
			}
		}

		private static void ValidateCollection(
			SceneCollection                       collection,
			GameInitializationConfig?             config,
			IReadOnlyCollection<string>           buildScenePaths,
			GlobalDependencyBuildValidationResult result)
		{
			ValidateDefinitionConflicts(
				collection.Entries.SelectMany(entry => entry.Dependencies),
				collection,
				result
			);

			var providerCounts = new Dictionary<Type, int>();

			for (var entryIndex = 0; entryIndex < collection.Entries.Count; entryIndex++)
			{
				var entry     = collection.Entries[entryIndex];
				string? path = entry.Scene?.Path;
				if (path is not { Length: > 0 })
					continue;

				if (!buildScenePaths.Contains(path))
					result.AddError(
						$"Scene '{path}' from {collection.name} is missing from enabled build scenes.",
						collection
					);

				WithScene(
					path,
					scene =>
					{
						CountGlobalProviders(scene, providerCounts);

						foreach (var definition in entry.Dependencies)
						{
							if (!ValidateDefinition(definition, result))
								continue;

							ValidateLocalProvider(scene, definition, result);
						}

						foreach (var requirement in FindRequirements(scene))
						{
							foreach (var definition in requirement.Dependencies)
								ValidateCollectionRequirement(
									collection,
									entryIndex,
									scene,
									definition,
									config,
									result
								);
						}
					},
					result,
					collection
				);
			}

			foreach (var pair in providerCounts.Where(pair => pair.Value > 1))
				result.AddError(
					$"{collection.name} contains duplicate provider type {pair.Key.Name}.",
					collection
				);
		}

		private static void ValidateCollectionRequirement(
			SceneCollection                       collection,
			int                                   requirementEntryIndex,
			Scene                                 requirementScene,
			GlobalDependencyDefinition            definition,
			GameInitializationConfig?             config,
			GlobalDependencyBuildValidationResult result)
		{
			if (!ValidateDefinition(definition, result))
				return;

			if (definition.Lifetime == SingletonLifetime.Application)
			{
				ValidateApplicationRequirement(definition, config, result);
				return;
			}

			int providerEntryIndex = -1;
			for (var i = 0; i < collection.Entries.Count; i++)
			{
				if (collection.Entries[i].Dependencies.Contains(definition))
				{
					providerEntryIndex = i;
					break;
				}
			}

			if (providerEntryIndex < 0)
			{
				if (!HasCompatibleProvider(requirementScene, definition))
					result.AddError(
						$"Scene '{requirementScene.path}' requires {definition.name}, but no provider is configured.",
						definition
					);

				return;
			}

			if (providerEntryIndex > requirementEntryIndex)
				result.AddError(
					$"Provider scene for {definition.name} must appear before dependent scene '{requirementScene.path}'.",
					collection
				);
		}

		private static void ValidateStandaloneScene(
			string                                scenePath,
			GameInitializationConfig?             config,
			GlobalDependencyBuildValidationResult result)
		{
			WithScene(
				scenePath,
				scene =>
				{
					var providerCounts = new Dictionary<Type, int>();
					CountGlobalProviders(scene, providerCounts);
					foreach (var pair in providerCounts.Where(pair => pair.Value > 1))
						result.AddError(
							$"Standalone scene '{scenePath}' contains duplicate provider type {pair.Key.Name}."
						);

					foreach (var requirement in FindRequirements(scene))
					{
						foreach (var definition in requirement.Dependencies)
						{
							if (!ValidateDefinition(definition, result))
								continue;

							if (definition.Lifetime == SingletonLifetime.Application)
								ValidateApplicationRequirement(definition, config, result);
							else if (!HasCompatibleProvider(scene, definition))
								result.AddError(
									$"Standalone scene '{scenePath}' requires {definition.name}, but has no compatible provider.",
									definition
								);
						}
					}
				},
				result,
				null
			);
		}

		private static void ValidateApplicationRequirement(
			GlobalDependencyDefinition            definition,
			GameInitializationConfig?              config,
			GlobalDependencyBuildValidationResult result)
		{
			if (config == null || !config.ApplicationDependencies.Contains(definition))
				result.AddError(
					$"{definition.name} must be explicitly listed in {nameof(GameInitializationConfig)}.",
					definition
				);
		}

		private static bool ValidateDefinition(
			GlobalDependencyDefinition?           definition,
			GlobalDependencyBuildValidationResult result)
		{
			if (definition == null)
			{
				result.AddError("A global dependency definition is missing.");
				return false;
			}

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

		private static void ValidateDefinitionConflicts(
			IEnumerable<GlobalDependencyDefinition> definitions,
			Object                                  context,
			GlobalDependencyBuildValidationResult result)
		{
			foreach (var group in definitions
								  .Where(definition => definition != null &&
													   definition.ProviderType != null)
								  .GroupBy(definition => definition.ProviderType))
			{
				if (group.Distinct().Count() > 1)
					result.AddError(
						$"{context.name} contains conflicting definitions for {group.Key!.Name}.",
						context
					);
			}
		}

		private static void ValidateLocalProvider(
			Scene                                 scene,
			GlobalDependencyDefinition            definition,
			GlobalDependencyBuildValidationResult result)
		{
			var providers = FindProviders(scene, definition.ProviderType!);
			if (providers.Count == 0)
			{
				result.AddError(
					$"Provider scene '{scene.path}' does not contain {definition.ProviderType!.Name}.",
					definition
				);
				return;
			}

			if (providers.Count > 1)
				result.AddError(
					$"Provider scene '{scene.path}' contains duplicate {definition.ProviderType!.Name} providers.",
					definition
				);

			if (!IsCompatibleProvider(providers[0], definition))
				result.AddError(
					$"Provider in scene '{scene.path}' is incompatible with {definition.name}.",
					providers[0]
				);
		}

		private static bool HasCompatibleProvider(Scene scene, GlobalDependencyDefinition definition) =>
			FindProviders(scene, definition.ProviderType!)
				.Any(provider => IsCompatibleProvider(provider, definition));

		private static bool IsCompatibleProvider(Component provider, GlobalDependencyDefinition definition)
		{
			var scope     = provider.GetComponentInParent<SingletonScope>(true);
			var reference = provider.GetComponent<GlobalReference>();
			return provider.gameObject.activeInHierarchy &&
				   provider.GetType() == definition.ProviderType &&
				   scope != null &&
				   scope.Identity == provider &&
				   scope.Lifetime == definition.Lifetime &&
				   reference != null &&
				   reference.Target == provider;
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

		private static List<GlobalDependencyRequirement> FindRequirements(Scene scene)
		{
			var requirements = new List<GlobalDependencyRequirement>();
			foreach (var root in scene.GetRootGameObjects())
				requirements.AddRange(root.GetComponentsInChildren<GlobalDependencyRequirement>(true));

			return requirements;
		}

		private static void CountGlobalProviders(Scene scene, IDictionary<Type, int> counts)
		{
			var references = new List<GlobalReference>();
			foreach (var root in scene.GetRootGameObjects())
				references.AddRange(root.GetComponentsInChildren<GlobalReference>(true));

			foreach (var reference in references)
			{
				if (reference.Target == null)
					continue;

				var type = reference.Target.GetType();
				counts[type] = counts.TryGetValue(type, out int count) ? count + 1 : 1;
			}
		}

		private static void WithScene(
			string                                scenePath,
			Action<Scene>                         inspect,
			GlobalDependencyBuildValidationResult result,
			Object?                               context)
		{
			var scene  = SceneManager.GetSceneByPath(scenePath);
			bool close = !scene.IsValid() || !scene.isLoaded;

			try
			{
				if (close)
					scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);

				inspect(scene);
			}
			catch (Exception exception)
			{
				result.AddError(
					$"Could not validate scene '{scenePath}': {exception.Message}",
					context
				);
			}
			finally
			{
				if (close && scene.IsValid() && scene.isLoaded)
					EditorSceneManager.CloseScene(scene, true);
			}
		}

		private static T[] FindAssets<T>()
			where T : Object =>
			AssetDatabase.FindAssets($"t:{typeof(T).Name}")
						 .Select(AssetDatabase.GUIDToAssetPath)
						 .Select(AssetDatabase.LoadAssetAtPath<T>)
						 .Where(asset => asset != null)
						 .ToArray();
	}
}
