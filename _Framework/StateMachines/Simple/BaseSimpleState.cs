using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bezoro.StateMachines
{
	public abstract class BaseSimpleState<TAgent> : BaseSimpleState
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

	public abstract class BaseSimpleState : ISimpleState
	{
		public virtual void Init
		(
			ISimpleStateMachine owningStateMachine,
			BaseSimpleStateMachine masterStateMachine
		)
		{
			_owningStateMachine = owningStateMachine;
			_masterStateMachine = masterStateMachine;
			OnInit();
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
		}

		public void SwitchOwningStateMachineCurrentState(ISimpleState toState) =>
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

		private ISimpleStateMachine _owningStateMachine;
		private BaseSimpleStateMachine _masterStateMachine;

		private readonly List<SimpleTransition> _transitions = new();
	}

	internal sealed class NullSimpleState : BaseSimpleState
	{
		protected override void OnInit() { }
		protected override void OnEnter() { }
		protected override void OnExit() { }
	}
}