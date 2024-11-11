using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;

namespace Bezoro.Framework.Unity.Editor.Utilities
{
	public abstract class EditorUtils : ICallbacks
	{
		private static bool _isTestRunning;

		public static bool IsEditorBusy()
		{
			UniTask.ReturnToMainThread();

			return EditorApplication.isCompiling
				|| EditorApplication.isUpdating
				|| AssetDatabase.IsAssetImportWorkerProcess()
				|| BuildPipeline.isBuildingPlayer
				|| _isTestRunning;
		}

		public void RunStarted( ITestAdaptor testsToRun )
		{
			_isTestRunning = true;
		}

		public void RunFinished( ITestResultAdaptor result )
		{
			_isTestRunning = false;
		}

		public void TestStarted( ITestAdaptor test )
		{
			_isTestRunning = true;
		}

		public void TestFinished( ITestResultAdaptor result )
		{
			_isTestRunning = false;
		}
	}
}
