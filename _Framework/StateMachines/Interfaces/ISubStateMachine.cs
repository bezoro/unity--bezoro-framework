namespace Bezoro.StateMachines
{
	public interface IKCCSubStateMachine : ISubStateMachine, IKCCStateMachine, IKCCState { }

	public interface ITickableSubStateMachine : ISubStateMachine, ITickableStateMachine,
		ITickableState { }

	public interface IEventSubStateMachine : ISubStateMachine, IEventStateMachine, IEventState { }

	public interface ISubStateMachine : IStateMachine, IState { }
}