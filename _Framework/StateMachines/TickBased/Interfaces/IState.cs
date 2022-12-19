namespace Bezoro.StateMachines
{
	public interface IState
	{
		public void Init(IStateMachine owningStateMachine, BaseStateMachine masterStateMachine);

		public void Enter();

		public void EarlyTick();

		public void Tick();

		public void FixedTick();

		public void LateTick();

		public void Exit();

		public void SwitchOwningStateMachineCurrentState(IState toState);
	}
}