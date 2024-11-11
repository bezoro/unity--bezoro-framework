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

		private void ExecuteEntries( IInitializationObject[] entries )
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
	}

	public interface IInitializationObject
	{
		void Initialize();
	}
}
