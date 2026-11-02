using System;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Bezoro.Unity.Core.Editor.Utilities;

[InitializeOnLoad] [Serializable]
/// <summary>
/// Publishes events when Unity player builds start and finish.
/// </summary>
public sealed class BuildEventHandler :
	IPreprocessBuildWithReport,
	IPostprocessBuildWithReport
{
	public static event Action? BuildCancelled;
	public static event Action? BuildFailed;
	public static event Action? BuildFinishedSuccessfully;
	public static event Action? BuildStarted;

	static BuildEventHandler()
	{
		Initialize();
	}

	private static void Initialize() => IsBuildInProgress = false;

	public static bool IsBuildInProgress { get; private set; }

	public int callbackOrder => 0;

	public void OnPostprocessBuild(BuildReport report)
	{
		IsBuildInProgress = false;
		Debug.Log(
			$"Build completed with result: {report.summary.result}"
		);

		switch (report.summary.result)
		{
			case BuildResult.Failed:
				Debug.Log(
					"Build failed."
				);

				RaiseEvent(
					BuildFailed
				);

				break;

			case BuildResult.Cancelled:
				Debug.Log(
					"Build cancelled."
				);

				RaiseEvent(
					BuildCancelled
				);

				break;

			case BuildResult.Succeeded:
				Debug.Log(
					"Build succeeded."
				);

				RaiseEvent(
					BuildFinishedSuccessfully
				);

				break;
		}
	}

	public void OnPreprocessBuild(BuildReport report)
	{
		IsBuildInProgress = true;
		RaiseEvent(
			BuildStarted
		);
	}

	private static void RaiseEvent(Action? eventAction)
	{
		Debug.Log(
			$"Raised {eventAction}"
		);

		eventAction?.Invoke();
	}
}
