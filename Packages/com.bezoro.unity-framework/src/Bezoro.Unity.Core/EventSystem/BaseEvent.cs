namespace Bezoro.Unity.Core.EventSystem;

/// <summary>
///     Reusable base implementation for payload events created by
///     <see cref="IEventBus" /> pools.
/// </summary>
public abstract class BaseEvent<TPayload> : BaseEvent,
	IEvent<TPayload>
{
	/// <summary>
	///     Stores the payload and event metadata before publication.
	/// </summary>
	public void Initialize(TPayload payload, int priority = 1, bool runOnThreadPool = false)
	{
		Payload = payload;

		base.Initialize(
			priority,
			runOnThreadPool
		);
	}

	/// <summary>
	///     Gets the payload assigned during the latest initialization.
	/// </summary>
	public TPayload? Payload { get; private set; }
}

/// <summary>
///     Reusable base implementation for event priority and dispatch-thread
///     metadata.
/// </summary>
public abstract class BaseEvent : IEvent
{
	/// <summary>
	///     Updates metadata shared by all event instances before publication.
	/// </summary>
	protected virtual void Initialize(int priority = 1, bool runOnThreadPool = false)
	{
		Priority        = priority;
		RunOnThreadPool = runOnThreadPool;
	}

	/// <inheritdoc />
	public bool RunOnThreadPool { get; protected set; }

	/// <inheritdoc />
	public int Priority { get; protected set; } = 1;
}
