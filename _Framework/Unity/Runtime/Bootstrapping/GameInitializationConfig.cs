using System.Threading;
using Bezoro.Framework.Core;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Bezoro.Framework.Unity.Bootstrapping
{
	[DefaultExecutionOrder( int.MinValue )]
	[CreateAssetMenu( fileName = "GIC_NewGameInitConfig", menuName = "Bezoro/Game Init Config", order = 0 )]
	public sealed class GameInitializationConfig : ScriptableSingleton<GameInitializationConfig>
	{
		[SerializeReference] private IInitializationObject[] subsystemRegistrationEntries;
		[SerializeReference] private IInitializationObject[] afterAssemblyLoadEntries;
		[SerializeReference] private IInitializationObject[] beforeSplashScreenEntries;
		[SerializeReference] private IInitializationObject[] beforeSceneLoadEntries;
		[SerializeReference] private IInitializationObject[] afterSceneLoadEntries;

		public static bool IsGameInitialized { get; private set; }

		[RuntimeInitializeOnLoadMethod( RuntimeInitializeLoadType.SubsystemRegistration )]
		public static void SubsystemRegistrationPhase()
		{
			ExecuteEntries( Instance.subsystemRegistrationEntries );
		}

		[RuntimeInitializeOnLoadMethod( RuntimeInitializeLoadType.AfterAssembliesLoaded )]
		public static void AfterAssemblyLoadPhase()
		{
			ExecuteEntries( Instance.afterAssemblyLoadEntries );
		}

		[RuntimeInitializeOnLoadMethod( RuntimeInitializeLoadType.BeforeSplashScreen )]
		public static void BeforeSplashScreenPhase()
		{
			ExecuteEntries( Instance.beforeSplashScreenEntries );
		}

		[RuntimeInitializeOnLoadMethod( RuntimeInitializeLoadType.BeforeSceneLoad )]
		public static void BeforeSceneLoadPhase()
		{
			ExecuteEntries( Instance.beforeSceneLoadEntries );
		}

		[RuntimeInitializeOnLoadMethod( RuntimeInitializeLoadType.AfterSceneLoad )]
		public static void AfterSceneLoadPhase()
		{
			ExecuteEntries( Instance.afterSceneLoadEntries );
			IsGameInitialized = true;
		}

		private static void ExecuteEntries( IInitializationObject[] entries )
		{
			Assert.IsNotNull( entries );

			foreach ( var entry in entries ) { entry.Initialize(); }
		}

		public static async UniTask<bool> WaitForInitializationComplete( CancellationToken cancellationToken = default )
		{
			while (!IsGameInitialized) { await UniTask.Yield( cancellationToken ); }
			return true;
		}
	}

	public interface IInitializationObject
	{
		void Initialize();
	}
}
