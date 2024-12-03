using System;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Bezoro.Framework.Unity.Editor.Tools
{
	[InitializeOnLoad]
	public class AutoUnityTestRunner
	{
		static AutoUnityTestRunner()
		{
			AssemblyReloadEvents.afterAssemblyReload -= OnAfterAssemblyReload;
			AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;
		}

		public static TestCallbacks Callbacks { get; private set; }

		private static async void OnAfterAssemblyReload()
		{
			if ( !EditorPrefs.GetBool( "AutoRunTests_Enabled" ) )
			{
				return;
			}

			await RunAllTestsAsync();
		}

		public static async Task RunAllTestsAsync()
		{
			await RunTestsByModeAsync( TestMode.EditMode );

			await Task.Delay( 2000 );

			await RunTestsByModeAsync( TestMode.PlayMode );
		}

		private static Task<bool> RunTestsByModeAsync( TestMode testMode )
		{
			var testRunnerApi = ScriptableObject.CreateInstance<TestRunnerApi>();
			var tcs           = new TaskCompletionSource<bool>();
			Callbacks = new();

			Callbacks.TestsRunFinished += () => tcs.SetResult( true );
			Callbacks.TestsRunFinished += () => testRunnerApi.UnregisterCallbacks( Callbacks );
			Callbacks.TestsRunFinished += () => Object.DestroyImmediate( testRunnerApi );

			var filter = new Filter
			{
				testMode = testMode
			};

			testRunnerApi.RegisterCallbacks( Callbacks );
			testRunnerApi.Execute( new( filter ) );

			return tcs.Task;
		}

		public class TestCallbacks : ICallbacks,
			IDisposable
		{
			private readonly TaskCompletionSource<bool> _tcs;

			public void RunStarted( ITestAdaptor testsToRun )
			{
				TestsRunStarted?.Invoke();
				Debug.Log( $"{testsToRun.TestMode} tests started." );
			}

			public void RunFinished( ITestResultAdaptor result )
			{
				Debug.Log( $"{result.Test.TestMode} tests finished." );

				if ( result.HasChildren && result.TestStatus == TestStatus.Passed )
				{
					Debug.Log( $"All {result.Test.TestMode} tests passed successfully." );
				}
				else
				{
					Debug.LogWarning
					(
						$"Some {result.Test.TestMode} tests failed. Check the Test Runner window for details."
					);
				}

				TestsRunFinished?.Invoke();
			}

			public void TestStarted( ITestAdaptor test )
			{
				TestStartedEvent?.Invoke();
			}

			public void TestFinished( ITestResultAdaptor result )
			{
				TestFinishedEvent?.Invoke();
			}

			public void Dispose()
			{
				TestsRunStarted   = null;
				TestsRunFinished  = null;
				TestStartedEvent  = null;
				TestFinishedEvent = null;
			}

			public event Action TestsRunStarted;
			public event Action TestsRunFinished;

			public event Action TestStartedEvent;
			public event Action TestFinishedEvent;
		}
	}
}
