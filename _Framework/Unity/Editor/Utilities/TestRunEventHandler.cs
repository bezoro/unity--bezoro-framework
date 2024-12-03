using System;
using System.Threading;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;

namespace Bezoro.Framework.Unity.Editor.Utilities
{
	[InitializeOnLoad]
	public sealed class TestRunEventHandler : ICallbacks
	{
		private static int _isTestRunInProgress;

		static TestRunEventHandler()
		{
			Initialize();
		}

		public static bool IsTestRunInProgress
		{
			get => Interlocked.CompareExchange(
				ref _isTestRunInProgress,
				0,
				0 ) == 1;
			private set => Interlocked.Exchange(
				ref _isTestRunInProgress,
				value ? 1 : 0 );
		}

		public void RunStarted( ITestAdaptor testsToRun )
		{
			IsTestRunInProgress = true;
			RaiseEvent( TestRunStarted );
		}

		public void RunFinished( ITestResultAdaptor result )
		{
			IsTestRunInProgress = false;
			RaiseEvent( TestRunFinished );
		}

		public void TestStarted( ITestAdaptor test )
		{
		}

		public void TestFinished( ITestResultAdaptor result )
		{
		}

		public static event Action TestRunStarted;
		public static event Action TestRunFinished;

		public static void Initialize()
		{
			IsTestRunInProgress = false;
		}

		private static void RaiseEvent( Action eventAction )
		{
			eventAction?.Invoke();
		}
	}
}
