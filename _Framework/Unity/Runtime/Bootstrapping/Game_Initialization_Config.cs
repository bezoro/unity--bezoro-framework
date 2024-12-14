using System.Threading;
using Bezoro.Framework.Core;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Bezoro.Framework.Unity.Bootstrapping
{
	[DefaultExecutionOrder(int.MinValue)]
	[CreateAssetMenu(
		fileName = Constants.Game_Initialization.GAME_INITIALIZATION_CONFIG_ASSET_MENU_FILE_NAME,
		menuName = Constants.Game_Initialization.GAME_INITIALIZATION_CONFIG_ASSET_MENU_PATH,
		order = Constants.Game_Initialization.GAME_INITIALIZATION_CONFIG_ASSET_MENU_ORDER
	)]
	public sealed class Game_Initialization_Config : ScriptableObject
	{
		[SerializeReference] private I_Initialization_Object[] subsystem_Registration_Entries = { };
		[SerializeReference] private I_Initialization_Object[] after_Assembly_Load_Entries    = { };
		[SerializeReference] private I_Initialization_Object[] before_Splash_Screen_Entries   = { };
		[SerializeReference] private I_Initialization_Object[] before_Scene_Load_Entries      = { };
		[SerializeReference] private I_Initialization_Object[] after_Scene_Load_Entries       = { };

		public static bool Is_Game_Initialized { get; private set; }

		public void SubsystemRegistrationPhase()
		{
			ExecuteEntries(subsystem_Registration_Entries);
		}

		public void AfterAssemblyLoadPhase()
		{
			ExecuteEntries(after_Assembly_Load_Entries);
		}

		public void BeforeSplashScreenPhase()
		{
			ExecuteEntries(before_Splash_Screen_Entries);
		}

		public void BeforeSceneLoadPhase()
		{
			ExecuteEntries(before_Scene_Load_Entries);
		}

		public void AfterSceneLoadPhase()
		{
			ExecuteEntries(after_Scene_Load_Entries);
			Is_Game_Initialized = true;
		}

		public static async UniTask<bool> Wait_For_Initialization_Complete(CancellationToken ct = default)
		{
			while ( !Is_Game_Initialized ) await UniTask.Yield(ct);
			return true;
		}

		private static void ExecuteEntries(I_Initialization_Object[] entries)
		{
			Assert.Is_Not_Null(entries);
			foreach ( var entry in entries ) entry.Initialize();
		}
	}

	public interface I_Initialization_Object
	{
		public void Initialize();
	}
}
