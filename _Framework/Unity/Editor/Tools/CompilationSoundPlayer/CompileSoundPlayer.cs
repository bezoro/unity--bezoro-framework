using System;
using System.Reflection;
using Bezoro.Framework.Unity.Editor.Utilities;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Compilation;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace Bezoro.Framework.Unity.Editor.Tools
{
	internal class CompilationSoundPlayer : IPostprocessBuildWithReport, ICallbacks
	{
		private static CompilationSoundSettings _settings;

		private static bool _isCompiling;
		private static bool _isBuildInProgress;
		private static bool _isTestRunning;

		private static bool _shouldPlayCompilationStartClip;
		private static bool _shouldPlayCompilationEndClip;
		private static bool _shouldPlayUnityFullyInitializedClip;
		private static bool _shouldPlayTestStartClip;
		private static bool _shouldPlayTestEndClip;
		private static bool _shouldPlayBuildCompletedClip;

		public int callbackOrder => 0;

		[InitializeOnLoadMethod]
		private static void Initialize()
		{
			InitializeCompilationSoundSettings().Forget();
			EditorApplication.update += Update;
		}

		private static async UniTask InitializeCompilationSoundSettings()
		{
			// if ( !CompilationSoundSettings.IsAssetInstance() ) { return; }

			// _settings = CompilationSoundSettings.instance;
			// if ( _settings == null )
			// {
			// CompilationSoundSettingsWindow.ShowWindow();
			// Debug.LogError( "CompilationSoundSettings not found. Please create the asset in Resources folder." );

			// return;
			// }

			var testRunnerApi = ScriptableObject.CreateInstance<TestRunnerApi>();
			testRunnerApi.RegisterCallbacks( new CompilationSoundPlayer() );

			CompilationPipeline.compilationStarted += _ =>
			{
				_isCompiling                    = true;
				_shouldPlayCompilationStartClip = !_isBuildInProgress && !_isTestRunning;
			};

			CompilationPipeline.compilationFinished += _ =>
			{
				_isCompiling                  = false;
				_shouldPlayCompilationEndClip = !_isBuildInProgress && !_isTestRunning;
			};

			AssemblyReloadEvents.afterAssemblyReload += () =>
			{
				if ( !_isCompiling && !_isBuildInProgress )
				{
					EditorApplication.delayCall += () =>
					{
						if ( !EditorUtils.IsEditorBusy() ) { _shouldPlayUnityFullyInitializedClip = true; }
					};
				}
			};

			BuildPlayerWindow.RegisterBuildPlayerHandler(
				buildPlayerOptions =>
				{
					_isBuildInProgress = true;
					BuildPipeline.BuildPlayer( buildPlayerOptions );
				}
			);
		}

		private static async void Update()
		{
			await UniTask.WaitUntil( () => CompilationSoundSettings.Initialized );

			if ( !CompilationSoundSettings.IsAssetInstance() )
			{
				return;
			}

			_settings = CompilationSoundSettings.instance;

			if ( _shouldPlayCompilationStartClip )
			{
				_shouldPlayCompilationStartClip = false;
				PlayClip( _settings.compilationStartClip );
			}

			if ( _shouldPlayCompilationEndClip )
			{
				_shouldPlayCompilationEndClip = false;
				PlayClip( _settings.compilationEndClip );
			}

			if ( _shouldPlayUnityFullyInitializedClip )
			{
				_shouldPlayUnityFullyInitializedClip = false;
				PlayClip( _settings.unityFullyInitializedClip );
			}

			if ( _shouldPlayTestStartClip )
			{
				_shouldPlayTestStartClip = false;
				PlayClip( _settings.testStartClip );
			}

			if ( _shouldPlayTestEndClip )
			{
				_shouldPlayTestEndClip = false;
				PlayClip( _settings.testEndClip );
			}

			if ( _shouldPlayBuildCompletedClip )
			{
				_shouldPlayBuildCompletedClip = false;
				PlayClip( _settings.buildCompletedClip );
			}
		}

		public void RunStarted( ITestAdaptor testsToRun )
		{
			_isTestRunning           = true;
			_shouldPlayTestStartClip = true;
		}

		public void RunFinished( ITestResultAdaptor result )
		{
			_isTestRunning         = false;
			_shouldPlayTestEndClip = true;
		}

		public void TestStarted( ITestAdaptor test )          {}
		public void TestFinished( ITestResultAdaptor result ) {}

		private static void PlayClip( AudioClip clip )
		{
			if ( clip == null ) { return; }

			try
			{
				var unityEditorAssembly = typeof(AudioImporter).Assembly;
				var audioUtilClass = unityEditorAssembly.GetType( "UnityEditor.AudioUtil" );
				var method = audioUtilClass?.GetMethod(
					"PlayPreviewClip",
					BindingFlags.Static | BindingFlags.Public,
					null,
					new[] { typeof(AudioClip), typeof(int), typeof(bool) },
					null
				);

				method?.Invoke( null, new object[] { clip, 0, false } );
			}
			catch ( Exception e )
			{
				Debug.LogError( "Error playing compilation sound: " + e.Message );
			}
		}

		public void OnPostprocessBuild( BuildReport report )
		{
			_isBuildInProgress            = false;
			_shouldPlayBuildCompletedClip = true;
		}
	}
}
