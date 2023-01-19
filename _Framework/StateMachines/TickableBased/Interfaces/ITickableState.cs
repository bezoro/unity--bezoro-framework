namespace Bezoro.StateMachines
{
	public interface ITickableState
	{
		public void Init(ITickableStateMachine owningStateMachine, BaseTickableStateMachine masterStateMachine);

		public void Enter();

		public void EarlyTick();

		public void Tick();

		public void FixedTick();

		public void LateTick();

		public void Exit();

		public void Disable();

		public void OnTransitionTrue(ITickableState toState);
	}
}