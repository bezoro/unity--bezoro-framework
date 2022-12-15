using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bezoro.StateMachines
{
	public abstract class BaseState<TAgent> : BaseState
	where TAgent : MonoBehaviour
	{
		public sealed override void Init
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

	public abstract class BaseState : IState
	{
		public virtual void Init
		(
			IStateMachine owningStateMachine,
			BaseStateMachine masterStateMachine
		)
		{
			_owningStateMachine = owningStateMachine;
			_masterStateMachine = masterStateMachine;
			OnInit();
		}

		public Transition TransitionsTo(IState state)
		{
			var transition = new Transition(this, state, _masterStateMachine);
			_transitions.Add(transition);
			return transition;
		}

		public void Enter()
		{
			OnEnter();

			foreach (var transition in _transitions)
				transition.BecomeRelevant();
		}

		public void Tick()
		{
			OnTick();
			CheckTransitions();
		}

		public void FixedTick() => OnFixedTick();
		public void LateTick() => OnLateTick();

		public void SwitchOwningStateMachineCurrentState(IState toState) =>
			_owningStateMachine.SwitchCurrentState(toState);

		public void Exit()
		{
			foreach (var transition in _transitions)
				transition.CeaseRelevant();

			OnExit();
		}

		protected abstract void OnInit();
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

		private IStateMachine _owningStateMachine;
		private BaseStateMachine _masterStateMachine;

		private readonly List<Transition> _transitions = new();
	}

	public class NullState : BaseState
	{
		protected override void OnInit() { }
		protected override void OnEnter() => Debug.Log($"NullState Entered");
		protected override void OnTick() => Debug.Log($"NullState Ticked");
		protected override void OnFixedTick() => Debug.Log($"NullState FixedTicked");
		protected override void OnLateTick() => Debug.Log($"NullState LateTicked");
		protected override void OnExit() => Debug.Log($"NullState Exited");
	}
}