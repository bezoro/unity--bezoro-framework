using UnityEngine;

namespace Bezoro.StateMachines
{
	public abstract class BaseCondition<TAgent> : BaseCondition
	where TAgent : MonoBehaviour
	{
		public override void Init(BaseStateMachine masterStateMachine, bool inverted = false)
		{
			Agent = masterStateMachine.GetComponent<TAgent>();
			base.Init(masterStateMachine, inverted);
		}

		protected TAgent Agent { get; private set; }
	}

	public abstract class BaseCondition
	{
		public virtual void Init(BaseStateMachine masterStateMachine, bool inverted = false)
		{
			_inverted = inverted;
			OnInit();
		}

		public void BecomeRelevant() => OnBecomeRelevant();

		public bool Check() => _inverted ? !OnCheck() : OnCheck();

		public void CeaseRelevant() => OnCeaseRelevant();

		protected abstract void OnInit();
		protected abstract void OnBecomeRelevant();
		protected abstract bool OnCheck();
		protected abstract void OnCeaseRelevant();

		private bool _inverted;
	}
}