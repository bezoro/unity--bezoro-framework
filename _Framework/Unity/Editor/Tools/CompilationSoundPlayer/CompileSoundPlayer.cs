using System;
using System.Reflection;
using Bezoro.Framework.Unity.Editor.Utilities;
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

		public int callbackOrder => 0;

		[InitializeOnLoadMethod]
		private static void Initialize()
		{
			if ( !CompilationSoundSettings.IsAssetInstance() ) { return; }

			_settings = CompilationSoundSettings.instance;
			if ( _settings == null )
			{
				CompilationSoundSettingsWindow.ShowWindow();
				Debug.LogError( "CompilationSoundSettings not found. Please create the asset in Resources folder." );

				return;
			}

			var testRunnerApi = ScriptableObject.CreateInstance<TestRunnerApi>();
			testRunnerApi.RegisterCallbacks( new CompilationSoundPlayer() );

			CompilationPipeline.compilationStarted += _ =>
			{
				_isCompiling = true;
				if ( !_isBuildInProgress && !_isTestRunning ) { PlayClip( _settings.compilationStartClip ); }
			};

			CompilationPipeline.compilationFinished += _ =>
			{
				_isCompiling = false;
				if ( !_isBuildInProgress && !_isTestRunning ) { PlayClip( _settings.compilationEndClip ); }
			};

			AssemblyReloadEvents.afterAssemblyReload += () =>
			{
				if ( !_isCompiling && !_isBuildInProgress )
				{
					EditorApplication.delayCall += () =>
					{
						if ( !EditorUtils.IsEditorBusy() ) { PlayClip( _settings.unityFullyInitializedClip ); }
					};
				}
			};

			BuildPlayerWindow.RegisterBuildPlayerHandler
			(
				buildPlayerOptions =>
				{
					_isBuildInProgress = true;
					BuildPipeline.BuildPlayer( buildPlayerOptions );
				}
			);
		}

		public void RunStarted( ITestAdaptor testsToRun )
		{
			_isTestRunning = true;
			PlayClip( _settings.testStartClip );
		}

		public void RunFinished( ITestResultAdaptor result )
		{
			_isTestRunning = false;
			PlayClip( _settings.testEndClip );
		}

		// Implement other ICallbacks methods
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
			_isBuildInProgress          =  false;
			EditorApplication.delayCall += () => PlayClip( _settings.buildCompletedClip );
		}
	}
}
