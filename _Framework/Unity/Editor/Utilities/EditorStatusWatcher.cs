using System;
using System.Threading.Tasks;
using UnityEditor;

namespace Bezoro.Framework.Unity.Editor.Utilities
{
	[InitializeOnLoad]
	public static class EditorStatusWatcher
	{
		public static event Action EditorReady;

		public static bool IsEditorBusy =>
			CompilationEventHandler.IsCompilationInProgress
		 || BuildEventHandler.IsBuildInProgress
		 || TestRunEventHandler.IsTestRunInProgress
		 || EditorApplication.isUpdating
		 || AssetDatabase.IsAssetImportWorkerProcess();

		static EditorStatusWatcher()
		{
			_ = MonitorEditorStatus();
		}

		private static async Task MonitorEditorStatus()
		{
			while (IsEditorBusy) { await Task.Yield(); }
			await Task.Delay( 1500 );
			RaiseEvent( EditorReady );
		}

		private static void RaiseEvent( Action eventAction )
		{
			eventAction?.Invoke();
		}
	}
}
