using System;
using System.Collections.Generic;
using System.Threading;
using Bezoro.Logging;
using Cysharp.Threading.Tasks;
using UnityEngine.Profiling;

namespace Bezoro.Unity.Core.EventSystem;

/// <summary>
///     Manages event subscriptions, unsubscriptions, and event publishing.
/// </summary>
[Serializable]
public class EventBus : IEventBus
{
	private const int BATCH_SIZE    = 100;
	private const int MAX_POOL_SIZE = 100;

	private readonly Dictionary<Type, HashSet<Delegate>>  _eventHandlers                   = new();
	private readonly Dictionary<Type, ITypeSafeEventPool> _eventPools                      = new();
	private readonly List<CancellationTokenSource>        _retiredCancellationTokenSources = new();
	private readonly object                               _lock                            = new();
	private readonly SemaphoreSlim                        _processSemaphore                = new(1, 1);

	private readonly TypeSafeEventQueue _eventQueue = new();

	private bool _isProcessingEvents;
	private bool _disposed;

	private CancellationTokenSource _cancellationTokenSource;

	public EventBus()
	{
		_cancellationTokenSource = new();
	}

	public void Dispose()
	{
		if (_disposed) return;

		_disposed = true;
		_cancellationTokenSource.Cancel();
		_cancellationTokenSource.Dispose();
		DisposeRetiredCancellationTokenSources();
		_processSemaphore.Dispose();
	}

	private HashSet<Delegate> GetHandlersForType(Type type)
	{
		lock (_lock)
		{
			return _eventHandlers.TryGetValue(
					   type,
					   out var handlers
				   )
					   ? new(
						   handlers
					   )
					   : new HashSet<Delegate>();
		}
	}

	private TEvent GetFromPool<TEvent>()
		where TEvent : IEvent
	{
		lock (_lock)
		{
			if (!_eventPools.TryGetValue(
					typeof(TEvent),
					out var pool
				))
			{
				pool = new TypeSafeEventPool<TEvent>(
					MAX_POOL_SIZE,
					() => (TEvent)Activator.CreateInstance(
						typeof(TEvent)
					)
				);

				_eventPools[typeof(TEvent)] = pool;
			}

			return ((TypeSafeEventPool<TEvent>)pool).Get();
		}
	}

	private async UniTask InvokeHandlersAsync(
		HashSet<Delegate> handlers,
		IEvent            eventData,
		CancellationToken cancellationToken
	)
	{
		Profiler.BeginSample(
			"InvokeHandlersAsync"
		);

		foreach (var handler in handlers)
		{
			cancellationToken.ThrowIfCancellationRequested();

			try
			{
				if (eventData.RunOnThreadPool)
				{
					await UniTask.RunOnThreadPool(
						() => handler.DynamicInvoke(
							eventData
						),
						cancellationToken: cancellationToken
					);
				}
				else
				{
					await UniTask.SwitchToMainThread(
						cancellationToken
					);

					handler.DynamicInvoke(
						eventData
					);
				}
			}
			catch (OperationCanceledException)
			{
				throw;
			}
			catch (Exception e)
			{
				await UniTask.SwitchToMainThread(
					cancellationToken
				);

				Logger.LogError(
					$"Error processing event {eventData.GetType().Name}: {e}"
				);
			}
		}

		Profiler.EndSample();
	}

	private async UniTask ProcessEventsAsync(CancellationToken cancellationToken)
	{
		Profiler.BeginSample(
			"ProcessEventsAsync"
		);

		var semaphoreAcquired = false;

		try
		{
			await _processSemaphore.WaitAsync(
				cancellationToken
			);

			semaphoreAcquired = true;

			await UniTask.SwitchToMainThread(
				cancellationToken
			);

			_isProcessingEvents = true;

			while (!cancellationToken.IsCancellationRequested)
			{
				int processedCount = await ProcessEventBatchAsync(
										 cancellationToken
									 );

				if (processedCount == 0) break;
			}
		}
		catch (OperationCanceledException)
		{
			Logger.Log(
				"Event processing was cancelled."
			);
		}
		finally
		{
			_isProcessingEvents = false;
			if (semaphoreAcquired)
				_processSemaphore.Release();

			DisposeRetiredCancellationTokenSources();
			Profiler.EndSample();
		}
	}

