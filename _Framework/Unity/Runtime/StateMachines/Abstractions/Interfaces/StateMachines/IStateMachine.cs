namespace Bezoro.Framework.Unity.StateMachines
{
	public interface IStateMachine<TState>
	where TState : IState
	{
		public TState CurrentState { get; }

		public TState LastState { get; }

		public void DoTransition( TState toState );
	}
}
