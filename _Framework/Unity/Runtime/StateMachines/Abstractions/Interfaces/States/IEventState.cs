using Bezoro.Framework.Unity.StateMachines;

namespace Bezoro.Framework.Unity.States
{
	public interface IEventState : IState, IHasTransitions<EventTransition, IEventState>
	{
		void Init(EventStateMachineOwner owner, IEventStateMachine owningStateMachine);
	}
}
