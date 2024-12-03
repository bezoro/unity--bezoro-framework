using System;
using System.Threading.Tasks;
using UnityEditor;

namespace Bezoro.Framework.Unity.Editor.Utilities
{
	[InitializeOnLoad]
	public static class EditorStatusWatcher
	{
		static EditorStatusWatcher()
		{
			_ = MonitorEditorStatus();
		}

		public static bool IsEditorBusy =>
			CompilationEventHandler.IsCompilationInProgress
			|| BuildEventHandler.IsBuildInProgress
			|| TestRunEventHandler.IsTestRunInProgress
			|| EditorApplication.isUpdating
			|| AssetDatabase.IsAssetImportWorkerProcess();

		public static event Action EditorReady;

		private static async Task MonitorEditorStatus()
		{
			while ( IsEditorBusy ) { await Task.Yield(); }

			await Task.Delay( 1500 );
			RaiseEvent( EditorReady );
		}

		private static void RaiseEvent( Action eventAction )
		{
			eventAction?.Invoke();
		}
	}
}
