namespace Bezoro.StateMachines
{
	public interface ISimpleState
	{
		public void Init
		(
			ISimpleStateMachine owningStateMachine,
			BaseSimpleStateMachine masterStateMachine
		);

		public void Enter();
		public void Exit();
		public void SwitchOwningStateMachineCurrentState(ISimpleState toState);
	}
}