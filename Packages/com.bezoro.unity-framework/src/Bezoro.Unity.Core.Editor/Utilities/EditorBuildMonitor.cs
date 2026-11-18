using System;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Bezoro.Unity.Core.Editor.Utilities;

/// <summary>
/// Publishes the player build report once the pipeline has stopped.
/// </summary>
public static class EditorBuildMonitor
{
	private static BuildReport? _currentReport;
	private static DateTime _startedAt;
	private static string _outputPath = string.Empty;
	private static bool _awaitingCompletion;

	public static event Action<BuildResult>? BuildCompleted;
	public static event Action<CustomBuildReport>? BuildCompletedWithReport;
	public static event Action? BuildStarted;

	internal sealed class BuildCallbacks : IPreprocessBuildWithReport, IPostprocessBuildWithReport
	{
		public int callbackOrder => int.MinValue;

		public void OnPreprocessBuild(BuildReport report)
		{
			if (_awaitingCompletion)
				PublishCompletion();

			_currentReport = report;
			_startedAt = report.summary.buildStartedAt;
			_outputPath = report.summary.outputPath;
			_awaitingCompletion = true;
			EditorApplication.update += OnEditorUpdate;
			BuildStarted?.Invoke();
		}

		public void OnPostprocessBuild(BuildReport report) => _currentReport = report;
	}

	private static void OnEditorUpdate()
	{
		if (!BuildPipeline.isBuildingPlayer)
			PublishCompletion();
	}

	private static void PublishCompletion()
	{
		EditorApplication.update -= OnEditorUpdate;
		_awaitingCompletion = false;
		var report = _currentReport;
		_currentReport = null;
		if (report == null || !IsFinalResult(report.summary.result))
		{
			var latest = BuildReport.GetLatestReport();
			if (latest != null && latest.summary.buildStartedAt == _startedAt &&
				latest.summary.outputPath == _outputPath)
				report = latest;
		}

		var details = CreateCompletionReport(report != null ? report.summary : null, _startedAt, _outputPath);
		BuildCompleted?.Invoke(details.result);
		BuildCompletedWithReport?.Invoke(details);
	}

	private static bool IsFinalResult(BuildResult result) =>
		result == BuildResult.Succeeded || result == BuildResult.Failed || result == BuildResult.Cancelled;

	internal static CustomBuildReport CreateCompletionReport(BuildSummary? summary, DateTime startedAt, string outputPath)
	{
		if (summary is { } completed && IsFinalResult(completed.result))
			return new CustomBuildReport(completed.result, completed.totalTime, completed.buildEndedAt,
				completed.totalErrors, completed.totalWarnings, completed.outputPath);

		var now = DateTime.UtcNow;
		return new CustomBuildReport(BuildResult.Unknown, now - startedAt, now, buildPath: outputPath);
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
}
