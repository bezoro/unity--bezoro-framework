using System.Threading;
using Bezoro.Framework.Core;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Bezoro.Framework.Unity.Bootstrapping
{
	[DefaultExecutionOrder( int.MinValue )]
	[CreateAssetMenu(
		fileName = "GIC_NewGameInitConfig",
		menuName = "Bezoro/Game Init Config",
		order = 0 )]
	public sealed class GameInitializationConfig : ScriptableObject
	{
		[SerializeReference] private IInitializationObject[] subsystem_registration_entries = { };
		[SerializeReference] private IInitializationObject[] after_assembly_load_entries    = { };
		[SerializeReference] private IInitializationObject[] before_splash_screen_entries   = { };
		[SerializeReference] private IInitializationObject[] before_scene_load_entries      = { };
		[SerializeReference] private IInitializationObject[] after_scene_load_entries       = { };

		public static bool IsGameInitialized { get; private set; }

		public void SubsystemRegistrationPhase()
		{
			ExecuteEntries( subsystem_registration_entries );
		}

		public void AfterAssemblyLoadPhase()
		{
			ExecuteEntries( after_assembly_load_entries );
		}

		public void BeforeSplashScreenPhase()
		{
			ExecuteEntries( before_splash_screen_entries );
		}

		public void BeforeSceneLoadPhase()
		{
			ExecuteEntries( before_scene_load_entries );
		}

		public void AfterSceneLoadPhase()
		{
			ExecuteEntries( after_scene_load_entries );
			IsGameInitialized = true;
		}

		private static void ExecuteEntries( IInitializationObject[] entries )
		{
			Assert.Is_Not_Null( entries );

			foreach ( var entry in entries ) { entry.Initialize(); }
		}

		public static async UniTask<bool> WaitForInitializationComplete( CancellationToken cancellationToken = default )
		{
			while ( !IsGameInitialized ) { await UniTask.Yield( cancellationToken ); }

			return true;
		}
	}

	public interface IInitializationObject
	{
		public void Initialize();
	}
}
