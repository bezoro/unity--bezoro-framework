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

		public IEventState CurrentState => _currentState;

		IState IStateMachine.CurrentState => _currentState;

		public IEventState LastState => _lastState;

		IState IStateMachine.LastState => _lastState;

		public MonoBehaviour MasterStateMachine => _masterStateMachine;

		public ITransition[] Transitions => _transitions;

		public IEventTransition TransitionsTo(IEventState toState)
		{
			var newTransition = this.CreateTransition<EventTransition>(toState);
			ArrayHelpers.Add(ref _transitions, newTransition);
			return newTransition;
		}

		public void SetCurrentState(IEventState state) =>
			_currentState = state;

		void IStateMachine.SetCurrentState(IState state) =>
			_currentState = state as IEventState;

		public void SetLastState(IEventState state) =>
			_lastState = state;

		void IStateMachine.SetLastState(IState state) =>
			_lastState = state as IEventState;

		public void Enter()
		{
			OnEnter();
			this.AllTransitionsBecomeRelevant();
			_currentState = ResolveStartState();
			_currentState.Enter();
		}

		public void DoTransition(IEventState toState) =>
			this.DoTransition_Ext(toState);

		void IStateMachine.DoTransition(IState toState) =>
			this.DoTransition_Ext(toState);

		public void OnTransitionRaised(IEventState toState) =>
			_owningStateMachine.DoTransition(toState);

		public void Exit()
		{
			_currentState.Exit();
			this.AllTransitionsCeaseRelevant();
			OnExit();
		}

		#region Abstract Methods

		protected abstract void RegisterStates();

		protected abstract void SetUpTransitions();

		protected abstract IEventState ResolveStartState();

		protected abstract void OnEnter();

		protected abstract void OnExit();

		#endregion

		protected T RegisterState<T>()
		where T : class, IEventState, new() =>
			this.CreateState<T, IEventStateMachine>(_masterStateMachine);

		private IEventState _startState = new NullEventState();
		private IEventState _currentState = new NullEventState();
		private IEventState _lastState = new NullEventState();
		private IEventStateMachine _owningStateMachine;
		private MonoBehaviour _masterStateMachine;
		private ITransition[] _transitions = new ITransition[4];
	}
}