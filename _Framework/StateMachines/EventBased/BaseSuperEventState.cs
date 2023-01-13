using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Bezoro.StateMachines
{
	public abstract class BaseSuperEventState<TAgent> : BaseSuperEventState
	where TAgent : MonoBehaviour
	{
		public override void Init
		(
			IEventStateMachine owningStateMachine,
			BaseEventStateMachine masterStateMachine
		)
		{
			Agent = masterStateMachine.GetComponent<TAgent>();
			base.Init(owningStateMachine, masterStateMachine);
		}

		protected Coroutine StartCoroutine(IEnumerator routine) =>
			Agent.StartCoroutine(routine);

		protected TAgent Agent { get; private set; }
	}

	public abstract class BaseSuperEventState : IEventState, IEventStateMachine
	{
		public IEventState CurrentState => _currentState;

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
			OnEnter();

			foreach (var transition in _transitions)
				transition.BecomeRelevant();

			_currentState.Enter();
		}

		public void SwitchCurrentState(IEventState stateToSwitchTo)
		{
			_currentState.Exit();
			_currentState = stateToSwitchTo;
			_currentState.Enter();
		}

		public void OnTransitionRaised(IEventState toState) =>
			_owningStateMachine.SwitchCurrentState(toState);

		public void Exit()
		{
			_currentState.Exit();

			foreach (var transition in _transitions)
				transition.CeaseRelevant();

			OnExit();
		}

		protected abstract void SetUp();

		protected abstract void OnEnter();

		protected abstract void OnExit();

		protected void RegisterState<TState>(out TState createdState)
		where TState : IEventState, new()
		{
			var newState = new TState();
			newState.Init(this, _masterStateMachine);
			createdState = newState;

			if (_currentState is NullEventState)
				_currentState = newState;
		}

		private IEventState _currentState = new NullEventState();
		private IEventStateMachine _owningStateMachine;
		private BaseEventStateMachine _masterStateMachine;

		private readonly List<EventTransition> _transitions = new();
	}
}