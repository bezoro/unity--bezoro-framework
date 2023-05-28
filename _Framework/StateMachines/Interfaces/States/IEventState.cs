namespace Bezoro.Framework.StateMachines
{
	public interface IEventState : IState, IHasTransitions<EventTransition, IEventState>
	{
		void Init(EventStateMachineOwner owner, IEventStateMachine owningStateMachine);
	}
}