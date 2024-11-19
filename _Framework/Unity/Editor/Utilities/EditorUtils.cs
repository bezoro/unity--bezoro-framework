using System;
using System.Reflection;
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
	[InitializeOnLoad]
	public class EditorUtils :
		ICallbacks,
		IPreprocessBuildWithReport, IPostprocessBuildWithReport
	{
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

		public static bool IsTestRunInProgress;
		public static bool IsDomainReloadInProgress;
		public static bool IsBuildInProgress;

		public static bool IsCompilationInProgress => EditorApplication.isCompiling;
		public static bool IsAssetImportInProgress => AssetDatabase.IsAssetImportWorkerProcess();
		public static bool IsUpdating => EditorApplication.isUpdating;

		public static bool IsEditorBusy() =>
			IsCompilationInProgress
		 || IsUpdating
		 || IsAssetImportInProgress
		 || IsBuildInProgress
		 || IsTestRunInProgress
		 || IsDomainReloadInProgress;

		public int callbackOrder => 0;

		static EditorUtils()
		{
			IsTestRunInProgress      = false;
			IsDomainReloadInProgress = false;
			IsBuildInProgress        = false;

			// EditorApplication.update -= Update;
			// AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeAssemblyReload;
			CompilationPipeline.compilationStarted   -= OnCompilationStarted;
			CompilationPipeline.compilationFinished  -= OnCompilationFinished;
			AssemblyReloadEvents.afterAssemblyReload -= OnAfterAssemblyReload;

			// EditorApplication.update += Update;
			// AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
			CompilationPipeline.compilationStarted   += OnCompilationStarted;
			CompilationPipeline.compilationFinished  += OnCompilationFinished;
			AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;

			// MonitorEditorBusyAsync().Forget();
		}

		private static void OnBeforeAssemblyReload()
		{
			IsDomainReloadInProgress = true;
			RaiseEvent( AssemblyReloadStarted );
		}

		private static void OnAfterAssemblyReload()
		{
			if ( IsCompilationInProgress ) { return; }
			if ( IsBuildInProgress ) { return; }
			IsDomainReloadInProgress = false;
			RaiseEvent( AssemblyReloadFinished );
		}

		private static void TryRaiseAssetImporterEvents()
		{
			if ( AssetDatabase.IsAssetImportWorkerProcess() && !IsAssetImportInProgress )
			{
				RaiseEvent( AssetImportStarted );
			}
			else if ( !AssetDatabase.IsAssetImportWorkerProcess() && IsAssetImportInProgress )
			{
				RaiseEvent( AssetImportFinished );
			}
		}

		private static void OnCompilationFinished( object o )
		{
			if ( IsBuildInProgress ) { return; }
			if ( IsTestRunInProgress ) { return; }
			RaiseEvent( CompilationFinished );
		}

		private static void OnCompilationStarted( object o )
		{
			if ( IsBuildInProgress ) { return; }
			if ( IsTestRunInProgress ) { return; }
			RaiseEvent( CompilationStarted );
		}

		private static void RaiseEvent( Action eventAction )
		{
			eventAction?.Invoke();
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

		public void TestStarted( ITestAdaptor test ) {}

		public void TestFinished( ITestResultAdaptor result ) {}

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

		public static void PlayClip( AudioClip clip )
		{
			Assert.IsNotNull( clip );
			try
			{
				var unityEditorAssembly = typeof(AudioImporter).Assembly;
				var audioUtilClass = unityEditorAssembly.GetType( "UnityEditor.AudioUtil" );

				var stopMethod = audioUtilClass?.GetMethod(
					"StopAllPreviewClips",
					BindingFlags.Static | BindingFlags.Public
				);

				stopMethod?.Invoke( null, null );

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
				BDebug.LogError( "Error playing compilation sound: " + e.Message );
			}
		}
	}
}
