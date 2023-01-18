using UnityEngine;
using UnityEngine.Assertions;

namespace Bezoro.StateMachines
{
	public abstract class BaseKCCCondition<TAgent, TBlackBoard> : BaseKCCCondition<TAgent>
	where TAgent : MonoBehaviour
	where TBlackBoard : BlackBoard
	{
		public override void Init(bool inverted, BaseKCCStateMachine masterStateMachine)
		{
			BlackBoard = masterStateMachine.TryGetBlackBoard<TBlackBoard>();
			Assert.IsNotNull(BlackBoard);

			base.Init(inverted, masterStateMachine);
		}

		protected TBlackBoard BlackBoard { get; private set; }
	}

	public abstract class BaseKCCCondition<TAgent> : BaseKCCCondition
	where TAgent : MonoBehaviour
	{
		public override void Init(bool inverted, BaseKCCStateMachine masterStateMachine)
		{
			Agent = masterStateMachine.gameObject.GetComponent<TAgent>();
			base.Init(inverted, masterStateMachine);
		}

		protected TAgent Agent { get; private set; }
	}

	public abstract class BaseKCCCondition
	{
		public bool Inverted => _inverted;

		public virtual void Init(bool inverted, BaseKCCStateMachine masterStateMachine)
		{
			_inverted = inverted;
			OnInit();
		}

		public void BecomeRelevant() =>
			OnBecomeRelevant();

		public bool Tick() =>
			_inverted ? !OnTick() : OnTick();


		public void CeaseRelevant() =>
			OnCeaseRelevant();

		protected abstract void OnInit();

		protected abstract void OnBecomeRelevant();

		protected abstract bool OnTick();

		protected abstract void OnCeaseRelevant();

		private bool _inverted;
	}
}