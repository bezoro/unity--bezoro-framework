using System.Collections;
using UnityEngine;

namespace Bezoro.StateMachines
{
	public abstract class BaseKCCCondition
	{
		public void Init(BaseKCCStateMachine stateMachine, bool inverted = false)
		{
			_stateMachine = stateMachine;
			_inverted = inverted;
			OnInit();
		}

		public void BecomeRelevant() =>
			OnBecomeRelevant();

		public bool Tick() =>
			_inverted ? !OnTick() : OnTick();

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

		protected abstract bool OnTick();

		protected abstract void OnCeaseRelevant();

		private bool _inverted;
		private BaseKCCStateMachine _stateMachine;
	}
}