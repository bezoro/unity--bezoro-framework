using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Bezoro.Framework.Unity.Bootstrapping
{
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
			foreach ( var entry in entries )
			{
				entry.Initialize();
			}
		}

		public void SubsystemRegistrationPhase()
		{
			ExecuteEntries( subsystemRegistrationEntries );
		}

		public void AfterAssemblyLoadPhase()
		{
			ExecuteEntries( afterAssemblyLoadEntries );
		}

		public void BeforeSplashScreenPhase()
		{
			ExecuteEntries( beforeSplashScreenEntries );
		}

		public void BeforeSceneLoadPhase()
		{
			ExecuteEntries( beforeSceneLoadEntries );
		}

		public void AfterSceneLoadPhase()
		{
			ExecuteEntries( afterSceneLoadEntries );
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
