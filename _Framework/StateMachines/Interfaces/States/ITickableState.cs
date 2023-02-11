namespace Bezoro.StateMachines
{
	public interface ITickableState : IState, ITickable,
		IHasTransitions<TickTransition, ITickableState>
	{
		void Init(TickableStateMachineOwner owner, ITickableStateMachine owningStateMachine);
	}
}