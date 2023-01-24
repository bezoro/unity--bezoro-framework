namespace Bezoro.StateMachines
{
	public interface IKCCSubStateMachine : IKCCStateMachine, IKCCState { }

	public interface ITickableSubStateMachine : ITickableStateMachine, ITickableState { }

	public interface IEventSubStateMachine : IEventStateMachine, IEventState { }

	public interface ISubStateMachine : IStateMachine, IState { }
}