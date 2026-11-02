using System;
using System.Collections.Generic;
using Bezoro.Unity.Core.MonoBehaviours;

namespace Bezoro.Unity.Core.Dependencies
{
	internal static class GlobalDependencyBootstrapper
	{
		internal static void BootstrapApplicationDependencies(
			IReadOnlyList<GlobalDependencyDefinition>? definitions,
			Action<GlobalDependencyDefinition>          instantiate)
		{
			if (instantiate == null)
				throw new ArgumentNullException(nameof(instantiate));

			if (definitions == null)
				return;

			var providerTypes = new HashSet<Type>();

			for (var i = 0; i < definitions.Count; i++)
			{
				var definition = definitions[i];
				if (definition == null)
					throw new InvalidOperationException($"Application dependency at index {i} is missing.");

				if (!definition.TryValidateConfiguration(out var error))
					throw new InvalidOperationException(error);

				if (definition.Lifetime != SingletonLifetime.Application)
					throw new InvalidOperationException(
						$"{definition.name} must use {nameof(SingletonLifetime.Application)} lifetime."
					);

				var providerType = definition.ProviderType!;
				if (!providerTypes.Add(providerType))
					throw new InvalidOperationException(
						$"Application dependencies contain duplicate provider type {providerType.Name}."
					);

				instantiate(definition);
			}
		}
	}
}
