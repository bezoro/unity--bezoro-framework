namespace Bezoro.StateMachines
{
	public interface IKCCStateMachine : ITickableStateMachine
	{
		public new IKCCState CurrentState { get; }

		public new IKCCState LastState { get; }

		public void SetCurrentState(IKCCState state);

		public void SetLastState(IKCCState state);

		public void DoTransition(IKCCState toState);
	}

	public interface ITickableStateMachine : IStateMachine
	{
		public new ITickableState CurrentState { get; }

		public new ITickableState LastState { get; }

		public void SetCurrentState(ITickableState state);

		public void SetLastState(ITickableState state);

		public void DoTransition(ITickableState toState);
	}

	public interface IEventStateMachine : IStateMachine
	{
		public new IEventState CurrentState { get; }

		public new IEventState LastState { get; }

		public void SetCurrentState(IEventState state);

		public void SetLastState(IEventState state);

		public void DoTransition(IEventState toState);
	}

	public interface IStateMachine
	{
		public IState CurrentState { get; }

		public IState LastState { get; }

		public void SetCurrentState(IState state);

		public void SetLastState(IState state);

		public void DoTransition(IState toState);

		public IState ResolveStartState();

		void RegisterStates();

		void SetUpTransitions();
	}
}