namespace Bezoro.Framework.Unity.EventSystem
{
	public interface IEvent<TPayload> : IEvent
	{
		public TPayload Payload { get; }

		public void Initialize( TPayload payload, int priority = 1, bool runOnThreadPool = false );
	}

	public interface IEvent
	{
		public int Priority { get; }

		public bool RunOnThreadPool { get; }
	}
}
