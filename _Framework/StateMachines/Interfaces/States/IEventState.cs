namespace Bezoro.StateMachines
{
	public interface IEventState : IState, IHasTransitions<EventTransition, IEventState>
	{
		void Init(EventStateMachineOwner owner, IEventStateMachine owningStateMachine);

		void Enable();

		void Disable();

		void Destroy();
	}
}