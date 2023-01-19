using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bezoro.StateMachines
{
	public abstract class BaseEventState : IEventState
	{
		public virtual void Init
		(
			IEventStateMachine owningStateMachine,
			BaseEventStateMachine masterStateMachine
		)
		{
			_owningStateMachine = owningStateMachine;
			_masterStateMachine = masterStateMachine;
			OnInit();
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

		private IEventStateMachine _owningStateMachine;
		private BaseEventStateMachine _masterStateMachine;

		private readonly List<EventTransition> _transitions = new();
	}

	public sealed class NullEventState : BaseEventState
	{
		protected override void OnInit() { }

		protected override void OnEnter() { }

		protected override void OnDisable() { }

		protected override void OnExit() { }
	}
}