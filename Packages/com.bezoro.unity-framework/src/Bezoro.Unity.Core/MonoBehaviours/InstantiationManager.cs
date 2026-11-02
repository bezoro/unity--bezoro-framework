using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Bezoro.Logging.Types;
using Bezoro.Unity.Core.Common.Enums;
using Bezoro.Unity.Core.Common.Extensions;
using Bezoro.Unity.Core.Loading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Logger = Bezoro.Logging.Logger;
using Object = UnityEngine.Object;

namespace Bezoro.Unity.Core.MonoBehaviours
{
	/// <summary>
	///     Centralised queue that spreads instantiation work across frames so
	///     that a frame never spends more than <see cref="FRAME_BUDGET_MS" /> in
	///     cloning prefabs.
	/// </summary>
	[DefaultExecutionOrder(ExecutionOrder.Fundamental)]
	[RequireComponent(typeof(SingletonScope), typeof(GlobalReference))]
	public sealed class InstantiationManager : MonoBehaviour
	{
		public const  float FRAME_BUDGET_MS              = 16f;
		public const  int   MAX_INSTANTIATIONS_PER_FRAME = 5;
		private const float WEIGHT                       = .5f;

		private static readonly InstantiationManagerModel<IRequest> Model = new();
		private static readonly Stopwatch FrameStopwatch = new();

		private static CancellationTokenSource _cts = new();

		private static LoadingStage?    _loadingStage;
		private static ProgressTracker? _tracker;
		private bool _isSingletonInstance;

		public static bool IsAvailable => TryGetInstance(out _);

		public static InstantiationManager Instance => GlobalReferenceAccess<InstantiationManager>.Instance;

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void ResetStaticState()
		{
			_cts.Cancel();
			CancelQueuedRequests(
				_cts.Token
			);

			_cts.Dispose();
			_cts = new();

			Model.ResetState();
			FrameStopwatch.Reset();
			_loadingStage = null;
			_tracker      = null;
		}

		public static bool TryGetInstance([NotNullWhen(true)] out InstantiationManager? instance) =>
			GlobalReferenceAccess<InstantiationManager>.TryGetInstance(out instance);

		#region Unity Lifecycle

		private void Awake()
		{
			_isSingletonInstance = GlobalReferenceAccess<InstantiationManager>.IsCurrent(this);
		}

		private void OnDestroy()
		{
			if (!_isSingletonInstance)
				return;

			Model.MarkDisposed();

			FailQueuedRequests(
				new ObjectDisposedException(
					nameof(InstantiationManager)
				)
			);

			_loadingStage = null;
			_tracker      = null;
		}

		#endregion

		/// <summary>Enqueue a prefab for instantiation.</summary>
		public static UniTask<T> InstantiateAsync<T>(
			T                 prefab,
			Vector3?          position = null,
			Quaternion?       rotation = null,
			Transform?        parent   = null,
			int               priority = 0,
			CancellationToken ct       = default) where T : Object
		{
			_ = Instance;
			prefab.ThrowIfUnityNull();

			var pos = position ?? Vector3.zero;
			var rot = rotation ?? Quaternion.identity;

			return InternalEnqueue(
				prefab,
				pos,
				rot,
				parent,
				priority,
				ct
			);
		}

		private static bool ShouldProcessNext(int processedCount) =>
			Model.ShouldProcessNext(
				processedCount,
				FrameStopwatch.Elapsed.TotalMilliseconds,
				FRAME_BUDGET_MS,
				MAX_INSTANTIATIONS_PER_FRAME,
				_cts.Token.IsCancellationRequested
			);

		private static async UniTask<T> InternalEnqueue<T>(
			T                 prefab,
			Vector3           pos,
			Quaternion        rot,
			Transform?        parent,
			int               priority,
			CancellationToken ct)
			where T : Object
		{
			var req = new Request<T>(
				prefab,
				pos,
				rot,
				parent,
				ct
			);

			var enqueueDecision = Model.DecideEnqueue(_cts.IsCancellationRequested);

			if (enqueueDecision == InstantiationManagerEnqueueDecision.FailDisposed)
			{
				req.Fail(
					new ObjectDisposedException(
						nameof(InstantiationManager)
					)
				);

				return await req.Task;
			}

			if (enqueueDecision == InstantiationManagerEnqueueDecision.Cancel)
			{
				req.Cancel(
					_cts.Token
				);

				return await req.Task;
			}

			Model.Enqueue(
				req,
				priority
			);

			if (!Model.TryStartProcessing()) return await req.Task;

			ProcessQueueWhileNotEmpty().Forget();

			return await req.Task;
		}


