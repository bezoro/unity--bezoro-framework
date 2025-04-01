using System.Threading;
using Bezoro.Framework.Core;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;

namespace Bezoro.Framework.Unity.Bootstrapping
{
	[DefaultExecutionOrder(int.MinValue)]
	[CreateAssetMenu(
		fileName = Constants.GameInitialization.CONFIG_ASSET_MENU_FILE_NAME,
		menuName = Constants.GameInitialization.CONFIG_ASSET_MENU_PATH,
		order = Constants.GameInitialization.CONFIG_ASSET_MENU_ORDER
	)]
	public sealed class Game_Initialization_Config : ScriptableObject
	{
		[FormerlySerializedAs("subsystem_Registration_Entries")]
		[SerializeReference] private IInitializationObject[] subsystemRegistrationEntries = { };
		[FormerlySerializedAs("after_Assembly_Load_Entries")]
		[SerializeReference] private IInitializationObject[] afterAssemblyLoadEntries = { };
		[FormerlySerializedAs("before_Splash_Screen_Entries")]
		[SerializeReference] private IInitializationObject[] beforeSplashScreenEntries = { };
		[FormerlySerializedAs("before_Scene_Load_Entries")]
		[SerializeReference] private IInitializationObject[] beforeSceneLoadEntries = { };
		[FormerlySerializedAs("after_Scene_Load_Entries")]
		[SerializeReference] private IInitializationObject[] afterSceneLoadEntries = { };

		public static bool Is_Game_Initialized { get; private set; }

		public void SubsystemRegistrationPhase()
		{
			ExecuteEntries(subsystemRegistrationEntries);
		}

		public void AfterAssemblyLoadPhase()
		{
			ExecuteEntries(afterAssemblyLoadEntries);
		}

		public void BeforeSplashScreenPhase()
		{
			ExecuteEntries(beforeSplashScreenEntries);
		}

		public void BeforeSceneLoadPhase()
		{
			ExecuteEntries(beforeSceneLoadEntries);
		}

		public void AfterSceneLoadPhase()
		{
			ExecuteEntries(afterSceneLoadEntries);
			Is_Game_Initialized = true;
		}

		public static async UniTask<bool> WaitForInitializationToComplete(CancellationToken ct = default)
		{
			while ( !Is_Game_Initialized ) await UniTask.Yield(ct);
			return true;
		}

		private static void ExecuteEntries(IInitializationObject[] entries)
		{
			Assert.IsNotNull(entries);
			foreach ( var entry in entries ) entry.Initialize();
		}
	}

	public interface IInitializationObject
	{
		public void Initialize();
	}
}
