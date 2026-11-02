using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Bezoro.Core.Extensions;
using Bezoro.Logging.Types;
using Bezoro.Unity.Core.Common.Enums;
using Bezoro.Unity.Core.Loading;
using Bezoro.Unity.Core.MonoBehaviours;
using Bezoro.Unity.Core.Scenes;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Logger = Bezoro.Logging.Logger;

namespace Bezoro.Unity.Core.AsyncLoading
{
	/// <summary>
	///     Processes registered async-loadable scene objects by priority.
	/// </summary>
	[RequireComponent(typeof(SingletonScope), typeof(GlobalReference))]
	[DefaultExecutionOrder(ExecutionOrder.Manager)]
	public class AsyncLoadingManager : MonoBehaviour
	{
		private readonly AsyncLoadingManagerModel<IAsyncLoadable> _model = new(item => item.Priority);

		private bool _isProcessing;
		private int  _itemsLoaded;
		private int  _totalItemsToLoad;

		[Header("Debug")]

		private CancellationTokenSource _cts = null!;

		private UniTaskCompletionSource? _workSignal;

		public static bool IsAvailable => TryGetInstance(out _);

		public static AsyncLoadingManager Instance => GlobalReferenceAccess<AsyncLoadingManager>.Instance;

		public bool IsProcessing => _isProcessing;

		public static bool TryGetInstance([NotNullWhen(true)] out AsyncLoadingManager? instance) =>
			GlobalReferenceAccess<AsyncLoadingManager>.TryGetInstance(out instance);

		#region Unity Lifecycle

		private void Awake()
		{
			if (!GlobalReferenceAccess<AsyncLoadingManager>.IsCurrent(this))
				return;

			_cts = new();
			RunProcessingLoopAsync(_cts.Token).Forget();
		}

		private void OnDestroy()
		{
			if (!_cts.IsNull())
			{
				_cts.Cancel();
				_cts.Dispose();
			}

			ClearState();
		}

		#endregion

		public void Register(IAsyncLoadable item)
		{
			if (!_model.ShouldRegister(!item.IsNull()))
			{
				Logger.LogError(
					"Cannot register null item for async loading.",
					contextObject: this,
					category: LogCategory.Loading
				);

				return;
			}

			_model.Register(item);
			_totalItemsToLoad = _model.QueueCount;

			Logger.Log(
				$"Registered item for loading: {item.LoadingName}",
				contextObject: this,
				category: LogCategory.Loading
			);

			_workSignal?.TrySetResult();
		}

		public void Remove(IAsyncLoadable item)
		{
			if (!_model.ShouldRemove(!item.IsNull())) return;

			_model.Remove(item);
			_totalItemsToLoad = _model.QueueCount;

			Logger.Log(
				$"Removed item from loading: {item.LoadingName}",
				contextObject: this,
				category: LogCategory.Loading
			);
		}

		private IAsyncLoadable GetHighestPriority()
		{
			return _model.GetHighestPriority();
		}

		private async UniTask RunProcessingLoopAsync(CancellationToken ct)
		{
			while (!ct.IsCancellationRequested)
			{
				// Wait for work if queue is empty
				if (_model.QueueCount == 0)
				{
					_isProcessing = false;
					_workSignal   = new();

					try
					{
						await _workSignal.Task.AttachExternalCancellation(ct);
					}
					catch (OperationCanceledException)
					{
						return;
					}
					finally
					{
						_workSignal = null;
					}
				}

				await UniTask.Yield(ct);

				SetupProgressTracking();

				_isProcessing     = true;
				_itemsLoaded      = 0;
				_totalItemsToLoad = _model.QueueCount;

				Logger.Log(
					$"Started processing. Queue size: {_model.QueueCount}",
					contextObject: this,
					category: LogCategory.Loading
				);

				// Process all items until queue is drained
				while (_model.QueueCount > 0 && !ct.IsCancellationRequested)
				{
					var item = GetHighestPriority();

					if (item.IsLoaded)
					{
						_model.Remove(item);
						continue;
					}

					try
					{
						await item.LoadAsync(ct);
					}
					catch (OperationCanceledException) when (ct.IsCancellationRequested)
					{
						throw;
					}
					catch (Exception e)
					{
						Logger.LogError(
							$"Failed to load item: {item.LoadingName} — {e.Message}",
							contextObject: this,
							category: LogCategory.Loading
						);
					}

					_model.Remove(item);
					_itemsLoaded++;

					_totalItemsToLoad = _model.GetTrackedTotalItemsToLoad(_totalItemsToLoad, _itemsLoaded);
				}

				_isProcessing = false;

				Logger.Log(
					$"Finished processing. Remaining: {_model.QueueCount}",
					contextObject: this,
					category: LogCategory.Loading
				);
			}
		}

		private void ClearState()
		{
			_model.Clear();
			_itemsLoaded      = 0;
			_totalItemsToLoad = 0;
			_isProcessing     = false;
		}

		[ContextMenu("Debug Scene Info")]
		private void DebugSceneInfo()
		{
			Debug.Log($"Loading status: processing={_isProcessing}, loaded={_itemsLoaded}, total={_totalItemsToLoad}.", this);

			var currentScene = SceneManager.GetActiveScene();
			Logger.Log(
				$"Current Scene: {currentScene.name}, Path: {currentScene.path}",
				contextObject: this,
				category: LogCategory.Loading
			);

			var monoBehaviours = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include);

			var asyncObjects = 0;

			for (var i = 0; i < monoBehaviours.Length; i++)
			{
				if (monoBehaviours[i] is IAsyncLoadable)
					asyncObjects++;
			}

			Logger.Log(
				$"Found {asyncObjects} async objects.",
				contextObject: this,
				category: LogCategory.Loading
			);
		}

		private void SetupProgressTracking()
		{
			if (!LoadingProgressService.TryGetInstance(out var loadingProgressService))
				return;

			if (!loadingProgressService.IsLoading)
			{
				Logger.LogWarning(
					$"[{nameof(HierarchyCache)}] No active loading operation found – " +
					"creating a fallback one. Progress aggregation may be less accurate.",
					contextObject: this,
					category: LogCategory.Loading
				);

				loadingProgressService.BeginOperation("Async Loading (Fallback)");
			}
		}
	}

	public sealed class AsyncLoadingManagerModel<TItem>
	{
		private readonly List<TItem>  _loadingQueue = new();
		private readonly Func<TItem, int> _getPriority;

		public AsyncLoadingManagerModel(Func<TItem, int> getPriority)
		{
			_getPriority = getPriority;
		}

		public int QueueCount => _loadingQueue.Count;

		public bool ShouldRegister(bool hasItem)
		{
			return hasItem;
		}

		public bool ShouldRemove(bool hasItem)
		{
			return hasItem;
		}

		public void Register(TItem item)
		{
			_loadingQueue.Add(item);
		}

		public bool Remove(TItem item)
		{
			return _loadingQueue.Remove(item);
		}

		public TItem GetHighestPriority()
		{
			var best = _loadingQueue[0];

			for (var i = 1; i < _loadingQueue.Count; i++)
			{
				var candidate = _loadingQueue[i];

				if (_getPriority(candidate) > _getPriority(best))
					best = candidate;
			}

			return best;
		}

		public int GetTrackedTotalItemsToLoad(int currentTotalItemsToLoad, int itemsLoaded)
		{
			return Math.Max(currentTotalItemsToLoad, itemsLoaded + _loadingQueue.Count);
		}

		public void Clear()
		{
			_loadingQueue.Clear();
		}
	}
}
