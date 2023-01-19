using System.Collections.Generic;
using UnityEngine;

namespace Bezoro.StateMachines
{
	public abstract class BaseSubEventStateMachine : IEventStateMachine, IEventState
	{
		public IEventState CurrentState => _currentState;

		public IEventState LastState => _lastState;

		public virtual void Init
		(
			IEventStateMachine owningStateMachine,
			BaseEventStateMachine masterStateMachine
		)
		{
			_owningStateMachine = owningStateMachine;
			_masterStateMachine = masterStateMachine;
			SetUp();
		}

		public EventTransition TransitionsTo(BaseEventState toState)
		{
			var newTransition = new EventTransition(this, toState, _masterStateMachine);
			_transitions.Add(newTransition);
			return newTransition;
		}

		public void Enter()
		{
			_currentState = _startState;

			OnEnter();

			for (var i = 0; i < _transitions.Count; i++)
				_transitions[i].BecomeRelevant();

			_currentState.Enter();
		}

		public void Disable()
		{
			OnDisable();

			_currentState.Disable();

			if (_masterStateMachine.DisableMethod is DisableMethod.Restart)
				_currentState = _startState;
		}

		public void DoTransition(IEventState toState)
		{
			_lastState = _currentState;
			_currentState.Exit();
			_currentState = toState;
			_currentState.Enter();
		}

		public void OnTransitionRaised(IEventState toState) =>
			_owningStateMachine.DoTransition(toState);

		public void Exit()
		{
			_currentState.Exit();

			for (var i = 0; i < _transitions.Count; i++)
				_transitions[i].CeaseRelevant();

			OnExit();
		}

		protected abstract void SetUp();

		protected abstract void OnEnter();

		protected abstract void OnDisable();

		protected abstract void OnExit();

		protected bool TryGetComponent<T>(out T component)
		where T : Component
		{
			if (_masterStateMachine.TryGetComponent<T>(out var c))
			{
				component = c;
				return true;
			}

			component = null;
			return false;
		}

		protected void RegisterStartState<T>()
		where T : class, IEventState, new() =>
			_startState = CreateState<T>();

		protected void RegisterState<T>(out T outState)
		where T : class, IEventState, new() =>
			outState = CreateState<T>();

		private T CreateState<T>()
		where T : class, IEventState, new()
		{
			var newState = new T();
			newState.Init(this, _masterStateMachine);
			return newState;
		}

		private IEventState _startState = new NullEventState();
		private IEventState _currentState = new NullEventState();
		private IEventState _lastState = new NullEventState();
		private IEventStateMachine _owningStateMachine;
		private BaseEventStateMachine _masterStateMachine;

		private readonly List<EventTransition> _transitions = new();
	}
}