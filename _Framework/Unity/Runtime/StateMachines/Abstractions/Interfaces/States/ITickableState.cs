using Bezoro.Framework.Unity.StateMachines.TickableBased;

namespace Bezoro.Framework.Unity.StateMachines
{
	public interface ITickableState : IState,
		ITickable,
		IHasTransitions<TickTransition, ITickableState>
	{
		public void Init( TickableStateMachineOwner owner, ITickableStateMachine owningStateMachine );
	}
}
