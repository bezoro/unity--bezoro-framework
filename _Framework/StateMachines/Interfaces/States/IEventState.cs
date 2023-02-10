namespace Bezoro.StateMachines
{
	public interface IEventState : IState, IHasTransitions<EventTransition, IEventState>
	{
		void Init(EventStateMachineOwner owner, IEventStateMachine owningStateMachine);

		void Disable();

		void Enable();

		void Destroy();
	}
}