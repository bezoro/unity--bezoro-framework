using UnityEngine;

namespace Bezoro.StateMachines
{
	public abstract class BaseEventCondition :
		IEventCondition
	{
		//test
		public void Init(IEventTransition transition, MonoBehaviour stateMachine)
		{
			_transition = transition;
			_stateMachine = stateMachine;
			OnInit();
		}

		void ICondition.Init(ITransition transition, MonoBehaviour stateMachine) =>
			Init(transition as IEventTransition, stateMachine);

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

		private IEventTransition _transition;
		private MonoBehaviour _stateMachine;
	}
}