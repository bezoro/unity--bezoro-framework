using System;
using System.Reflection;
using System.Threading;
using Bezoro.Framework.Core;
using Bezoro.Framework.Unity.Utilities;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Compilation;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace Bezoro.Framework.Unity.Editor.Utilities
{
	/// <summary>
	///     Utility class for managing various Unity editor events such as test runs, compilation, assembly reloads, asset
	///     imports, and builds.
	///     Ensures thread safety and proper initialization/cleanup of event subscriptions.
	/// </summary>
	[InitializeOnLoad]
	public class EditorUtils :
		ICallbacks,
		IPreprocessBuildWithReport, IPostprocessBuildWithReport
	{
		static EditorUtils()
		{
			Initialize();
		}

		~EditorUtils()
		{
			Cleanup();
		}

	#region Events

		public static event Action TestRunStarted;
		public static event Action TestRunFinished;
		public static event Action CompilationStarted;
		public static event Action CompilationFinished;
		public static event Action AssemblyReloadStarted;
		public static event Action AssemblyReloadFinished;
		public static event Action AssetImportStarted;
		public static event Action AssetImportFinished;
		public static event Action BuildStarted;
		public static event Action BuildFinished;

	#endregion

	#region State Flags

		private static int _isTestRunInProgress;

		public static bool IsTestRunInProgress
		{
			get => Interlocked.CompareExchange( ref _isTestRunInProgress, 0, 0 ) == 1;
			private set => Interlocked.Exchange( ref _isTestRunInProgress, value ? 1 : 0 );
		}

		private static int _isDomainReloadInProgress;

		public static bool IsDomainReloadInProgress
		{
			get => Interlocked.CompareExchange( ref _isDomainReloadInProgress, 0, 0 ) == 1;
			private set => Interlocked.Exchange( ref _isDomainReloadInProgress, value ? 1 : 0 );
		}

		private static int _isBuildInProgress;

		public static bool IsBuildInProgress
		{
			get => Interlocked.CompareExchange( ref _isBuildInProgress, 0, 0 ) == 1;
			private set => Interlocked.Exchange( ref _isBuildInProgress, value ? 1 : 0 );
		}

		public static bool IsCompilationInProgress => EditorApplication.isCompiling;
		public static bool IsAssetImportInProgress => AssetDatabase.IsAssetImportWorkerProcess();
		public static bool IsUpdating => EditorApplication.isUpdating;

		/// <summary>
		///     Checks if the Unity editor is busy with any process such as compilation, asset import, build, etc.
		/// </summary>
		public static bool IsEditorBusy() =>
			IsCompilationInProgress
		 || IsUpdating
		 || IsAssetImportInProgress
		 || IsBuildInProgress
		 || IsTestRunInProgress
		 || IsDomainReloadInProgress;

		public int callbackOrder => 0;

	#endregion

	#region Initialization and Cleanup

		/// <summary>
		///     Initializes the EditorUtils class.
		///     Subscribes to necessary compilation pipeline and assembly reload events.
		/// </summary>
		private static void Initialize()
		{
			IsTestRunInProgress      = false;
			IsDomainReloadInProgress = false;
			IsBuildInProgress        = false;

			CompilationPipeline.compilationStarted   += OnCompilationStarted;
			CompilationPipeline.compilationFinished  += OnCompilationFinished;
			AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;
		}

		/// <summary>
		///     Cleans up event subscriptions when the instance is destroyed.
		/// </summary>
		private static void Cleanup()
		{
			CompilationPipeline.compilationStarted   -= OnCompilationStarted;
			CompilationPipeline.compilationFinished  -= OnCompilationFinished;
			AssemblyReloadEvents.afterAssemblyReload -= OnAfterAssemblyReload;
		}

	#endregion

	#region Event Handlers

		private static void OnBeforeAssemblyReload()
		{
			IsDomainReloadInProgress = true;
			RaiseEvent( AssemblyReloadStarted );
		}

		private static void OnAfterAssemblyReload()
		{
			if ( IsProcessing() )
			{
				return;
			}

			IsDomainReloadInProgress = false;
			RaiseEvent( AssemblyReloadFinished );
		}

		private static bool IsProcessing() => IsCompilationInProgress || IsBuildInProgress;

		private static void TryRaiseAssetImporterEvents()
		{
			if ( AssetDatabase.IsAssetImportWorkerProcess() )
			{
				if ( !IsAssetImportInProgress )
				{
					RaiseEvent( AssetImportStarted );
				}
			}
			else if ( IsAssetImportInProgress )
			{
				RaiseEvent( AssetImportFinished );
			}
		}

		private static void OnCompilationFinished( object context )
		{
			if ( IsBuildInProgress || IsTestRunInProgress )
			{
				return;
			}

			RaiseEvent( CompilationFinished );
		}

		private static void OnCompilationStarted( object context )
		{
			if ( IsBuildInProgress || IsTestRunInProgress )
			{
				return;
			}

			RaiseEvent( CompilationStarted );
		}

	#endregion

	#region Event Raising

		/// <summary>
		///     Safely invokes the specified event if it is not null.
		/// </summary>
		private static void RaiseEvent( Action eventAction )
		{
			eventAction?.Invoke();
		}

	#endregion

	#region ICallbacks Implementation

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

		public void TestStarted( ITestAdaptor test ) {}

		public void TestFinished( ITestResultAdaptor result ) {}

	#endregion

	#region IPreprocessBuildWithReport and IPostprocessBuildWithReport Implementation

		public void OnPreprocessBuild( BuildReport report )
		{
			IsBuildInProgress = true;
			RaiseEvent( BuildStarted );
		}

		public void OnPostprocessBuild( BuildReport report )
		{
			IsBuildInProgress = false;
			RaiseEvent( BuildFinished );
		}

	#endregion

	#region Audio Clip Playback

		/// <summary>
		///     Plays a given AudioClip in the Unity Editor.
		/// </summary>
		/// <param name="clip">The AudioClip to be played.</param>
		public static void PlayClip( AudioClip clip )
		{
			Assert.IsNotNull( clip );
			try
			{
				StopAllPreviewClips();
				InvokePlayPreviewClip( clip );
			}
			catch ( Exception ex )
			{
				BDebug.LogError( $"Error playing compilation sound: {ex.Message}" );
			}
		}

		private static void StopAllPreviewClips()
		{
			var stopMethod = GetAudioUtilMethod( "StopAllPreviewClips" );
			stopMethod?.Invoke( null, null );
		}

		private static void InvokePlayPreviewClip( AudioClip clip )
		{
			var method = GetAudioUtilMethod( "PlayPreviewClip" );
			method?.Invoke( null, new object[] { clip, 0, false } );
		}

		private static MethodInfo GetAudioUtilMethod( string methodName )
		{
			const string audioUtilClassName = "UnityEditor.AudioUtil";
			var unityEditorAssembly = typeof(AudioImporter).Assembly;
			try
			{
				var audioUtilClass = unityEditorAssembly.GetType( audioUtilClassName );
				const BindingFlags flags = BindingFlags.Static | BindingFlags.Public;
				return audioUtilClass?.GetMethod( methodName, flags );
			}
			catch ( Exception ex )
			{
				BDebug.LogError( $"Error retrieving method {methodName} from {audioUtilClassName}: {ex.Message}" );
				return null;
			}
		}

	#endregion
	}
}
