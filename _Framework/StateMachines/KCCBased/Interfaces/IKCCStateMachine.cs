namespace Bezoro.StateMachines
{
	public interface IKCCStateMachine
	{
		public IKCCState CurrentState { get; }

		public IKCCState LastState { get; }

		public void DoTransition(IKCCState toState);
	}
}