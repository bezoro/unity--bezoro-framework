using System;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Bezoro.Framework.Unity.Editor.Utilities
{
	[InitializeOnLoad]
	[Serializable]
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
			Debug.Log( $"Build completed with result: {report.summary.result}" );
			switch ( report.summary.result )
			{
				case BuildResult.Failed:
					Debug.Log( "Build failed." );
					RaiseEvent( BuildFailed );
					break;
				case BuildResult.Cancelled:
					Debug.Log( "Build cancelled." );
					RaiseEvent( BuildCancelled );
					break;
				case BuildResult.Succeeded:
					Debug.Log( "Build succeeded." );
					RaiseEvent( BuildFinishedSuccessfully );
					break;
			}
		}

		private static void RaiseEvent( Action eventAction )
		{
			Debug.Log( $"Raised {eventAction}" );
			eventAction?.Invoke();
		}

		public int callbackOrder => 0;
	}
}
