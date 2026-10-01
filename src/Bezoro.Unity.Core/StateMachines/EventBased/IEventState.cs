namespace Bezoro.Unity.Core.StateMachines.EventBased;

/// <summary>
///     Defines a state used by an event-based state machine.
/// </summary>
public interface IEventState : IState,
	IHasTransitions<EventTransition, IEventState>
{
	public void Init(EventStateMachineOwner owner, IEventStateMachine owningStateMachine);
}
