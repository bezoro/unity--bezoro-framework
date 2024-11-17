namespace Bezoro.Framework.Unity.EventSystem
{
	public abstract class BaseEvent<TPayload> : BaseEvent, IEvent<TPayload>
	{
		public TPayload Payload { get; private set; }

		public void Initialize( TPayload payload, int priority = 1, bool runOnThreadPool = false )
		{
			Payload = payload;
			base.Initialize( priority, runOnThreadPool );
		}
	}

	public abstract class BaseEvent : IEvent
	{
		public int Priority { get; protected set; } = 1;
		public bool RunOnThreadPool { get; protected set; }

		protected virtual void Initialize( int priority = 1, bool runOnThreadPool = false )
		{
			Priority        = priority;
			RunOnThreadPool = runOnThreadPool;
		}
	}
}
