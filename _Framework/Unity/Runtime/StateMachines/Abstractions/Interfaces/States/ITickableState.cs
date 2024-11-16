using Bezoro.Framework.Unity.StateMachines;

namespace Bezoro.Framework.Unity.States
{
	public interface ITickableState : IState, ITickable,
		IHasTransitions<TickTransition, ITickableState>
	{
		void Init( TickableStateMachineOwner owner, ITickableStateMachine owningStateMachine );
	}
}
