using UnityEngine;
using Bezoro.Core.Helpers;

namespace Bezoro.StateMachines
{
	public abstract class BaseEventSubStateMachine :
		IEventSubStateMachine
	{
		public void Init
		(MonoBehaviour masterStateMachine,
		 IEventStateMachine owningStateMachine)
		{
			_masterStateMachine = masterStateMachine;
			_owningStateMachine = owningStateMachine;
			SetUp();
		}

		void IState.Init
		(MonoBehaviour masterStateMachine,
		 IStateMachine owningStateMachine)
		{
			_masterStateMachine = masterStateMachine;
			_owningStateMachine = owningStateMachine as IEventStateMachine;
			SetUp();
		}

		public IEventState StartState => _startState;

		public IEventState CurrentState => _currentState;

		public IEventState LastState => _lastState;

		public MonoBehaviour MasterStateMachine => _masterStateMachine;

		public ITransition[] Transitions => _transitions;

		IState IStateMachine.StartState => _startState;

		IState IStateMachine.CurrentState => _currentState;

		IState IStateMachine.LastState => _lastState;

		public EventTransition TransitionsTo(IEventState toState)
		{
			var newTransition = new EventTransition(this, toState, _masterStateMachine);
			ArrayHelpers.Add(ref _transitions, newTransition);
			return newTransition;
		}

		public void SetCurrentState(IEventState state) =>
			_currentState = state;

		public void SetLastState(IEventState state) =>
			_lastState = state;

		void IStateMachine.SetCurrentState(IState state) =>
			_currentState = state as IEventState;

		void IStateMachine.SetLastState(IState state) =>
			_lastState = state as IEventState;

		public void Enter()
		{
			_currentState = _startState;
			OnEnter();
			this.AllTransitionsBecomeRelevant();
			_currentState.Enter();
		}

		public void Disable()
		{
			OnDisable();
			this.Disable_Ext();
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

		protected abstract void SetUp();

		protected abstract void OnEnter();

		protected abstract void OnDisable();

		protected abstract void OnExit();

		#endregion

		protected void SetStartState(IEventState state) =>
			_startState = state;

		protected void RegisterState<T>(out T outState)
		where T : class, IEventState, new() =>
			outState = this.CreateState<T>(_masterStateMachine);

		private IEventState _startState = new NullEventState();
		private IEventState _currentState = new NullEventState();
		private IEventState _lastState = new NullEventState();
		private IEventStateMachine _owningStateMachine;
		private MonoBehaviour _masterStateMachine;
		private ITransition[] _transitions = new ITransition[4];
	}
}