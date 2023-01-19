namespace Bezoro.StateMachines
{
	public interface IEventState
	{
		public void Init
		(
			IEventStateMachine owningStateMachine,
			BaseEventStateMachine masterStateMachine
		);

		public void Enter();

		public void Exit();

		public void Disable();

		public void OnTransitionRaised(IEventState toState);
	}
}