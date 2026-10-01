using System.Threading;
using Bezoro.Unity.Core.Common.Enums;
using Bezoro.Unity.Core.Loading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Bezoro.Unity.Core.MonoBehaviours
{
	/// <summary>
	///     Unloads the scene this GameObject is currently in when it receives the AsyncLoadingManager.LoadingComplete
	///     notification.
	/// </summary>
	[DefaultExecutionOrder(
		ExecutionOrder.Early
	)]
	public class UnloadSceneOnLoadingComplete : MonoBehaviour
	{
		private readonly UnloadSceneOnLoadingCompleteModel _model = new();

		[SerializeField] private float                    delayS = 1;
		private                  CancellationTokenSource? _destroyCts;

		#region Unity Lifecycle

		private void Awake()
		{
			_destroyCts = new();

			if (!LoadingProgressService.TryGetInstance(
					out var loadingProgressService
				)) return;

			loadingProgressService.Completed += OnLoadingCompleted;
		}

		private void OnDestroy()
		{
			_destroyCts?.Cancel();
			_destroyCts?.Dispose();
			_destroyCts = null;

			if (!LoadingProgressService.TryGetInstance(
					out var loadingProgressService
				)) return;

			loadingProgressService.Completed -= OnLoadingCompleted;
		}

		#endregion

		private async UniTaskVoid UnloadSceneAndDestroy(CancellationToken ct)
		{
			await UniTask.WaitUntil(
				() => gameObject.scene.isLoaded,
				cancellationToken: ct
			);

			await UniTask.WaitForSeconds(
				delayS,
				cancellationToken: ct
			);

			ct.ThrowIfCancellationRequested();
			if (this == null)
				return;

			SceneLoader.Unload(
				gameObject.scene.path,
				ct
			).Task.Forget();

			Destroy(
				gameObject
			);
		}

		private void OnLoadingCompleted()
		{
			var destroyCts = _destroyCts;
			if (!_model.ShouldStartUnload(destroyCts != null, destroyCts?.IsCancellationRequested ?? true) ||
				destroyCts == null)
				return;

			UnloadSceneAndDestroy(destroyCts.Token).Forget();
		}
	}

	public sealed class UnloadSceneOnLoadingCompleteModel
	{
		public bool ShouldStartUnload(bool hasCancellationSource, bool isCancellationRequested) =>
			hasCancellationSource && !isCancellationRequested;
	}
}
