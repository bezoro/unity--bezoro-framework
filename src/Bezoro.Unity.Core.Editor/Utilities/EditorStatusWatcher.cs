using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;

namespace Bezoro.Unity.Core.Editor.Utilities
{
	/// <summary>
	///     Raises an event after compilation, builds, tests, imports, and updates are idle.
	/// </summary>
	[InitializeOnLoad]
	public static class EditorStatusWatcher
	{
		private static CancellationTokenSource? _cts;
		public static event Action?             EditorReady;

		static EditorStatusWatcher()
		{
			_ = MonitorEditorStatus();
		}

		public static bool IsEditorBusy =>
			CompilationEventHandler.IsCompilationInProgress ||
			BuildEventHandler.IsBuildInProgress ||
			TestRunEventHandler.IsTestRunInProgress ||
			EditorApplication.isUpdating ||
			AssetDatabase.IsAssetImportWorkerProcess();

		private static async Task MonitorEditorStatus()
		{
			_cts?.Cancel();
			_cts?.Dispose();
			_cts = new();
			var ct = _cts.Token;

			while (IsEditorBusy && !ct.IsCancellationRequested)
				await Task.Yield();

			try
			{
				await Task.Delay(
					1500,
					ct
				);
			}
			catch (OperationCanceledException) when (ct.IsCancellationRequested)
			{
				return;
			}

			if (ct.IsCancellationRequested) return;

			RaiseEvent(
				EditorReady
			);
		}

		private static void RaiseEvent(Action? eventAction) =>
			eventAction?.Invoke();
	}
}
