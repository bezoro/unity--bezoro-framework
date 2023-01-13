using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Bezoro.StateMachines
{
	public abstract class BaseEventState<TAgent> : BaseEventState
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

			foreach (var transition in _transitions)
				transition.BecomeRelevant();
		}

		public void OnTransitionRaised(IEventState toState) =>
			_owningStateMachine.SwitchCurrentState(toState);

		public void Exit()
		{
			foreach (var transition in _transitions)
				transition.CeaseRelevant();

			OnExit();
		}

		protected abstract void OnInit();
		protected abstract void OnEnter();
		protected abstract void OnExit();

		private IEventStateMachine _owningStateMachine;
		private BaseEventStateMachine _masterStateMachine;

		private readonly List<EventTransition> _transitions = new();
	}

	internal sealed class NullEventState : BaseEventState
	{
		protected override void OnInit() { }
		protected override void OnEnter() { }
		protected override void OnExit() { }
	}
}