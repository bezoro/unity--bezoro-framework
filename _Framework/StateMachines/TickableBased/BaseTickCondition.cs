using UnityEngine;

namespace Bezoro.StateMachines
{
	public abstract class BaseTickCondition :
		ITickCondition
	{
		public void Init(MonoBehaviour stateMachine, bool inverted = false)
		{
			_stateMachine = stateMachine;
			_inverted = inverted;
			OnInit();
		}

		void ICondition.Init(ITransition transition, MonoBehaviour stateMachine) =>
			Init(stateMachine);

		public MonoBehaviour StateMachine => _stateMachine;

		public bool Inverted => _inverted;

		public void BecomeRelevant() =>
			OnBecomeRelevant();

		public bool Tick() =>
			this.Tick_Ext();

		public void CeaseRelevant() =>
			OnCeaseRelevant();

		#region Abstract Methods

		protected abstract void OnInit();

		protected abstract void OnBecomeRelevant();

		public abstract bool OnTick();

		protected abstract void OnCeaseRelevant();

		#endregion

		private MonoBehaviour _stateMachine;
		private bool _inverted;
	}
}