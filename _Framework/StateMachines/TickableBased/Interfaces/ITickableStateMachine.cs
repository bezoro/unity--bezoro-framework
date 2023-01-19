namespace Bezoro.StateMachines
{
	public interface ITickableStateMachine
	{
		ITickableState CurrentState { get; }

		ITickableState LastState { get; }

		public void DoTransition(ITickableState toState);
	}
}