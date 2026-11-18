using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Bezoro.Logging;
using Cysharp.Threading.Tasks;

namespace Bezoro.Unity.Core.EventSystem;

/// <summary>Serializes event dispatch and tracks the completion of each publication.</summary>
[Serializable]
public class EventBus : IEventBus
{
	private readonly object _gate = new();
	private readonly Dictionary<Type, HashSet<Delegate>> _eventHandlers = new();
	private readonly Dictionary<Type, ITypeSafeEventPool> _eventPools = new();
	private readonly ConditionalWeakTable<object, Lease> _leases = new();
	private readonly StableEventQueue<Publication> _queue = new();
	private bool _draining;
	private bool _disposed;
	private Publication? _active;

	private sealed class Lease
	{
		public bool CheckedOut;
	}

	private sealed class Publication
	{
		public readonly Type EventType;
		public readonly IEvent Data;
		public readonly UniTaskCompletionSource Completion = new();
		public volatile bool Canceled;

		public Publication(Type eventType, IEvent data)
		{
			EventType = eventType;
			Data = data;
		}
	}

	public void Dispose() => StopPending(dispose: true, clearHandlers: true);

	public void Clear() => StopPending(dispose: false, clearHandlers: true);

	public void CancelEventProcessing() => StopPending(dispose: false, clearHandlers: false);

	private void StopPending(bool dispose, bool clearHandlers)
	{
		var pending = new List<Publication>();
		lock (_gate)
		{
			if (_disposed && dispose) return;
			ThrowIfDisposed();
			_disposed = dispose;
			if (clearHandlers) _eventHandlers.Clear();
			if (_active != null) _active.Canceled = true;
			while (_queue.TryDequeue(out var publication))
			{
				ReturnLease(publication.Data);
				pending.Add(publication);
			}
			if (dispose) _eventPools.Clear();
		}
		foreach (var publication in pending) publication.Completion.TrySetCanceled();
	}

	public void SubscribeUnique<T>(Action<T> handler) where T : IEvent
	{
		if (handler == null) throw new ArgumentNullException(nameof(handler));
		lock (_gate)
		{
			ThrowIfDisposed();
			if (!_eventHandlers.TryGetValue(typeof(T), out var handlers))
			{
				handlers = new HashSet<Delegate>();
				_eventHandlers.Add(typeof(T), handlers);
			}
			handlers.Add(handler);
		}
	}

	public void Unsubscribe<T>(Action<T> handler) where T : IEvent
	{
		if (handler == null) throw new ArgumentNullException(nameof(handler));
		lock (_gate)
		{
			ThrowIfDisposed();
			if (!_eventHandlers.TryGetValue(typeof(T), out var handlers)) return;
			handlers.Remove(handler);
			if (handlers.Count == 0) _eventHandlers.Remove(typeof(T));
		}
	}

	public void UnsubscribeAll<T>() where T : IEvent
	{
		lock (_gate)
		{
			ThrowIfDisposed();
			_eventHandlers.Remove(typeof(T));
		}
	}

	public bool HasSubscribersFor<T>() where T : IEvent
	{
		lock (_gate) return _eventHandlers.ContainsKey(typeof(T));
	}

	public async UniTask Publish<T>(T eventData) where T : IEvent
	{
		if (eventData == null) throw new ArgumentNullException(nameof(eventData));
		var publication = new Publication(typeof(T), eventData);
		bool startDrain;
		lock (_gate)
		{
			ThrowIfDisposed();
			int priority = eventData.Priority;
			if (_leases.TryGetValue(publication.Data, out var lease))
			{
				if (!lease.CheckedOut)
					throw new InvalidOperationException("This event has already been published.");
				lease.CheckedOut = false;
			}
			if (!_eventHandlers.ContainsKey(typeof(T)))
			{
				ReturnLease(publication.Data);
				return;
			}
			_queue.Enqueue(publication, priority);
			startDrain = !_draining;
			_draining = true;
		}

		if (startDrain) DrainAsync().Forget();
		await publication.Completion.Task;
	}

	public UniTask PublishAsync<T>(T eventData) where T : IEvent => Publish(eventData);

	private async UniTask DrainAsync()
	{
		while (true)
		{
			Publication publication;
			Delegate[] handlers;
			lock (_gate)
			{
				if (!_queue.TryDequeue(out publication))
				{
					_draining = false;
					return;
				}
				_active = publication;
				if (_eventHandlers.TryGetValue(publication.EventType, out var registered))
				{
					handlers = new Delegate[registered.Count];
					registered.CopyTo(handlers);
				}
				else handlers = Array.Empty<Delegate>();
			}

			Exception? error = null;
			try
			{
				foreach (var handler in handlers)
				{
					if (publication.Canceled) break;
					if (publication.Data.RunOnThreadPool)
						await UniTask.RunOnThreadPool(() => Invoke(publication, handler));
					else
					{
						await UniTask.SwitchToMainThread();
						Invoke(publication, handler);
					}
				}
			}
			catch (Exception exception) { error = exception; }
			lock (_gate)
			{
				_active = null;
				ReturnLease(publication.Data);
			}

			if (error != null) publication.Completion.TrySetException(error);
			else if (publication.Canceled) publication.Completion.TrySetCanceled();
			else publication.Completion.TrySetResult();
		}
	}

	private static void Invoke(Publication publication, Delegate handler)
	{
		if (publication.Canceled) return;
		try { handler.DynamicInvoke(publication.Data); }
		catch (Exception exception)
		{
			Logger.LogError($"Error processing event {publication.EventType.Name}: {exception}");
		}
	}

	// A fresh instance is required: IEvent offers no operation to restore constructor defaults.
	public TEvent CreateEvent<TEvent>() where TEvent : IEvent, new()
	{
		lock (_gate) ThrowIfDisposed();
		return new TEvent();
	}

	public TEvent CreateEvent<TEvent, TPayload>(TPayload payload) where TEvent : IEvent<TPayload>, new()
	{
		TEvent item;
		lock (_gate)
		{
			ThrowIfDisposed();
			if (typeof(TEvent).IsValueType) item = new TEvent();
			else
			{
				if (!_eventPools.TryGetValue(typeof(TEvent), out var pool))
				{
					pool = new TypeSafeEventPool<TEvent>(100, () => new TEvent());
					_eventPools.Add(typeof(TEvent), pool);
				}
				item = ((TypeSafeEventPool<TEvent>)pool).Get();
				_leases.GetValue(item, _ => new Lease()).CheckedOut = true;
			}
		}
		item.Initialize(payload);
		return item;
	}

	private void ReturnLease(IEvent data)
	{
		if (!_disposed && _leases.TryGetValue(data, out _) && _eventPools.TryGetValue(data.GetType(), out var pool))
			pool.Return(data);
	}

	private void ThrowIfDisposed()
	{
		if (_disposed) throw new ObjectDisposedException(nameof(EventBus));
	}
}
