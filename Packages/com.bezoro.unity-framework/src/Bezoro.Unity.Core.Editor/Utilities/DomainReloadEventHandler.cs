using System;
using UnityEditor;

namespace Bezoro.Unity.Core.Editor.Utilities
{
	/// <summary>
	///     Publishes events around Unity assembly reloads.
	/// </summary>
	public sealed class DomainReloadEventHandler
	{
		public static event Action? DomainReloadFinished;
		public static event Action? DomainReloadStarted;
		static DomainReloadEventHandler()
		{
			AssemblyReloadEvents.beforeAssemblyReload += On_Before_Assembly_Reload;
			AssemblyReloadEvents.afterAssemblyReload  += On_After_Assembly_Reload;
		}

		public static bool IsDomainReloading { get; private set; }

		private static void On_After_Assembly_Reload()
		{
			IsDomainReloading = false;
			DomainReloadFinished?.Invoke();
		}

		private static void On_Before_Assembly_Reload()
		{
			IsDomainReloading = true;
			DomainReloadStarted?.Invoke();
		}
	}
}
