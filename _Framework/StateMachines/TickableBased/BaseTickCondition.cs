using UnityEngine;

namespace Bezoro.StateMachines
{
	public abstract class BaseTickCondition
	{
		public void Init(BaseTickableStateMachine stateMachine, bool inverted = false)
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
		private BaseTickableStateMachine _stateMachine;
	}
}