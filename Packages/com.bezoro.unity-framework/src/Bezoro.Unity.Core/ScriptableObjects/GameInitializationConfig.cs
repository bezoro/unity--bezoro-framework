using System;
using System.Collections.Generic;
using System.Threading;
using Bezoro.Core.Utilities;
using Bezoro.Unity.Core.Common.Enums;
using Bezoro.Unity.Core.Dependencies;
using Bezoro.Unity.Core.MonoBehaviours;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Logger = Bezoro.Logging.Logger;

namespace Bezoro.Unity.Core.ScriptableObjects
{
	[DefaultExecutionOrder(
		ExecutionOrder.Fundamental
	)]
	[CreateAssetMenu(
		fileName = Constants.GameInitialization.CONFIG_ASSET_MENU_FILE_NAME,
		menuName = Constants.GameInitialization.CONFIG_ASSET_MENU_PATH,
		order = Constants.GameInitialization.CONFIG_ASSET_MENU_ORDER
	)]
	/// <summary>
	/// Runs serialized initialization entries at Unity runtime initialization phases.
	/// </summary>
	public sealed class GameInitializationConfig : ScriptableObject
	{
		public static bool IsGameInitialized { get; private set; }

		public IReadOnlyList<GlobalDependencyDefinition> ApplicationDependencies => applicationDependencies;

		public static async UniTask<bool> WaitForInitializationToComplete(CancellationToken ct = default)
		{
			while (!IsGameInitialized)
			{
				await UniTask.Yield(
					ct
				);
			}

			return true;
		}

		public void AfterAssemblyLoadPhase() =>
			ExecuteEntries(
				afterAssemblyLoadEntries
			);

		public void AfterSceneLoadPhase()
		{
			ExecuteEntries(
				afterSceneLoadEntries
			);

			IsGameInitialized = true;
		}

		public void BeforeSceneLoadPhase() =>
			GameInitializationConfigModel.ExecuteBeforeSceneLoad(
				applicationDependencies,
				beforeSceneLoadEntries,
				InstantiateApplicationDependency
			);

		public void BeforeSplashScreenPhase() =>
			ExecuteEntries(
				beforeSplashScreenEntries
			);

		public void SubsystemRegistrationPhase() =>
			ExecuteEntries(
				subsystemRegistrationEntries
			);

		private static void ExecuteEntries(IInitializationObject[] entries)
		{
			if (entries == null)
			{
				Logger.LogWarning(
					"No entries to execute."
				);

				return;
			}

			foreach (var entry in entries) entry.Initialize();
		}

		private static void InstantiateApplicationDependency(GlobalDependencyDefinition definition)
		{
			var providerType = definition.ProviderType!;
			if (GlobalReferenceRegistry.TryGet(providerType, out _))
				return;

			var provider = definition.InstantiateProvider();
			if (GlobalReferenceRegistry.TryGet(providerType, out var registered) &&
				registered == provider)
				return;

			if (provider != null)
				Destroy(provider.gameObject);

			throw new InvalidOperationException(
				$"{definition.name} instantiated {providerType.Name}, but it did not register globally."
			);
		}

		#region Serialized Fields

		[SerializeReference] private IInitializationObject[] subsystemRegistrationEntries = { };
		[SerializeReference] private IInitializationObject[] afterAssemblyLoadEntries     = { };
		[SerializeReference] private IInitializationObject[] beforeSplashScreenEntries    = { };
		[SerializeField] private GlobalDependencyDefinition[] applicationDependencies = { };
		[SerializeReference] private IInitializationObject[] beforeSceneLoadEntries       = { };
		[SerializeReference] private IInitializationObject[] afterSceneLoadEntries        = { };

		#endregion
	}

	internal static class GameInitializationConfigModel
	{
		internal static void ExecuteBeforeSceneLoad(
			IReadOnlyList<GlobalDependencyDefinition>? applicationDependencies,
			IInitializationObject[]                    beforeSceneLoadEntries,
			Action<GlobalDependencyDefinition>          instantiate)
		{
			GlobalDependencyBootstrapper.BootstrapApplicationDependencies(
				applicationDependencies,
				instantiate
			);

			if (beforeSceneLoadEntries == null)
				return;

			foreach (var entry in beforeSceneLoadEntries)
				entry.Initialize();
		}
	}

	/// <summary>
	///     Defines an initialization step invoked by GameInitializationConfig.
	/// </summary>
	public interface IInitializationObject
	{
		public void Initialize();
	}
}
