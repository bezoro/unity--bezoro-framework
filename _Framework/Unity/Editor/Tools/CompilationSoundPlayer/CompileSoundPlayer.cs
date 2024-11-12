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
	[InitializeOnLoad]
	internal class CompilationSoundPlayer :
		IPostprocessBuildWithReport, ICallbacks
	{
		private static readonly CompilationSoundSettings Settings = CompilationSoundSettings.instance;

		// private static readonly AudioClip UnityFullyInitializedClip;
		// private static readonly AudioClip BuildCompletedClip;
		// private static readonly AudioClip CompilationEndClip;
		// private static readonly AudioClip CompilationStartClip;
		// private static readonly AudioClip TestStartClip;
		// private static readonly AudioClip TestEndClip;
		// private static readonly bool Initialized;

		private static bool _isCompiling;
		private static bool _isBuildInProgress;
		private static bool _isTestRunning;

		public int callbackOrder => 0;

		static CompilationSoundPlayer()
		{
			if ( Settings == null )
			{
				CompilationSoundSettingsWindow.ShowWindow();
				Debug.LogError( "CompilationSoundSettings not found. Please create the asset in Resources folder." );

				return;
			}

			// CompilationStartClip      = Resources.Load<AudioClip>( "SFX_StartCompilation" );
			// CompilationEndClip        = Resources.Load<AudioClip>( "SFX_EndCompilation" );
			// UnityFullyInitializedClip = Resources.Load<AudioClip>( "SFX_IdleReady" );
			// BuildCompletedClip        = Resources.Load<AudioClip>( "SFX_BuildCompleted" );
			// TestStartClip             = Resources.Load<AudioClip>( "SFX_TestStart" );
			// TestEndClip               = Resources.Load<AudioClip>( "SFX_TestEnd" );

			var testRunnerApi = ScriptableObject.CreateInstance<TestRunnerApi>();
			testRunnerApi.RegisterCallbacks( new CompilationSoundPlayer() );

			CompilationPipeline.compilationStarted += _ =>
			{
				_isCompiling = true;
				if ( !_isBuildInProgress && !_isTestRunning ) { PlayClip( Settings.compilationStartClip ); }
			};

			CompilationPipeline.compilationFinished += _ =>
			{
				_isCompiling = false;
				if ( !_isBuildInProgress && !_isTestRunning ) { PlayClip( Settings.compilationEndClip ); }
			};

			AssemblyReloadEvents.afterAssemblyReload += () =>
			{
				if ( !_isCompiling && !_isBuildInProgress )
				{
					EditorApplication.delayCall += () =>
					{
						if ( !EditorUtils.IsEditorBusy() ) { PlayClip( Settings.unityFullyInitializedClip ); }
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

			// Initialized = true;
		}

		public void RunStarted( ITestAdaptor testsToRun )
		{
			_isTestRunning = true;
			PlayClip( Settings.testStartClip );
		}

		public void RunFinished( ITestResultAdaptor result )
		{
			_isTestRunning = false;
			PlayClip( Settings.testEndClip );
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
			EditorApplication.delayCall += () => PlayClip( Settings.buildCompletedClip );
		}
	}
}
