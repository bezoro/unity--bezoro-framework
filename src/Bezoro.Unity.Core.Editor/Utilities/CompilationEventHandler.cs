using System;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace Bezoro.Unity.Core.Editor.Utilities
{
	[InitializeOnLoad]
	[DefaultExecutionOrder(
		int.MinValue
	)]
	/// <summary>
	/// Publishes events when Unity script compilation starts and finishes.
	/// </summary>
	public sealed class CompilationEventHandler
	{
		public static event Action? CompilationFinished;
		public static event Action? CompilationStarted;

		static CompilationEventHandler()
		{
			CompilationPipeline.compilationStarted -= OnCompilationStarted;
			CompilationPipeline.compilationStarted += OnCompilationStarted;

			CompilationPipeline.compilationFinished -= OnCompilationFinished;
			CompilationPipeline.compilationFinished += OnCompilationFinished;
		}

		public static bool IsCompilationInProgress => EditorApplication.isCompiling;

		private static void OnCompilationFinished(object context)
		{
			if (BuildEventHandler.IsBuildInProgress) return;

			if (TestRunEventHandler.IsTestRunInProgress) return;

			RaiseEvent(
				CompilationFinished
			);
		}

		private static void OnCompilationStarted(object context)
		{
			if (BuildEventHandler.IsBuildInProgress) return;

			if (TestRunEventHandler.IsTestRunInProgress) return;

			RaiseEvent(
				CompilationStarted
			);
		}

		private static void RaiseEvent(Action? eventAction) =>
			eventAction?.Invoke();
	}
}
