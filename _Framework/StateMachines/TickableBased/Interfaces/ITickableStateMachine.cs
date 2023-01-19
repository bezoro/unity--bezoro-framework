namespace Bezoro.StateMachines
{
	public interface ITickableStateMachine
	{
		public ITickableState CurrentState { get; }

		public ITickableState LastState { get; }

		public void DoTransition(ITickableState toState);
	}
}