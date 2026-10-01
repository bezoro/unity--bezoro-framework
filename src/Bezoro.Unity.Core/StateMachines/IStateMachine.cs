namespace Bezoro.Unity.Core.StateMachines;

/// <summary>
///     Defines a state machine with an active state and state-change operations.
/// </summary>
public interface IStateMachine<TState>
	where TState : IState
{
	public TState CurrentState { get; }

	public TState LastState { get; }

	public void DoTransition(TState toState);
}
