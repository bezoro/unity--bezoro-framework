namespace Bezoro.Unity.Core.EventSystem;

/// <summary>
///     Typed event that carries a payload while still participating in the shared
///     event-priority and threading contract.
/// </summary>
public interface IEvent<TPayload> : IEvent
{
	/// <summary>
	///     Gets the payload assigned during initialization.
	/// </summary>
	public TPayload? Payload { get; }

	/// <summary>
	///     Initializes a reusable event instance before publication.
	/// </summary>
	public void Initialize(TPayload payload, int priority = 1, bool runOnThreadPool = false);
}

/// <summary>
///     Base event metadata understood by the event bus regardless of payload type.
/// </summary>
public interface IEvent
{
	/// <summary>
	///     Gets whether handlers for this event should be invoked from a worker
	///     thread when the bus supports background dispatch.
	/// </summary>
	public bool RunOnThreadPool { get; }

	/// <summary>
	///     Gets the processing priority; higher-level bus code uses this to order
	///     queued events.
	/// </summary>
	public int Priority { get; }
}
