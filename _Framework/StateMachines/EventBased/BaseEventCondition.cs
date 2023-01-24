using System.Collections;
using UnityEngine;

namespace Bezoro.StateMachines
{
	public abstract class BaseEventCondition
	{
		public void Init
		(EventTransition transition,
		 MonoBehaviour stateMachine)
		{
			_transition = transition;
			_stateMachine = stateMachine;
			OnInit();
		}

		public void BecomeRelevant() =>
			OnBecomeRelevant();

		protected void Raise() =>
			_transition.OnConditionRaised();

		public void CeaseRelevant() =>
			OnCeaseRelevant();

		protected Coroutine StartCoroutine(IEnumerator routine) =>
			_stateMachine.StartCoroutine(routine);

		protected void StopCoroutine(Coroutine routine) =>
			_stateMachine.StopCoroutine(routine);

		protected bool TryGetComponent<T>(out T component)
		where T : Component
		{
			if (_stateMachine.TryGetComponent<T>(out var c))
			{
				component = c;
				return true;
			}

			component = null;
			return false;
		}

		protected abstract void OnInit();

		protected abstract void OnBecomeRelevant();

		protected abstract void OnCeaseRelevant();

		private EventTransition _transition;
		private MonoBehaviour _stateMachine;
	}
}