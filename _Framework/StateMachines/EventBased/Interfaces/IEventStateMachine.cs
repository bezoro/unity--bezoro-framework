namespace Bezoro.StateMachines
{
	public interface IEventStateMachine
	{
		IEventState CurrentState { get; }

		IEventState LastState { get; }

		public void DoTransition(IEventState toState);
	}
}