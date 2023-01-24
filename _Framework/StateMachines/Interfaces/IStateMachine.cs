namespace Bezoro.StateMachines
{
	public interface IKCCStateMachine : ITickableStateMachine
	{
		public new IKCCState CurrentState { get; }

		public new IKCCState LastState { get; }

		public void DoTransition(IKCCState toState);
	}

	public interface ITickableStateMachine : IStateMachine
	{
		public new ITickableState CurrentState { get; }

		public new ITickableState LastState { get; }

		public void DoTransition(ITickableState toState);
	}

	public interface IEventStateMachine : IStateMachine
	{
		public new IEventState StartState { get; }

		public new IEventState CurrentState { get; }

		public new IEventState LastState { get; }

		public void DoTransition(IEventState toState);

		public void SetCurrentState(IEventState state);

		public void SetLastState(IEventState state);
	}

	public interface IStateMachine
	{
		public IState StartState { get; }

		public IState CurrentState { get; }

		public IState LastState { get; }

		public DisableMethod DisableMethod { get; }

		public void DoTransition(IState toState);

		public void SetCurrentState(IState state);

		public void SetLastState(IState state);
	}
}