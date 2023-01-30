using UnityEngine;
using System.Collections;

namespace Bezoro.StateMachines
{
	public abstract class BaseEventCondition
	{
		public void Init(EventTransition transition, MonoBehaviour stateMachine)
		{
			_transition = transition;
			_stateMachine = stateMachine;
			OnInit();
		}

		public MonoBehaviour StateMachine => _stateMachine;

		public void BecomeRelevant() =>
			OnBecomeRelevant();

		protected void Raise() =>
			_transition.OnConditionRaised();

		public void CeaseRelevant() =>
			OnCeaseRelevant();

		#region Abstract Methods

		protected abstract void OnInit();

		protected abstract void OnBecomeRelevant();

		protected abstract void OnCeaseRelevant();

		#endregion

		protected Coroutine StartCoroutine(IEnumerator routine) =>
			_stateMachine.StartCoroutine(routine);

		protected void StopCoroutine(Coroutine routine) =>
			_stateMachine.StopCoroutine(routine);

		private EventTransition _transition;
		private MonoBehaviour _stateMachine;
	}
}