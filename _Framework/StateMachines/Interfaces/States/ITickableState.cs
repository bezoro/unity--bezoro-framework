using UnityEngine;

namespace Bezoro.StateMachines
{
	public interface ITickableState : IState, ITickable, IHasTransitions<ITickableState>
	{
		void Init(MonoBehaviour masterStateMachine, ITickableStateMachine owningStateMachine);
	}
}