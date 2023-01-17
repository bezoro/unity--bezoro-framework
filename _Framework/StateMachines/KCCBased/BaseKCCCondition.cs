using UnityEngine;
using UnityEngine.Assertions;
using KinematicCharacterController;

namespace Bezoro.StateMachines
{
	public abstract class BaseKCCCondition<TAgent, TBlackBoard> :
		BaseKCCCondition<TAgent>
	where TAgent : MonoBehaviour
	where TBlackBoard : BlackBoard
	{
		public override void Init
		(
			bool inverted,
			BaseKCCStateMachine masterStateMachine,
			KinematicCharacterMotor motor,
			KCCTransition transition
		)
		{
			BlackBoard = masterStateMachine.TryGetBlackBoard<TBlackBoard>();
			Assert.IsNotNull(BlackBoard);

			base.Init(inverted, masterStateMachine, motor, transition);
		}

		protected TBlackBoard BlackBoard { get; private set; }
	}

	public abstract class BaseKCCCondition<TAgent> : BaseKCCCondition
	where TAgent : MonoBehaviour
	{
		public override void Init
		(
			bool inverted,
			BaseKCCStateMachine masterStateMachine,
			KinematicCharacterMotor motor,
			KCCTransition transition
		)
		{
			Agent = masterStateMachine.gameObject.GetComponent<TAgent>();
			base.Init(inverted, masterStateMachine, motor, transition);
		}

		protected TAgent Agent { get; private set; }
	}

	public abstract class BaseKCCCondition
	{
		public bool Inverted { get; private set; }

		protected KinematicCharacterMotor Motor { get; private set; }

		public virtual void Init
		(
			bool inverted,
			BaseKCCStateMachine masterStateMachine,
			KinematicCharacterMotor motor,
			KCCTransition transition
		)
		{
			Inverted = inverted;
			Motor = motor;
			_transition = transition;
			OnInit();
		}

		public void BecomeRelevant() =>
			OnBecomeRelevant();

		public bool Tick() => 
			Inverted ? !OnTick() : OnTick();
		

		public void CeaseRelevant() =>
			OnCeaseRelevant();

		protected abstract void OnInit();

		protected abstract void OnBecomeRelevant();

		protected abstract bool OnTick();

		protected abstract void OnCeaseRelevant();

		private KCCTransition _transition;

		private bool _state;
	}
}