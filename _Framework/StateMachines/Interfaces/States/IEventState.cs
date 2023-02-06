using UnityEngine;

namespace Bezoro.StateMachines
{
	public interface IEventState : IState, IHasTransitions<EventTransition, IEventState>
	{
		void Init(MonoBehaviour masterStateMachine, IEventStateMachine owningStateMachine);
	}
}