		private static async UniTaskVoid ProcessQueueWhileNotEmpty()
		{
			try
			{
				if (!LoadingProgressService.TryGetInstance(
						out var loadingProgressService
					))
				{
					FailQueuedRequests(
						new InvalidOperationException(
							$"{nameof(InstantiationManager)} requires an active {nameof(LoadingProgressService)}."
						)
					);

					return;
				}

				FrameStopwatch.Reset();

				if (!loadingProgressService.IsLoading)
				{
					Logger.LogWarning(
						$"[{nameof(HierarchyCache)}] No active loading operation found – " +
						"creating a fallback one. Progress aggregation may be less accurate.",
						contextObject: Instance,
						category:
						LogCategory.Loading
					);

					loadingProgressService.BeginOperation(
						"Async Instantiation (Fallback)"
					);
				}

				_loadingStage ??= loadingProgressService.BeginStage(
					"Instantiate Batches",
					WEIGHT
				);

				_tracker ??= _loadingStage.CreateTracker(
					"Instantiation Batch"
				);

				while (Model.ShouldContinueProcessing(_cts.IsCancellationRequested))
				{
					Logger.Log(
						$"[{nameof(InstantiationManager)}] Processing queue – current count: {Model.QueueCount}",
						contextObject:
						Instance,
						category:
						LogCategory.Loading
					);

					FrameStopwatch.Restart();
					var processedCount = 0;
					int batchCount     = Model.QueueCount;

					while (ShouldProcessNext(
							   processedCount
						   ))
					{
						var req = Model.Dequeue();
						await req.ExecuteAsync();
						processedCount++;
					}

					_tracker.BindToVariable(() => Model.GetBatchProgress(processedCount, batchCount)
					);

					FrameStopwatch.Stop();
					Logger.Log(
						$"[{nameof(InstantiationManager)}] Frame finished – " +
						$"elapsed: {FrameStopwatch.Elapsed.TotalMilliseconds:F2} ms, remaining: {Model.QueueCount}",
						contextObject: Instance,
						category:
						LogCategory.Loading
					);
				}
			}
			catch (Exception ex)
			{
				Logger.LogException(
					ex,
					contextObject: Instance,
					category: LogCategory.Loading
				);

				FailQueuedRequests(
					ex
				);
			}
			finally
			{
				switch (Model.DecideFinalQueueAction(_cts.IsCancellationRequested))
				{
					case InstantiationManagerFinalQueueAction.Cancel:
						CancelQueuedRequests(
							_cts.Token
						);
						break;

					case InstantiationManagerFinalQueueAction.FailDisposed:
						FailQueuedRequests(
							new ObjectDisposedException(
								nameof(InstantiationManager)
							)
						);
						break;
				}

				Model.FinishProcessing();
			}
		}

		private static void CancelQueuedRequests(CancellationToken ct)
		{
			while (Model.QueueCount > 0)
			{
				Model.Dequeue()
					 .Cancel(
						 ct
					 );
			}
		}

		private static void FailQueuedRequests(Exception exception)
		{
			while (Model.QueueCount > 0)
			{
				Model.Dequeue()
					 .Fail(
						 exception
					 );
			}
		}

		/// <summary>
		///     Defines queued instantiation work processed by the manager.
		/// </summary>
		private interface IRequest
		{
			UniTask ExecuteAsync();
			void    Cancel(CancellationToken ct);

			void Fail(Exception exception);
		}

		/// <summary>
		///     Executes one queued prefab instantiation and completes its awaiting task.
		/// </summary>
		private sealed class Request<T> : IRequest where T : Object
		{
			private readonly CancellationToken             _ct;
			private readonly CancellationTokenRegistration _ctRegistration;
			private readonly Quaternion                    _rotation;
			private readonly T                             _prefab;
			private readonly Transform?                    _parent;
			private readonly UniTaskCompletionSource<T>    _utcs = new();
			private readonly Vector3                       _position;

			private bool _executed;

			public Request(T prefab, Vector3 position, Quaternion rotation, Transform? parent, CancellationToken ct)
			{
				_prefab   = prefab;
				_position = position;
				_rotation = rotation;
				_parent   = parent;
				_ct       = ct;

				if (ct.IsCancellationRequested)
				{
					_utcs.TrySetCanceled(
						ct
					);

					return;
				}

				if (ct.CanBeCanceled)
					_ctRegistration = ct.Register(() => _utcs.TrySetCanceled(
													  ct
												  )
					);
			}

			public UniTask<T> Task => _utcs.Task;

