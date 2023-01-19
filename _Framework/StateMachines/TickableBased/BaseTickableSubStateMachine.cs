using System.Collections;
using UnityEngine;
using System.Collections.Generic;

namespace Bezoro.StateMachines
{
	public abstract class BaseTickableSubStateMachine : ITickableState, ITickableStateMachine
	{
		public ITickableState CurrentState => _currentState;

		public ITickableState LastState => _lastState;

		public virtual void Init
		(
			ITickableStateMachine owningStateMachine,
			BaseTickableStateMachine masterStateMachine
		)
		{
			_owningStateMachine = owningStateMachine;
			_masterStateMachine = masterStateMachine;
			SetUp();
		}

		public TickTransition TransitionsTo(ITickableState state)
		{
			var transition = new TickTransition(this, state, _masterStateMachine);
			_transitions.Add(transition);
			return transition;
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

		public void EarlyTick()
		{
			OnEarlyTick();
			_currentState.EarlyTick();
		}

		public void Tick()
		{
			OnTick();
			_currentState.Tick();
		}

		public void FixedTick()
		{
			OnFixedTick();
			_currentState.FixedTick();
		}

		public void LateTick()
		{
			OnLateTick();
			_currentState.LateTick();
			TickTransitions();
		}

		public void OnTransitionTrue(ITickableState toState) =>
			_owningStateMachine.DoTransition(toState);

		public void Exit()
		{
			_currentState.Exit();

			for (var i = 0; i < _transitions.Count; i++)
				_transitions[i].CeaseRelevant();

			OnExit();
		}

		public void DoTransition(ITickableState toState)
		{
			_lastState = _currentState;
			_currentState.Exit();
			_currentState = toState;
			_currentState.Enter();
		}

		#region Protected Abstract Methods
		
		protected abstract void SetUp();

		protected abstract void OnEnter();

		protected abstract void OnEarlyTick();

		protected abstract void OnTick();

		protected abstract void OnFixedTick();

		protected abstract void OnLateTick();

		protected abstract void OnDisable();

		protected abstract void OnExit();
		
		#endregion

		protected Coroutine StartCoroutine(IEnumerator routine) =>
			_masterStateMachine.StartCoroutine(routine);
		
		protected void StopCoroutine(Coroutine routine) =>
			_masterStateMachine.StopCoroutine(routine);

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

		protected void SetStartState(IEventState state) =>
			_startState = state;

		protected void RegisterState<T>(out T outState)
		where T : class, ITickableState, new() =>
			outState = CreateState<T>();

		private T CreateState<T>()
		where T : class, ITickableState, new()
		{
			var newState = new T();
			newState.Init(this, _masterStateMachine);
			return newState;
		}

		private void TickTransitions()
		{
			for (var i = 0; i < _transitions.Count; i++)
				_transitions[i].Tick();
		}

		private ITickableState _startState = new NullTickableState();
		private ITickableState _currentState = new NullTickableState();
		private ITickableState _lastState = new NullTickableState();
		private ITickableStateMachine _owningStateMachine;
		private BaseTickableStateMachine _masterStateMachine;

		private readonly List<TickTransition> _transitions = new(4);
	}
}