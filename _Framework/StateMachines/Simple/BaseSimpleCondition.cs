using UnityEngine;

namespace Bezoro.StateMachines
{
	public abstract class BaseSimpleCondition<TAgent> : BaseSimpleCondition
	where TAgent : MonoBehaviour
	{
		public override void Init
		(
			SimpleTransition owningTransition,
			BaseSimpleStateMachine masterStateMachine
		)
		{
			Agent = masterStateMachine.GetComponent<TAgent>();
			base.Init(owningTransition, masterStateMachine);
		}

		protected TAgent Agent { get; private set; }
	}

	public abstract class BaseSimpleCondition
	{
		public virtual void Init
		(
			SimpleTransition owningTransition,
			BaseSimpleStateMachine masterStateMachine
		)
		{
			_owningTransition = owningTransition;
			OnInit();
		}

		public void BecomeRelevant() => OnBecomeRelevant();

		protected void Raise() => _owningTransition.NotifyStateOfConditionRaise();

		public void CeaseRelevant() => OnCeaseRelevant();

		protected abstract void OnInit();
		protected abstract void OnBecomeRelevant();
		protected abstract void OnCeaseRelevant();

		private SimpleTransition _owningTransition;
		private BaseSimpleStateMachine _masterStateMachine;
	}
}