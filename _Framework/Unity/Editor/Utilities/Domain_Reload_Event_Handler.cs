using System;
using UnityEditor;

namespace Bezoro.Framework.Unity.Editor.Utilities
{
	public sealed class Domain_Reload_Event_Handler
	{
		static Domain_Reload_Event_Handler()
		{
			AssemblyReloadEvents.beforeAssemblyReload += On_Before_Assembly_Reload;
			AssemblyReloadEvents.afterAssemblyReload  += On_After_Assembly_Reload;
		}

		public static bool         IsDomainReloading { get; private set; }
		public static event Action Domain_Reload_Started;
		public static event Action Domain_Reload_Finished;

		private static void On_Before_Assembly_Reload()
		{
			IsDomainReloading = true;
			Domain_Reload_Started?.Invoke();
		}

		private static void On_After_Assembly_Reload()
		{
			IsDomainReloading = false;
			Domain_Reload_Finished?.Invoke();
		}
	}
}
