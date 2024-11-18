using System;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Compilation;
using UnityEditor.TestTools.TestRunner.Api;

namespace Bezoro.Framework.Unity.Editor.Utilities
{
	[InitializeOnLoad]
	public abstract class EditorUtils :
		ICallbacks,
		IPreprocessBuildWithReport, IPostprocessBuildWithReport
	{
	#region Events

		public static event Action TestRunStarted;
		public static event Action TestRunFinished;
		public static event Action CompilationStarted;
		public static event Action CompilationFinished;
		public static event Action DomainReloadStarted;
		public static event Action DomainReloadFinished;
		public static event Action AssetImportStarted;
		public static event Action AssetImportFinished;
		public static event Action BuildStarted;
		public static event Action BuildFinished;

	#endregion

		public static bool IsTestRunInProgress;
		public static bool IsCompilationInProgress;
		public static bool IsDomainReloadInProgress;
		public static bool IsAssetImportInProgress;
		public static bool IsBuildInProgress;

		public int callbackOrder => 0;

		static EditorUtils()
		{
			IsTestRunInProgress      = false;
			IsCompilationInProgress  = false;
			IsDomainReloadInProgress = false;
			IsAssetImportInProgress  = false;
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
		}

		private static void Update()
		{
			TryRaiseCompilationEvents();
			TryRaiseAssetImporterEvents();
		}

		private static void OnBeforeAssemblyReload()
		{
			IsDomainReloadInProgress = true;
			RaiseEvent( DomainReloadStarted );
		}

		private static void OnAfterAssemblyReload()
		{
			IsDomainReloadInProgress = false;
			RaiseEvent( DomainReloadFinished );
		}

		private static void TryRaiseAssetImporterEvents()
		{
			if ( AssetDatabase.IsAssetImportWorkerProcess() && !IsAssetImportInProgress )
			{
				IsAssetImportInProgress = true;
				RaiseEvent( AssetImportStarted );
			}
			else if ( !AssetDatabase.IsAssetImportWorkerProcess() && IsAssetImportInProgress )
			{
				IsAssetImportInProgress = false;
				RaiseEvent( AssetImportFinished );
			}
		}

		private static void TryRaiseCompilationEvents()
		{
			// TryRaiseCompilationStarted();
			// TryRaiseCompilationFinished();
		}

		private static void OnCompilationFinished( object o )
		{
			// if ( EditorApplication.isCompiling ) { return; }

			// if ( !IsCompilationInProgress ) { return; }

			IsCompilationInProgress = false;
			RaiseEvent( CompilationFinished );
		}

		private static void OnCompilationStarted( object o )
		{
			// Debug.Log( EditorApplication.isCompiling );
			// Debug.Log( IsCompilationInProgress );
			// if ( !EditorApplication.isCompiling ) { return; }

			// if ( IsCompilationInProgress ) { return; }

			IsCompilationInProgress = true;
			RaiseEvent( CompilationStarted );
		}

		private static void RaiseEvent( Action eventAction )
		{
			// BDebug.Log( $"{nameof(RaiseEvent)}: {eventAction.Method.Name}" );
			eventAction?.Invoke();
		}

		public static bool IsEditorBusy()
		{
			UniTask.ReturnToMainThread();
			return IsCompilationInProgress
				|| EditorApplication.isUpdating
				|| AssetDatabase.IsAssetImportWorkerProcess()
				|| IsBuildInProgress
				|| IsTestRunInProgress;
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
	}
}
