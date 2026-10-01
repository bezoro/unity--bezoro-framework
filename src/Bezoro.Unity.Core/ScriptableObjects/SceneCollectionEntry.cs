using System;
using System.Collections.Generic;
using Bezoro.Unity.Core.Dependencies;
using Bezoro.Unity.Core.Scenes;
using UnityEngine;

namespace Bezoro.Unity.Core.ScriptableObjects
{
	[Serializable]
	public sealed class SceneCollectionEntry
	{
		[SerializeField] private SceneReference? scene;
		[SerializeField] private List<GlobalDependencyDefinition> dependencies = new();
		[SerializeField] [HideInInspector] private List<GlobalDependencyDefinition> requirementManagedDependencies = new();

		public SceneCollectionEntry(
			SceneReference scene,
			params GlobalDependencyDefinition[] configuredDependencies)
		{
			this.scene = scene ?? throw new ArgumentNullException(nameof(scene));

			if (configuredDependencies != null)
				dependencies.AddRange(configuredDependencies);
		}

		public IReadOnlyList<GlobalDependencyDefinition> Dependencies => dependencies;
		public IReadOnlyList<GlobalDependencyDefinition> RequirementManagedDependencies => requirementManagedDependencies;
		public SceneReference? Scene => scene;

		internal bool AddDependency(GlobalDependencyDefinition definition, bool requirementManaged)
		{
			if (definition == null)
				throw new ArgumentNullException(nameof(definition));

			bool added = false;
			if (!dependencies.Contains(definition))
			{
				dependencies.Add(definition);
				added = true;
			}

			if (requirementManaged && !requirementManagedDependencies.Contains(definition))
				requirementManagedDependencies.Add(definition);

			return added;
		}

		internal bool RemoveManagedDependency(GlobalDependencyDefinition definition)
		{
			if (!requirementManagedDependencies.Remove(definition))
				return false;

			dependencies.Remove(definition);
			return true;
		}
	}
}
