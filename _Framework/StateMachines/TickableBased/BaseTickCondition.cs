using UnityEngine;

namespace Bezoro.StateMachines
{
	public abstract class BaseTickCondition
	{
		public void Init(MonoBehaviour stateMachine, bool inverted = false)
		{
			_stateMachine = stateMachine;
			_inverted = inverted;
			OnInit();
		}

		public MonoBehaviour StateMachine => _stateMachine;

		public bool Inverted => _inverted;

		public void BecomeRelevant() =>
			OnBecomeRelevant();

		public bool Tick() =>
			_inverted ? !OnTick() : OnTick();

		public void CeaseRelevant() =>
			OnCeaseRelevant();

		#region Abstract Methods

		protected abstract void OnInit();

		protected abstract void OnBecomeRelevant();

		protected abstract bool OnTick();

		protected abstract void OnCeaseRelevant();

		#endregion

		private MonoBehaviour _stateMachine;
		private bool _inverted;
	}
}