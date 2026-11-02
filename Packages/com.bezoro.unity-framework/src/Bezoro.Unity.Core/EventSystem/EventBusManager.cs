using System;
using System.Collections.Concurrent;
using Bezoro.Unity.Core.Common.Patterns;

namespace Bezoro.Unity.Core.EventSystem;

/// <summary>
///     Process-wide registry of named event buses. Context names keep gameplay,
///     UI, input, and other event streams isolated while still allowing shared
///     singleton access.
/// </summary>
public sealed class EventBusManager : Singleton<EventBusManager>
{
	private readonly ConcurrentDictionary<string, IEventBus> _eventBuses;

	private EventBusManager()
	{
		_eventBuses = new();
	}

	/// <summary>
	///     Removes a bus from the registry without disposing subscribers explicitly.
	/// </summary>
	public bool RemoveEventBus(string context)
	{
		if (string.IsNullOrEmpty(context))
			throw new ArgumentException("Event bus context is required.", nameof(context));

		return _eventBuses.TryRemove(context, out _);
	}

	/// <summary>
	///     Looks up an existing event bus for a context without creating one.
	/// </summary>
	public bool TryGetEventBus(string context, out IEventBus eventBus)
	{
		if (string.IsNullOrEmpty(context))
		{
			eventBus = null!;
			return false;
		}

		return _eventBuses.TryGetValue(
			context,
			out eventBus
		);
	}

	/// <summary>
	///     Gets the existing bus for a context or creates a fresh
	///     <see cref="EventBus" /> when none has been registered.
	/// </summary>
	public IEventBus GetOrCreateEventBus(string context)
	{
		if (string.IsNullOrEmpty(context))
			throw new ArgumentException("Event bus context is required.", nameof(context));

		return _eventBuses.GetOrAdd(
			context,
			_ => new EventBus()
		);
	}

	/// <summary>
	///     Removes all context-to-bus mappings from the registry.
	/// </summary>
	public void ClearAll()
	{
		_eventBuses.Clear();
	}

	protected override void DisposeManagedResources() =>
		ClearAll();

	protected override void OnInitialize() { }

	protected override void OnReset() =>
		ClearAll();
}
