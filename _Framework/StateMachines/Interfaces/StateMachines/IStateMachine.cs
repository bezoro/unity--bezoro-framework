namespace Bezoro.Framework.StateMachines
{
	public interface IStateMachine<TState>
	where TState : IState
	{
		TState CurrentState { get; }

		TState LastState { get; }

		void DoTransition(TState toState);
	}
}