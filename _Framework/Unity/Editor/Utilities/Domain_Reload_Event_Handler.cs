using System;
using UnityEditor;

namespace Bezoro.Framework.Unity.Editor.Utilities
{
	public sealed class Domain_Reload_Event_Handler
	{
		public static event Action Domain_Reload_Started;
		public static event Action Domain_Reload_Finished;

		static Domain_Reload_Event_Handler()
		{
			AssemblyReloadEvents.beforeAssemblyReload += On_Before_Assembly_Reload;
			AssemblyReloadEvents.afterAssemblyReload  += On_After_Assembly_Reload;
		}

		private static void On_Before_Assembly_Reload()
		{
			Domain_Reload_Started?.Invoke();
		}

		private static void On_After_Assembly_Reload()
		{
			Domain_Reload_Finished?.Invoke();
		}
	}
}
