using UnityEngine;

namespace Bezoro.StateMachines
{
	public interface IEventState : IState, IHasTransitions<IEventState>
	{
		void Init(MonoBehaviour masterStateMachine, IEventStateMachine owningStateMachine);
	}
}