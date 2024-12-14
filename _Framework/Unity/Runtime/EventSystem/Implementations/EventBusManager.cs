using System.Collections.Concurrent;
using Bezoro.Framework.Core;
using Bezoro.Framework.Unity;
using Bezoro.Framework.Unity.EventSystem;

public sealed class EventBusManager : Base_Singleton<EventBusManager>
{
	private EventBusManager()
	{
		_eventBuses = new();
	}

	private readonly ConcurrentDictionary<string, I_Event_Bus> _eventBuses;

	public I_Event_Bus GetOrCreateEventBus(string context)
	{
		Assert.Is_Not_Null_Or_Empty(context);
		Assert.Is_Not_Null(_eventBuses);

		return _eventBuses.GetOrAdd(
			context,
			_ => new EventBus()
		);
	}

	public bool TryGetEventBus(string context, out I_Event_Bus eventBus)
	{
		Assert.Is_Not_Null_Or_Empty(context);
		Assert.Is_Not_Null(_eventBuses);

		return _eventBuses.TryGetValue(
			context,
			out eventBus
		);
	}

	public bool RemoveEventBus(string context)
	{
		Assert.Is_Not_Null_Or_Empty(context);
		Assert.Is_Not_Null(_eventBuses);

		return _eventBuses.TryRemove(
			context,
			out _
		);
	}

	public void ClearAll()
	{
		Assert.Is_Not_Null(_eventBuses);

		_eventBuses.Clear();
	}

	protected override void On_Initialize() { }

	protected override void Dispose_Managed_Resources()
	{
		ClearAll();
	}

	protected override void On_Reset()
	{
		ClearAll();
	}
}
