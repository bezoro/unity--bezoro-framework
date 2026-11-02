using System;
using System.Threading;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace Bezoro.Unity.Core.Editor.Utilities
{
	[InitializeOnLoad]
	/// <summary>
	/// Publishes events when Unity Test Runner sessions start and finish.
	/// </summary>
	public sealed class TestRunEventHandler : ICallbacks
	{
		private static int _isTestRunInProgress;
		private static TestRunEventHandler? _callbacks;
		private static TestRunnerApi?       _testRunnerApi;

		public static event Action? TestRunFinished;
		public static event Action? TestRunStarted;

		static TestRunEventHandler()
		{
			Initialize();
		}

		private static void Initialize()
		{
			IsTestRunInProgress = false;

			RegisterCallbacks();

			AssemblyReloadEvents.beforeAssemblyReload -= UnregisterCallbacks;
			AssemblyReloadEvents.beforeAssemblyReload += UnregisterCallbacks;
			EditorApplication.quitting                -= UnregisterCallbacks;
			EditorApplication.quitting                += UnregisterCallbacks;
		}

		public static bool IsTestRunInProgress
		{
			get => Interlocked.CompareExchange(
					   ref _isTestRunInProgress,
					   0,
					   0
				   ) ==
				   1;
			private set => Interlocked.Exchange(
				ref _isTestRunInProgress,
				value ? 1 : 0
			);
		}

		public void RunFinished(ITestResultAdaptor result)
		{
			IsTestRunInProgress = false;
			RaiseEvent(
				TestRunFinished
			);
		}

		public void RunStarted(ITestAdaptor testsToRun)
		{
			IsTestRunInProgress = true;
			RaiseEvent(
				TestRunStarted
			);
		}

		public void TestFinished(ITestResultAdaptor result) { }

		public void TestStarted(ITestAdaptor test) { }

		private static void RegisterCallbacks()
		{
			UnregisterCallbacks();

			_callbacks     = new TestRunEventHandler();
			_testRunnerApi = ScriptableObject.CreateInstance<TestRunnerApi>();
			_testRunnerApi.hideFlags = HideFlags.HideAndDontSave;
			_testRunnerApi.RegisterCallbacks(
				_callbacks
			);
		}

		private static void RaiseEvent(Action? eventAction) => eventAction?.Invoke();

		private static void UnregisterCallbacks()
		{
			if (_testRunnerApi != null && _callbacks != null)
				_testRunnerApi.UnregisterCallbacks(
					_callbacks
				);

			if (_testRunnerApi != null)
				UnityEngine.Object.DestroyImmediate(
					_testRunnerApi
				);

			_testRunnerApi = null;
			_callbacks     = null;
		}
	}
}
