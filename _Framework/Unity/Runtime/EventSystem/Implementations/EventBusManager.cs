using System.Collections.Concurrent;
using Bezoro.Framework.Core;
using Bezoro.Framework.Unity;
using Bezoro.Framework.Unity.EventSystem;

public sealed class EventBusManager : BaseSingleton<EventBusManager>
{
	private readonly ConcurrentDictionary<string, IEventBus> _eventBuses;

	private EventBusManager()
	{
		_eventBuses = new();
	}

	protected override void OnInitialize() {}

	protected override void DisposeManagedResources()
	{
		ClearAll();
	}

	public IEventBus GetOrCreateEventBus( string context )
	{
		Assert.IsNotNullOrEmpty( context );
		Assert.IsNotNull( _eventBuses );

		return _eventBuses.GetOrAdd( context, _ => new EventBus() );
	}

	public bool TryGetEventBus( string context, out IEventBus eventBus )
	{
		Assert.IsNotNullOrEmpty( context );
		Assert.IsNotNull( _eventBuses );

		return _eventBuses.TryGetValue( context, out eventBus );
	}

	public bool RemoveEventBus( string context )
	{
		Assert.IsNotNullOrEmpty( context );
		Assert.IsNotNull( _eventBuses );

		return _eventBuses.TryRemove( context, out _ );
	}

	public void ClearAll()
	{
		Assert.IsNotNull( _eventBuses );

		_eventBuses.Clear();
	}

	protected override void OnReset()
	{
		ClearAll();
	}
}
