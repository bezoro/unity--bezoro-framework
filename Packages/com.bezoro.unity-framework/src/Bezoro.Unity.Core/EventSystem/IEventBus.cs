using System;
using Bezoro.Unity.Core.DI;
using Cysharp.Threading.Tasks;

namespace Bezoro.Unity.Core.EventSystem;

/// <summary>
///     Publishes pooled, typed events to unique subscribers and provides the
///     lifecycle operations needed to clear queued work during scene or system
///     teardown.
/// </summary>
public interface IEventBus : IService,
	IDisposable
{
	/// <summary>
	///     Determines whether at least one handler is registered for the event type.
	/// </summary>
	public bool HasSubscribersFor<T>()
		where T : IEvent;

	/// <summary>
	///     Creates an event instance with default event metadata. Implementations may
	///     serve this from an internal pool.
	/// </summary>
	public TEvent CreateEvent<TEvent>()
		where TEvent : IEvent, new();

	/// <summary>
	///     Creates an event instance initialized with a payload and default priority.
	/// </summary>
	public TEvent CreateEvent<TEvent, TPayload>(TPayload payload)
		where TEvent : IEvent<TPayload>, new();

	/// <summary>
	///     Queues an event and completes after its own synchronous handlers return. A handler may
	///     enqueue nested work, which runs after the current event. Never block a handler
	///     waiting for nested work. Cancellation, clear and disposal cancel outstanding publications.
	///     Publishing a bus-created payload event transfers ownership back to its creating bus.
	/// </summary>
	public UniTask Publish<T>(T eventData)
		where T : IEvent;

	/// <summary>
	///     Equivalent to Publish. Completes when this publication finishes.
	/// </summary>
	public UniTask PublishAsync<T>(T eventData)
		where T : IEvent;

	/// <summary>
	///     Requests cancellation of the current processing loop without removing
	///     subscribers.
	/// </summary>
	public void CancelEventProcessing();

	/// <summary>
	///     Removes queued events and registered subscribers owned by this bus.
	/// </summary>
	public void Clear();

	/// <summary>
	///     Registers a handler for the event type, replacing duplicate registration
	///     attempts for the same delegate.
	/// </summary>
	public void SubscribeUnique<T>(Action<T> handler)
		where T : IEvent;

	/// <summary>
	///     Removes a previously registered handler for the event type.
	/// </summary>
	public void Unsubscribe<T>(Action<T> handler)
		where T : IEvent;

	/// <summary>
	///     Removes every handler registered for the event type.
	/// </summary>
	public void UnsubscribeAll<T>()
		where T : IEvent;
}
