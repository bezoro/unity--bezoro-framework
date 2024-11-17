namespace Bezoro.Framework.Unity.EventSystem
{
	public interface IEvent<TPayload> : IEvent
	{
		TPayload Payload { get; }

		void Initialize( TPayload payload, int priority = 1, bool runOnThreadPool = false );
	}

	public interface IEvent
	{
		int Priority { get; }

		bool RunOnThreadPool { get; }
	}
}
