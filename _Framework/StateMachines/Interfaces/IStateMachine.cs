namespace Bezoro.StateMachines
{
	public interface IKCCStateMachine : IStateMachine<IKCCState> { }

	public interface ITickableStateMachine : IStateMachine<ITickableState> { }

	public interface IEventStateMachine : IStateMachine<IEventState> { }

	public interface IStateMachine<TState>
	where TState : IState
	{
		TState CurrentState { get; }

		TState LastState { get; }

		void DoTransition(TState toState);
	}
}