			public async UniTask ExecuteAsync()
			{
				if (_executed) return;

				_executed = true;

				try
				{
					if (_ct.IsCancellationRequested)
					{
						await _ctRegistration.DisposeAsync();

						Logger.Log(
							$"[{nameof(InstantiationManager)}] Instantiation cancelled for prefab '{_prefab.name}'."
						);

						_utcs.TrySetCanceled(
							_ct
						);

						return;
					}

					await _ctRegistration.DisposeAsync();

					if (_ct.IsCancellationRequested)
					{
						_utcs.TrySetCanceled(
							_ct
						);

						return;
					}

					var op = await Object.InstantiateAsync(
								 _prefab,
								 _position,
								 _rotation
							 );

					if (op == null || op.Length == 0)
					{
						Fail(
							new InvalidOperationException(
								$"InstantiateAsync returned no objects for prefab '{_prefab.name}'."
							)
						);

						return;
					}

					var obj = op[0] as GameObject ?? (op[0] as Component)?.gameObject;

					if (obj == null)
					{
						Fail(
							new InvalidOperationException(
								$"Instantiation result did not contain a GameObject for prefab '{_prefab.name}'."
							)
						);

						return;
					}

					if (_parent != null)
						obj.transform.SetParent(
							_parent
						);

					Logger.Log(
						$"[{nameof(InstantiationManager)}] Instantiated prefab '{_prefab.name}' at {_position}.",
						contextObject: this,
						category:
						LogCategory.Loading
					);

					_utcs.TrySetResult(
						op[0]
					);
				}
				catch (OperationCanceledException) when (_ct.IsCancellationRequested)
				{
					_utcs.TrySetCanceled(
						_ct
					);
				}
				catch (Exception ex)
				{
					Fail(
						ex
					);
				}
			}

			public void Cancel(CancellationToken ct) =>
				_utcs.TrySetCanceled(
					ct
				);

			public void Fail(Exception exception)
			{
				Logger.LogException(
					exception,
					contextObject: this,
					category: LogCategory.Loading
				);

				_utcs.TrySetException(
					exception
				);
			}
		}
	}

	public sealed class InstantiationManagerModel<TRequest>
	{
		private readonly InstantiationPriorityQueue<TRequest> _queue = new();

		private bool _isDisposed;
		private bool _isProcessing;

		public int QueueCount => _queue.Count;

		public InstantiationManagerEnqueueDecision DecideEnqueue(bool isCancellationRequested)
		{
			if (_isDisposed)
				return InstantiationManagerEnqueueDecision.FailDisposed;

			return isCancellationRequested
					   ? InstantiationManagerEnqueueDecision.Cancel
					   : InstantiationManagerEnqueueDecision.Enqueue;
		}

		public void Enqueue(TRequest request, int priority)
		{
			_queue.Enqueue(request, priority);
		}

		public TRequest Dequeue()
		{
			return _queue.Dequeue();
		}

		public bool TryStartProcessing()
		{
			if (_isProcessing) return false;

			_isProcessing = true;
			return true;
		}

		public void FinishProcessing()
		{
			_isProcessing = false;
		}

		public bool ShouldContinueProcessing(bool isCancellationRequested)
		{
			return !_isDisposed && _queue.Count > 0 && !isCancellationRequested;
		}

		public bool ShouldProcessNext(
			int processedCount,
			double elapsedMilliseconds,
			float frameBudgetMilliseconds,
			int maxInstantiationsPerFrame,
			bool isCancellationRequested)
		{
			return _queue.Count > 0 &&
				   elapsedMilliseconds < frameBudgetMilliseconds &&
				   processedCount < maxInstantiationsPerFrame &&
				   !isCancellationRequested;
		}

		public float GetBatchProgress(int processedCount, int batchCount)
		{
			return batchCount == 0 ? 1f : processedCount / (float)batchCount;
		}

		public InstantiationManagerFinalQueueAction DecideFinalQueueAction(bool isCancellationRequested)
		{
			if (isCancellationRequested)
				return InstantiationManagerFinalQueueAction.Cancel;

			return _isDisposed
					   ? InstantiationManagerFinalQueueAction.FailDisposed
					   : InstantiationManagerFinalQueueAction.None;
		}

		public void MarkDisposed()
		{
			_isDisposed = true;
		}

		public void ResetState()
		{
			_queue.Clear();
			_isDisposed   = false;
			_isProcessing = false;
		}
	}

	public enum InstantiationManagerEnqueueDecision
	{
		Enqueue,
		Cancel,
		FailDisposed
	}

	public enum InstantiationManagerFinalQueueAction
	{
		None,
		Cancel,
		FailDisposed
	}

	/// <summary>Minimal priority queue based on <see cref="SortedDictionary{TKey,TValue}" />.</summary>
	public sealed class InstantiationPriorityQueue<TValue>
	{
		private readonly SortedDictionary<int, Queue<TValue>> _queuesByPriority = new();

		public int Count { get; private set; }

		public TValue Dequeue()
		{
			var pair = _queuesByPriority.First();
			var item = pair.Value.Dequeue();
			if (pair.Value.Count == 0)
				_queuesByPriority.Remove(
					pair.Key
				);

			Count--;
			return item;
		}

		public void Clear()
		{
			_queuesByPriority.Clear();
			Count = 0;
		}

		public void Enqueue(TValue value, int priority)
		{
			if (!_queuesByPriority.TryGetValue(
					priority,
					out var queue
				))
			{
				queue = new();
				_queuesByPriority.Add(
					priority,
					queue
				);
			}

			queue.Enqueue(
				value
			);

			Count++;
		}
	}
}
