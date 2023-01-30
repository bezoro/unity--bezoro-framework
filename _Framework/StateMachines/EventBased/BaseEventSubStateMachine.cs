using UnityEngine;
using Bezoro.Core.Helpers;

namespace Bezoro.StateMachines
{
	public abstract class BaseEventSubStateMachine : IEventStateMachine, IEventState
	{
		public void Init(MonoBehaviour masterStateMachine, IEventStateMachine owningStateMachine)
		{
			_masterStateMachine = masterStateMachine;
			_owningStateMachine = owningStateMachine;
			RegisterStates();
			SetUpTransitions();
		}

		public IEventState CurrentState => _currentState;

		public IEventState LastState => _lastState;

		public MonoBehaviour MasterStateMachine => _masterStateMachine;

		public EventTransition[] Transitions => _transitions;

		public void Enter()
		{
			OnEnter();

			for (var i = 0; i < _transitions.Length; i++)
			{
				if (_transitions[i] == null)
					continue;

				_transitions[i].BecomeRelevant();
			}

			_currentState = ResolveStartState();
			_currentState.Enter();
		}

		public void OnTransitionRaised(IEventState toState) =>
			_owningStateMachine.DoTransition(toState);

		public void Exit()
		{
			_currentState.Exit();
			for (var i = 0; i < _transitions.Length; i++)
			{
				if (_transitions[i] == null)
					continue;

				_transitions[i].CeaseRelevant();
			}

			OnExit();
		}

		public EventTransition TransitionsTo(IEventState toState)
		{
			var newTransition = new EventTransition();
			newTransition.Init(this, toState, _masterStateMachine);
			ArrayHelpers.Add(ref _transitions, newTransition);
			return newTransition;
		}

		public void DoTransition(IEventState toState)
		{
			_currentState.Exit();
			_lastState = _currentState;
			_currentState = toState;
			_currentState.Enter();
		}

		#region Abstract Methods

		protected abstract void RegisterStates();

		protected abstract void SetUpTransitions();

		protected abstract BaseEventState ResolveStartState();

		protected abstract void OnEnter();

		protected abstract void OnExit();

		#endregion

		protected T RegisterState<T>()
		where T : class, IEventState, new()
		{
			var newState = new T();
			newState.Init(_masterStateMachine, this);
			return newState;
		}

		private IEventState _currentState;
		private IEventState _lastState;
		private IEventStateMachine _owningStateMachine;
		private MonoBehaviour _masterStateMachine;
		private EventTransition[] _transitions = new EventTransition[4];
	}
}