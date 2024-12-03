using System;
using UnityEditor;
using UnityEditor.Compilation;

namespace Bezoro.Framework.Unity.Editor.Utilities
{
	[InitializeOnLoad]
	public sealed class CompilationEventHandler
	{
		static CompilationEventHandler()
		{
			CompilationPipeline.compilationStarted  += OnCompilationStarted;
			CompilationPipeline.compilationFinished += OnCompilationFinished;
		}

		public static bool         IsCompilationInProgress => EditorApplication.isCompiling;
		public static event Action CompilationStarted;
		public static event Action CompilationFinished;

		private static void OnCompilationStarted( object context )
		{
			if ( BuildEventHandler.IsBuildInProgress ) { return; }

			if ( TestRunEventHandler.IsTestRunInProgress ) { return; }

			RaiseEvent( CompilationStarted );
		}

		private static void OnCompilationFinished( object context )
		{
			if ( BuildEventHandler.IsBuildInProgress ) { return; }

			if ( TestRunEventHandler.IsTestRunInProgress ) { return; }

			RaiseEvent( CompilationFinished );
		}

		private static void RaiseEvent( Action eventAction )
		{
			eventAction?.Invoke();
		}
	}
}
