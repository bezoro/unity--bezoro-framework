using System;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Bezoro.Framework.Unity.Editor.Utilities
{
	[InitializeOnLoad]
	public sealed class BuildEventHandler :
		IPreprocessBuildWithReport, IPostprocessBuildWithReport
	{
		public static event Action BuildStarted;
		public static event Action BuildFinishedSuccessfully;
		public static event Action BuildFailed;
		public static event Action BuildCancelled;

		public static bool IsBuildInProgress { get; private set; }

		static BuildEventHandler()
		{
			Initialize();
		}

		public static void Initialize()
		{
			IsBuildInProgress = false;
		}

		public void OnPreprocessBuild( BuildReport report )
		{
			IsBuildInProgress = true;
			RaiseEvent( BuildStarted );
		}

		public void OnPostprocessBuild( BuildReport report )
		{
			IsBuildInProgress = false;
			switch ( report.summary.result )
			{
				case BuildResult.Failed:    RaiseEvent( BuildFailed ); break;
				case BuildResult.Cancelled: RaiseEvent( BuildCancelled ); break;
				case BuildResult.Succeeded: RaiseEvent( BuildFinishedSuccessfully ); break;
			}
		}

		private static void RaiseEvent( Action eventAction )
		{
			eventAction?.Invoke();
		}

		public int callbackOrder => 0;
	}
}
