using UnityEngine;

namespace Bezoro.StateMachines
{
	public abstract class BaseEventCondition<TAgent> : BaseEventCondition
	where TAgent : MonoBehaviour
	{
		public override void Init
		(
			EventTransition owningTransition,
			BaseEventStateMachine masterStateMachine
		)
		{
			Agent = masterStateMachine.GetComponent<TAgent>();
			base.Init(owningTransition, masterStateMachine);
		}

		protected TAgent Agent { get; private set; }
	}

	public abstract class BaseEventCondition
	{
		public virtual void Init
		(
			EventTransition owningTransition,
			BaseEventStateMachine masterStateMachine
		)
		{
			_owningTransition = owningTransition;
			OnInit();
		}

		public void BecomeRelevant() =>
			OnBecomeRelevant();

		protected void Raise() =>
			_owningTransition.OnConditionRaised();

		public void CeaseRelevant() =>
			OnCeaseRelevant();

		protected abstract void OnInit();

		protected abstract void OnBecomeRelevant();

		protected abstract void OnCeaseRelevant();

		private EventTransition _owningTransition;
		private BaseEventStateMachine _masterStateMachine;
	}
}