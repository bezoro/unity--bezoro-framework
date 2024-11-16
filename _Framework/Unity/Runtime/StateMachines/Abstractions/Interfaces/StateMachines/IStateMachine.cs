using Bezoro.Framework.Unity.States;

namespace Bezoro.Framework.Unity.StateMachines
{
	public interface IStateMachine<TState>
	where TState : IState
	{
		TState CurrentState { get; }

		TState LastState { get; }

		void DoTransition( TState toState );
	}
}
