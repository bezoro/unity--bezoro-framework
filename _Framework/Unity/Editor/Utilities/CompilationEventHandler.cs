using System;
using UnityEditor;
using UnityEditor.Compilation;

namespace Bezoro.Framework.Unity.Editor.Utilities
{
	[InitializeOnLoad]
	public sealed class CompilationEventHandler
	{
		public static event Action CompilationStarted;
		public static event Action CompilationFinished;

		public static bool IsCompilationInProgress => EditorApplication.isCompiling;

		static CompilationEventHandler()
		{
			CompilationPipeline.compilationStarted  += OnCompilationStarted;
			CompilationPipeline.compilationFinished += OnCompilationFinished;
		}

		private static void OnCompilationStarted( object context )
		{
			RaiseEvent( CompilationStarted );
		}

		private static void OnCompilationFinished( object context )
		{
			RaiseEvent( CompilationFinished );
		}

		private static void RaiseEvent( Action eventAction )
		{
			eventAction?.Invoke();
		}
	}
}
