using System.Collections.Concurrent;
using Bezoro.Framework.Unity;
using Bezoro.Framework.Unity.EventSystem;

public sealed class EventBusManager : BaseSingleton<EventBusManager>
{
	private readonly ConcurrentDictionary<string, IEventBus> _eventBuses = new();

	private EventBusManager() {}

	protected override void OnInitialize() {}

	protected override void DisposeManagedResources()
	{
		ClearAll();
	}

	public IEventBus GetOrCreateEventBus( string context )
	{
		return _eventBuses.GetOrAdd( context, _ => new EventBus() );
	}

	public bool TryGetEventBus( string context, out IEventBus eventBus ) =>
		_eventBuses.TryGetValue( context, out eventBus );

	public bool RemoveEventBus( string context ) => _eventBuses.TryRemove( context, out _ );

	public void ClearAll()
	{
		_eventBuses.Clear();
	}

	protected override void OnReset()
	{
		ClearAll();
	}
}
