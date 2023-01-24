using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Bezoro.StateMachines
{
	public abstract class BaseEventState : IEventState
	{
		public virtual void Init
		(MonoBehaviour masterStateMachine,
		 IEventStateMachine owningStateMachine)
		{
			_masterStateMachine = masterStateMachine;
			_owningStateMachine = owningStateMachine;
			OnInit();
		}

		void IState.Init
		(MonoBehaviour masterStateMachine,
		 IStateMachine owningStateMachine)
		{
			_masterStateMachine = masterStateMachine;
			_owningStateMachine = owningStateMachine;
		}

		public EventTransition TransitionsTo(IEventState toState)
		{
			var newTransition = new EventTransition(this, toState, _masterStateMachine);
			_transitions.Add(newTransition);
			return newTransition;
		}

		public void Enter()
		{
			OnEnter();

			for (var i = 0; i < _transitions.Count; i++)
				_transitions[i].BecomeRelevant();
		}

		public void Disable() =>
			OnDisable();

		public void OnTransitionRaised(IEventState toState) =>
			_owningStateMachine.DoTransition(toState);

		public void Exit()
		{
			for (var i = 0; i < _transitions.Count; i++)
				_transitions[i].CeaseRelevant();

			OnExit();
		}

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

		protected abstract void OnInit();

		protected abstract void OnEnter();

		protected abstract void OnDisable();

		protected abstract void OnExit();

		private IStateMachine _owningStateMachine;
		private MonoBehaviour _masterStateMachine;

		private readonly List<EventTransition> _transitions = new();
	}

	public sealed class NullEventState : IEventState
	{
		public void Init
		(MonoBehaviour masterStateMachine,
		 IEventStateMachine owningStateMachine) { }

		void IState.Init
		(MonoBehaviour masterStateMachine,
		 IStateMachine owningStateMachine) { }

		public void Enter() { }

		public void Exit() { }

		public void Disable() { }

		public void OnTransitionRaised(IEventState toState) { }
	}
}