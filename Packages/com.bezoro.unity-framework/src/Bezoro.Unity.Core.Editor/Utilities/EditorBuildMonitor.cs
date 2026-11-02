using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Bezoro.Logging;
using Bezoro.Logging.Types;
using UnityEditor;
using UnityEditor.Build.Reporting;

namespace Bezoro.Unity.Core.Editor.Utilities;

[InitializeOnLoad]
/// <summary>
/// Watches player builds and reports result, duration, warning, and error counts.
/// </summary>
public static class EditorBuildMonitor
{
	private static CancellationTokenSource? _cts;
	private static DateTime                 _buildStartTime;
	private static int                      _errorCountAtStart;

	public static event Action<BuildResult>?       BuildCompleted;
	public static event Action<CustomBuildReport>? BuildCompletedWithReport;
	public static event Action?                    BuildStarted;

	static EditorBuildMonitor()
	{
		StartMonitoring();
	}

	private static int GetConsoleErrorCount()
	{
		try
		{
			var consoleWindowType = typeof(EditorWindow).Assembly.GetType(
				"UnityEditor.ConsoleWindow"
			);

			var getCountsByType = consoleWindowType.GetMethod(
				"GetCountsByType",
				BindingFlags.Static | BindingFlags.NonPublic
			);

			if (getCountsByType != null)
			{
				var counts = (int[])getCountsByType.Invoke(
					null,
					null
				);

				return counts is { Length: > 2 } ? counts[2] : 0;
			}
		}
		catch (Exception ex)
		{
			Logger.LogWarning(
				$"Could not get console error count: {ex.Message}",
				contextObject: nameof(EditorBuildMonitor),
				category:
				LogCategory.Editor
			);
		}

		return 0;
	}

	private static int GetConsoleWarningCount()
	{
		try
		{
			var consoleWindowType = typeof(EditorWindow).Assembly.GetType(
				"UnityEditor.ConsoleWindow"
			);

			var getCountsByType = consoleWindowType.GetMethod(
				"GetCountsByType",
				BindingFlags.Static | BindingFlags.NonPublic
			);

			if (getCountsByType != null)
			{
				var counts = (int[])getCountsByType.Invoke(
					null,
					null
				);

				return counts != null && counts.Length > 1 ? counts[1] : 0;
			}
		}
		catch (Exception ex)
		{
			Logger.LogWarning(
				$"Could not get console warning count: {ex.Message}",
				contextObject: nameof(EditorBuildMonitor),
				category:
				LogCategory.Editor
			);
		}

		return 0;
	}

	private static string GetLastBuildPath()
	{
		try
		{
			return EditorUserBuildSettings.GetBuildLocation(
				EditorUserBuildSettings.activeBuildTarget
			);
		}
		catch
		{
			return "Unknown";
		}
	}

	private static async Task HandleBuildCompletion(CancellationToken token)
	{
		await Task.Delay(
			500,
			token
		);

		int currentErrorCount = GetConsoleErrorCount();
		int newErrors         = currentErrorCount - _errorCountAtStart;

		var result        = newErrors > 0 ? BuildResult.Failed : BuildResult.Succeeded;
		var buildDuration = DateTime.Now - _buildStartTime;

		BuildCompleted?.Invoke(
			result
		);

		var customReport = new CustomBuildReport(
			result,
			buildDuration,
			DateTime.Now,
			newErrors,
			GetConsoleWarningCount(),
			GetLastBuildPath()
		);

		BuildCompletedWithReport?.Invoke(
			customReport
		);
	}

	private static async void StartMonitoring()
	{
		_cts?.Cancel();
		_cts = new();

		AssemblyReloadEvents.beforeAssemblyReload += () =>
		{
			_cts?.Cancel();
			_cts?.Dispose();
			_cts = null;
		};

		var wasBuildingLastCheck = false;
		var token                = _cts.Token;

		try
		{
			while (!token.IsCancellationRequested)
			{
				await Task.Delay(
					100,
					token
				);

				if (EditorApplication.isPlaying) continue;

				bool isBuildingNow = BuildPipeline.isBuildingPlayer;

				if (isBuildingNow && !wasBuildingLastCheck)
				{
					_buildStartTime    = DateTime.Now;
					_errorCountAtStart = GetConsoleErrorCount();
					BuildStarted?.Invoke();
				}
				else if (!isBuildingNow && wasBuildingLastCheck)
				{
					await HandleBuildCompletion(
						token
					);
				}

				wasBuildingLastCheck = isBuildingNow;
			}
		}
		catch (OperationCanceledException)
		{
			/* expected on domain reload */
		}
		catch (Exception ex)
		{
			Logger.LogWarning(
				$"{nameof(EditorBuildMonitor)} stopped after an unexpected error: {ex}",
				contextObject: nameof(EditorBuildMonitor),
				category: LogCategory.Editor
			);
		}
	}

	[Serializable]
	/// <summary>
	/// Captures build result metadata emitted when a monitored build finishes.
	/// </summary>
	public class CustomBuildReport
	{
		public BuildResult result;
		public int         errorCount;
		public int         warningCount;
		public string      buildPath;
		public DateTime    BuildTime;
		public TimeSpan    TotalTime;

		public CustomBuildReport(
			BuildResult result,
			TimeSpan    totalTime,
			DateTime    buildTime,
			int         errorCount   = 0,
			int         warningCount = 0,
			string      buildPath    = "")
		{
			this.result       = result;
			TotalTime         = totalTime;
			BuildTime         = buildTime;
			this.errorCount   = errorCount;
			this.warningCount = warningCount;
			this.buildPath    = buildPath;
		}
	}
	// CustomBuildReport definition – unchanged
}
