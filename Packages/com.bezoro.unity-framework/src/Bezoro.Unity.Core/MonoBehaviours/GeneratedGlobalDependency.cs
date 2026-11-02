using System;
using Bezoro.Unity.Core.Dependencies;
using UnityEngine;

namespace Bezoro.Unity.Core.MonoBehaviours
{
	[AddComponentMenu("")]
	[DisallowMultipleComponent]
	public sealed class GeneratedGlobalDependency : MonoBehaviour
	{
		[SerializeField] private GlobalDependencyDefinition? definition;

		public GlobalDependencyDefinition? Definition => definition;

		internal void Configure(GlobalDependencyDefinition configuredDefinition)
		{
			definition = configuredDefinition != null
							 ? configuredDefinition
							 : throw new ArgumentNullException(nameof(configuredDefinition));
		}
	}
}
