using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bezoro.StateMachines
{
	public abstract class BaseSuperState<TAgent> : BaseSuperState
	where TAgent : MonoBehaviour
	{
		public override void Init
		(
			IStateMachine owningStateMachine,
			BaseStateMachine masterStateMachine
		)
		{
			Agent = masterStateMachine.GetComponent<TAgent>();
			base.Init(owningStateMachine, masterStateMachine);
		}

		protected Coroutine StartCoroutine(IEnumerator routine) =>
			Agent.StartCoroutine(routine);

		protected TAgent Agent { get; private set; }
	}

	public abstract class BaseSuperState : IState, IStateMachine
	{
		public IState CurrentState => _currentState;

		public virtual void Init
		(
			IStateMachine owningStateMachine,
			BaseStateMachine masterStateMachine
		)
		{
			_owningStateMachine = owningStateMachine;
			_masterStateMachine = masterStateMachine;
			SetUp();
		}

		public Transition TransitionsTo(IState state)
		{
			var transition = new Transition(this, state, _masterStateMachine);
			_transitions.Add(transition);
			return transition;
		}

		public void Enter()
		{
			_currentState = _startingState;

			OnEnter();

			foreach (var transition in _transitions)
				transition.BecomeRelevant();

			_currentState.Enter();
		}

		public void Tick()
		{
			OnTick();
			_currentState.Tick();
			CheckTransitions();
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
		}

		public void SwitchCurrentState(IState stateToSwitchTo)
		{
			_currentState.Exit();
			_currentState = stateToSwitchTo;
			_currentState.Enter();
		}

		public void SwitchOwningStateMachineCurrentState(IState toState) =>
			_owningStateMachine.SwitchCurrentState(toState);

		public void Exit()
		{
			foreach (var transition in _transitions)
				transition.CeaseRelevant();

			_currentState.Exit();
			OnExit();
		}

		protected void RegisterState<TState>(out TState createdState)
		where TState : IState, new()
		{
			var newState = new TState();
			newState.Init(this, _masterStateMachine);
			createdState = newState;

			if (_startingState is NullState)
				_startingState = newState;
		}

		protected abstract void SetUp();
		protected abstract void OnEnter();
		protected abstract void OnTick();
		protected abstract void OnFixedTick();
		protected abstract void OnLateTick();
		protected abstract void OnExit();

		private void CheckTransitions()
		{
			foreach (var transition in _transitions)
				if (transition.CheckConditions())
					return;
		}

		private IState _startingState = new NullState();
		private IState _currentState = new NullState();
		private IStateMachine _owningStateMachine;
		private BaseStateMachine _masterStateMachine;

		private readonly List<Transition> _transitions = new();
	}
}