	private async UniTask<int> ProcessEventBatchAsync(CancellationToken cancellationToken)
	{
		Profiler.BeginSample(
			"ProcessEventBatchAsync"
		);

		var processedCount = 0;

		for (var i = 0; i < BATCH_SIZE; i++)
		{
			cancellationToken.ThrowIfCancellationRequested();

			if (!_eventQueue.TryDequeueAny(
					out var eventType,
					out var eventData
				))
				break;

			try
			{
				var handlers = GetHandlersForType(
					eventType!
				);

				await InvokeHandlersAsync(
					handlers,
					eventData!,
					cancellationToken
				);

				processedCount++;
			}
			finally
			{
				if (eventData != null)
					ReturnToPool(
						eventData
					);
			}
		}

		Profiler.EndSample();

		return processedCount;
	}

	private void DisposeRetiredCancellationTokenSources()
	{
		lock (_lock)
		{
			foreach (var cts in _retiredCancellationTokenSources)
				cts.Dispose();

			_retiredCancellationTokenSources.Clear();
		}
	}

	private void ReturnToPool(IEvent eventData)
	{
		lock (_lock)
		{
			if (_eventPools.TryGetValue(
					eventData.GetType(),
					out var pool
				))
				pool.Return(
					eventData
				);
		}
	}

	#region Interface Implementations

	public void CancelEventProcessing()
	{
		ThrowIfDisposed();

		var previousCts = _cancellationTokenSource;
		_cancellationTokenSource.Cancel();
		_cancellationTokenSource = new();

		if (_isProcessingEvents)
			lock (_lock)
			{
				_retiredCancellationTokenSources.Add(
					previousCts
				);
			}
		else
			previousCts.Dispose();
	}

	public void SubscribeUnique<T>(Action<T> handler)
		where T : IEvent
	{
		if (handler == null)
			throw new ArgumentNullException(
				nameof(handler)
			);

		lock (_lock)
		{
			if (!_eventHandlers.TryGetValue(
					typeof(T),
					out var handlers
				))
			{
				handlers                  = new();
				_eventHandlers[typeof(T)] = handlers;
			}

			handlers.Add(
				handler
			);
		}
	}

	public void Unsubscribe<T>(Action<T> handler)
		where T : IEvent
	{
		if (handler == null)
			throw new ArgumentNullException(
				nameof(handler)
			);

		lock (_lock)
		{
			if (_eventHandlers.TryGetValue(
					typeof(T),
					out var handlers
				))
			{
				handlers.Remove(
					handler
				);

				if (handlers.Count == 0)
					_eventHandlers.Remove(
						typeof(T)
					);
			}
		}
	}

	public void UnsubscribeAll<T>()
		where T : IEvent
	{
		lock (_lock)
		{
			_eventHandlers.Remove(
				typeof(T)
			);
		}
	}

	public async UniTask Publish<T>(T eventData)
		where T : IEvent
	{
		ThrowIfDisposed();

		if (eventData == null)
			throw new ArgumentNullException(
				nameof(eventData)
			);

		if (!HasSubscribersFor<T>())
		{
			ReturnToPool(
				eventData
			);

			return;
		}

		Profiler.BeginSample(
			$"Publish {typeof(T).Name}"
		);

		_eventQueue.Enqueue(
			eventData
		);

		Profiler.EndSample();

		if (!_isProcessingEvents)
			await ProcessEventsAsync(
				_cancellationTokenSource.Token
			);
	}

	public async UniTask PublishAsync<T>(T eventData)
		where T : IEvent
	{
		await Publish(eventData);

		await UniTask.WaitUntil(
			() => !_isProcessingEvents,
			cancellationToken: _cancellationTokenSource.Token
		);
	}

	public bool HasSubscribersFor<T>()
		where T : IEvent
	{
		lock (_lock)
		{
			return _eventHandlers.TryGetValue(
					   typeof(T),
					   out var handlers
				   ) &&
				   handlers.Count > 0;
		}
	}

	public void Clear()
	{
		lock (_lock)
		{
			_eventHandlers.Clear();
			_eventQueue.Clear();
		}
	}

	public TEvent CreateEvent<TEvent>()
		where TEvent : IEvent, new() =>
		GetFromPool<TEvent>();

	public TEvent CreateEvent<TEvent, TPayload>(TPayload payload)
		where TEvent : IEvent<TPayload>, new()
	{
		var evt = GetFromPool<TEvent>();
		evt.Initialize(
			payload
		);

		return evt;
	}

	private void ThrowIfDisposed()
	{
		if (_disposed)
			throw new ObjectDisposedException(
				nameof(EventBus)
			);
	}

	#endregion
}
