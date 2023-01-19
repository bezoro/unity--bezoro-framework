using System.Collections;
using UnityEngine;
using System.Collections.Generic;

namespace Bezoro.StateMachines
{
	public abstract class BaseTickableState : ITickableState
	{
		public virtual void Init
		(
			ITickableStateMachine owningStateMachine,
			BaseTickableStateMachine masterStateMachine
		)
		{
			_owningStateMachine = owningStateMachine;
			_masterStateMachine = masterStateMachine;
			OnInit();
		}

		public TickTransition TransitionsTo(ITickableState state)
		{
			var transition = new TickTransition(this, state, _masterStateMachine);
			_transitions.Add(transition);
			return transition;
		}

		public void Enter()
		{
			OnEnter();

			for (var i = 0; i < _transitions.Count; i++)
				_transitions[i].BecomeRelevant();
		}

		public void EarlyTick() =>
			OnEarlyTick();

		public void Tick() =>
			OnTick();

		public void FixedTick() =>
			OnFixedTick();

		public void LateTick()
		{
			OnLateTick();
			TickTransitions();
		}

		public void Disable() =>
			OnDisable();

		public void OnTransitionTrue(ITickableState toState) =>
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

		#region Protected Abstract Methods

		protected abstract void OnInit();

		protected abstract void OnEnter();

		protected abstract void OnEarlyTick();

		protected abstract void OnTick();

		protected abstract void OnFixedTick();

		protected abstract void OnLateTick();

		protected abstract void OnDisable();

		protected abstract void OnExit();

		#endregion

		private void TickTransitions()
		{
			for (var i = 0; i < _transitions.Count; i++)
				_transitions[i].Tick();
		}

		private ITickableStateMachine _owningStateMachine;
		private BaseTickableStateMachine _masterStateMachine;

		private readonly List<TickTransition> _transitions = new();
	}

	public class NullTickableState : BaseTickableState
	{
		protected override void OnInit() { }

		protected override void OnEnter() { }

		protected override void OnEarlyTick() { }

		protected override void OnTick() { }

		protected override void OnFixedTick() { }

		protected override void OnLateTick() { }

		protected override void OnDisable() { }

		protected override void OnExit() { }
	}
}