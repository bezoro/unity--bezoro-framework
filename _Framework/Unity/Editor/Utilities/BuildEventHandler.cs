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
		public static event Action BuildFinished;

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
			RaiseEvent( BuildFinished );
		}

		private static void RaiseEvent( Action eventAction )
		{
			eventAction?.Invoke();
		}

		public int callbackOrder => 0;
	}
}
