namespace Bezoro.StateMachines
{
	public interface IKCCStateMachine
	{
		IKCCState CurrentState { get; }

		void DoTransition(IKCCState toState);
	}

	public interface ITickableStateMachine
	{
		ITickableState CurrentState { get; }

		void DoTransition(ITickableState toState);
	}

	public interface IEventStateMachine
	{
		IEventState CurrentState { get; }

		void DoTransition(IEventState toState);
	}
}