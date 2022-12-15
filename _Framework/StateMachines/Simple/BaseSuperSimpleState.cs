using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bezoro.StateMachines
{
	public abstract class BaseSuperSimpleState<TAgent> : BaseSuperSimpleState
	where TAgent : MonoBehaviour
	{
		public override void Init
		(
			ISimpleStateMachine owningStateMachine,
			BaseSimpleStateMachine masterStateMachine
		)
		{
			Agent = masterStateMachine.GetComponent<TAgent>();
			base.Init(owningStateMachine, masterStateMachine);
		}

		protected Coroutine StartCoroutine(IEnumerator routine) =>
			Agent.StartCoroutine(routine);

		protected TAgent Agent { get; private set; }
	}

	public abstract class BaseSuperSimpleState : ISimpleState, ISimpleStateMachine
	{
		public ISimpleState CurrentState => _currentState;

		public virtual void Init
		(
			ISimpleStateMachine owningStateMachine,
			BaseSimpleStateMachine masterStateMachine
		)
		{
			_owningStateMachine = owningStateMachine;
			_masterStateMachine = masterStateMachine;
			SetUp();
		}

		public SimpleTransition TransitionsTo(BaseSimpleState toState)
		{
			var newTransition = new SimpleTransition(this, toState, _masterStateMachine);
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

		public void SwitchCurrentState(ISimpleState stateToSwitchTo)
		{
			_currentState.Exit();
			_currentState = stateToSwitchTo;
			_currentState.Enter();
		}

		public void SwitchOwningStateMachineCurrentState(ISimpleState toState) =>
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
		where TState : ISimpleState, new()
		{
			var newState = new TState();
			newState.Init(this, _masterStateMachine);
			createdState = newState;

			if (_currentState is NullSimpleState)
				_currentState = newState;
		}

		private ISimpleState _currentState = new NullSimpleState();
		private ISimpleStateMachine _owningStateMachine;
		private BaseSimpleStateMachine _masterStateMachine;

		private readonly List<SimpleTransition> _transitions = new();
	}
}