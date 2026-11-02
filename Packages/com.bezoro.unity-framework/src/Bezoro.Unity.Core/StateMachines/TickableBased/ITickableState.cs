namespace Bezoro.Unity.Core.StateMachines.TickableBased;

/// <summary>
///     Defines a state updated by a tickable state machine.
/// </summary>
public interface ITickableState : IState,
	ITickable,
	IHasTransitions<TickTransition, ITickableState>
{
	public void Init(TickableStateMachineOwner owner, ITickableStateMachine owningStateMachine);
}
