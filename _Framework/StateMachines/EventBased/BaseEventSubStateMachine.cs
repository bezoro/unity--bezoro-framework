using UnityEngine;
using Bezoro.Core.Helpers;

namespace Bezoro.StateMachines
{
	public abstract class BaseEventSubStateMachine :
		IEventSubStateMachine
	{
		public void Init(MonoBehaviour masterStateMachine, IEventStateMachine owningStateMachine)
		{
			_masterStateMachine = masterStateMachine;
			_owningStateMachine = owningStateMachine;
			RegisterStates();
			SetUpTransitions();
		}

		void IState.Init(MonoBehaviour masterStateMachine, IStateMachine owningStateMachine) =>
			Init(masterStateMachine, owningStateMachine as IEventStateMachine);

		#region Public Properties

		public IEventState CurrentState => _currentState;

		IState IStateMachine.CurrentState => _currentState;

		public IEventState LastState => _lastState;

		IState IStateMachine.LastState => _lastState;

		public MonoBehaviour MasterStateMachine => _masterStateMachine;

		public ITransition[] Transitions => _transitions;

		#endregion

		public void Enter()
		{
			OnEnter();
			this.Enter_Ext();
		}

		public void OnTransitionRaised(IEventState toState) =>
			_owningStateMachine.DoTransition(toState);

		public void Exit()
		{
			this.Exit_Ext();
			OnExit();
		}

		public IEventTransition TransitionsTo(IEventState toState)
		{
			var newTransition = this.CreateTransition<EventTransition>(toState);
			ArrayHelpers.Add(ref _transitions, newTransition);
			return newTransition;
		}

		public void SetCurrentState(IEventState state) =>
			_currentState = state;

		void IStateMachine.SetCurrentState(IState state) =>
			SetCurrentState(state as IEventState);

		public void SetLastState(IEventState state) =>
			_lastState = state;

		void IStateMachine.SetLastState(IState state) =>
			SetLastState(state as IEventState);

		public void DoTransition(IEventState toState) =>
			this.DoTransition_Ext(toState);

		void IStateMachine.DoTransition(IState toState) =>
			this.DoTransition_Ext(toState);

		#region Abstract Methods

		public abstract void RegisterStates();

		public abstract void SetUpTransitions();

		public abstract IState ResolveStartState();

		protected abstract void OnEnter();

		protected abstract void OnExit();

		#endregion

		protected T RegisterState<T>()
		where T : class, IEventState, new() =>
			this.CreateState<T, IEventStateMachine>(_masterStateMachine);

		private IEventState _currentState = new NullEventState();
		private IEventState _lastState = new NullEventState();
		private IEventStateMachine _owningStateMachine;
		private MonoBehaviour _masterStateMachine;
		private ITransition[] _transitions = new ITransition[4];
	}
}