using System;
using System.Collections.Generic;
using Bezoro.Unity.Core.Dependencies;
using UnityEngine;

namespace Bezoro.Unity.Core.MonoBehaviours
{
	[DisallowMultipleComponent]
	public sealed class GlobalDependencyRequirement : MonoBehaviour
	{
		[SerializeField] private GlobalDependencyDefinition[] dependencies = Array.Empty<GlobalDependencyDefinition>();

		public IReadOnlyList<GlobalDependencyDefinition> Dependencies => dependencies;

		internal void Configure(params GlobalDependencyDefinition[] configuredDependencies)
		{
			dependencies = configuredDependencies != null
							   ? (GlobalDependencyDefinition[])configuredDependencies.Clone()
							   : throw new ArgumentNullException(nameof(configuredDependencies));
		}
	}
}
