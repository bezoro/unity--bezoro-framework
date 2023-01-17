namespace Bezoro.StateMachines
{
	public interface IKCCSuperState : IKCCState
	{
		public IKCCState LastState { get; }

		public IKCCState CurrentState { get; }

		public void DoTransition(IKCCState toState);
	}
}