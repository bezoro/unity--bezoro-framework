using System;
using Bezoro.Unity.Core.MonoBehaviours;
using UnityEngine;

namespace Bezoro.Unity.Core.Dependencies
{
	[CreateAssetMenu(
		fileName = "SO_DEP_NewGlobalDependency",
		menuName = "Bezoro/Runtime/Global Dependency",
		order = 0
	)]
	public sealed class GlobalDependencyDefinition : ScriptableObject
	{
		[SerializeField] private GameObject? providerPrefab;
		[SerializeField] private GlobalDependencyOrphanPolicy orphanPolicy;

		public GlobalDependencyOrphanPolicy OrphanPolicy => orphanPolicy;
		public Component? ProviderPrefab =>
			providerPrefab != null &&
			providerPrefab.TryGetComponent<GlobalReference>(out var reference)
				? reference.Target
				: null;
		public GameObject? ProviderPrefabRoot => providerPrefab;
		public Type? ProviderType => ProviderPrefab != null ? ProviderPrefab.GetType() : null;

		public SingletonLifetime? Lifetime
		{
			get
			{
				var provider = ProviderPrefab;
				if (provider == null)
					return null;

				return provider.TryGetComponent<SingletonScope>(out var scope)
						   ? scope.Lifetime
						   : null;
			}
		}

		public bool TryValidateConfiguration(out string error)
		{
			if (providerPrefab == null)
			{
				error = $"{name} requires a provider prefab component.";
				return false;
			}

			if (!providerPrefab.activeSelf)
			{
				error = $"{name} provider prefab root must be active.";
				return false;
			}

			var provider = ProviderPrefab;
			if (provider == null)
			{
				error = $"{name} provider prefab requires a configured {nameof(GlobalReference)} target.";
				return false;
			}

			if (provider.transform != provider.transform.root)
			{
				error = $"{name} provider prefab component must be on the prefab root.";
				return false;
			}

			if (!provider.TryGetComponent<SingletonScope>(out var scope) ||
				scope.Identity != provider)
			{
				error = $"{name} provider prefab requires a {nameof(SingletonScope)} targeting the provider.";
				return false;
			}

			if (!provider.TryGetComponent<GlobalReference>(out var reference) ||
				reference.Target != provider)
			{
				error = $"{name} provider prefab requires a {nameof(GlobalReference)} targeting the provider.";
				return false;
			}

			error = string.Empty;
			return true;
		}

		internal Component InstantiateProvider()
		{
			if (!TryValidateConfiguration(out var error))
				throw new InvalidOperationException(error);

			var prefab       = ProviderPrefab!;
			var instanceRoot = Instantiate(providerPrefab!);
			instanceRoot.name = providerPrefab!.name;

			var provider = instanceRoot.GetComponent(prefab.GetType());
			if (provider != null)
				return provider;

			if (UnityEngine.Application.isPlaying)
				Destroy(instanceRoot);
			else
				DestroyImmediate(instanceRoot);

			throw new InvalidOperationException(
				$"Instantiated provider '{prefab.name}' does not contain {prefab.GetType().Name} on its root."
			);
		}

		internal void Configure(Component configuredProviderPrefab, GlobalDependencyOrphanPolicy configuredOrphanPolicy)
		{
			providerPrefab = configuredProviderPrefab != null
								 ? configuredProviderPrefab.gameObject
								 : throw new ArgumentNullException(nameof(configuredProviderPrefab));
			orphanPolicy = configuredOrphanPolicy;
		}
	}
}
