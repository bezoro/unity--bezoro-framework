using System;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Bezoro.Unity.Core.Editor.Tools.AutoUnityTestRunner
{
	[InitializeOnLoad]
	/// <summary>
	/// Runs EditMode and PlayMode tests automatically after an assembly reload when enabled.
	/// </summary>
	public class AutoUnityTestRunner
	{
		public static event Action? TestsRunFinished;
		public static event Action? TestsRunStarted;

		static AutoUnityTestRunner()
		{
			AssemblyReloadEvents.afterAssemblyReload -= OnAfterAssemblyReload;
			AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;
		}

		public static TestCallbacks Callbacks { get; private set; } = null!;

		public static async Task RunAllTestsAsync()
		{
			await RunTestsByModeAsync(
				TestMode.EditMode
			);

			await Task.Delay(
				2000
			);

			await RunTestsByModeAsync(
				TestMode.PlayMode
			);
		}

		private static Task<bool> RunTestsByModeAsync(TestMode testMode)
		{
			var testRunnerApi = ScriptableObject.CreateInstance<TestRunnerApi>();
			var tcs           = new TaskCompletionSource<bool>();
			Callbacks = new();

			Callbacks.TestsRunStarted  += () => TestsRunStarted?.Invoke();
			Callbacks.TestsRunFinished += () => TestsRunFinished?.Invoke();
			Callbacks.TestsRunFinished += () => tcs.SetResult(
				true
			);

			Callbacks.TestsRunFinished += () => testRunnerApi.UnregisterCallbacks(
				Callbacks
			);

			Callbacks.TestsRunFinished += () => Object.DestroyImmediate(
				testRunnerApi
			);

			var filter = new Filter
			{
				testMode = testMode
			};

			testRunnerApi.RegisterCallbacks(
				Callbacks
			);

			testRunnerApi.Execute(
				new(
					filter
				)
			);

			return tcs.Task;
		}

		private static async void OnAfterAssemblyReload()
		{
			if (!EditorPrefs.GetBool(
					"AutoRunTests_Enabled"
				)) return;

			try
			{
				await RunAllTestsAsync();
			}
			catch (Exception ex)
			{
				Debug.LogError(
					$"Auto test run failed after assembly reload: {ex}"
				);
			}
		}

		/// <summary>
		///     Bridges Unity Test Runner callbacks into events awaited by the auto-runner.
		/// </summary>
		public class TestCallbacks : ICallbacks,
			IDisposable
		{
			public event Action? TestFinishedEvent;
			public event Action? TestsRunFinished;
			public event Action? TestsRunStarted;
			public event Action? TestStartedEvent;

			public void Dispose()
			{
				TestsRunStarted   = null;
				TestsRunFinished  = null;
				TestStartedEvent  = null;
				TestFinishedEvent = null;
			}

			#region Interface Implementations

			public void RunStarted(ITestAdaptor testsToRun)
			{
				TestsRunStarted?.Invoke();
				Debug.Log(
					$"{testsToRun.TestMode} tests started."
				);
			}

			public void RunFinished(ITestResultAdaptor result)
			{
				Debug.Log(
					$"{result.Test.TestMode} tests finished."
				);

				if (result.HasChildren && result.TestStatus == TestStatus.Passed)
					Debug.Log(
						$"All {result.Test.TestMode} tests passed successfully."
					);
				else
					Debug.LogWarning
					(
						$"Some {result.Test.TestMode} tests failed. Check the Test Runner window for details."
					);

				TestsRunFinished?.Invoke();
			}

			public void TestStarted(ITestAdaptor test) =>
				TestStartedEvent?.Invoke();

			public void TestFinished(ITestResultAdaptor result) =>
				TestFinishedEvent?.Invoke();

			#endregion
		}
	}
}
