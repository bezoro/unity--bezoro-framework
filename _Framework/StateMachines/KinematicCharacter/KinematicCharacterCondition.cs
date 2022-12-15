using KinematicCharacterController;
using UnityEngine;
using UnityEngine.Assertions;

namespace Bezoro.StateMachines
{
	public abstract class KinematicCharacterCondition<TAgent, TBlackBoard> :
		KinematicCharacterCondition<TAgent>
	where TAgent : MonoBehaviour
	where TBlackBoard : BlackBoard
	{
		public override void Init
		(
			KinematicCharacterStateMachine masterStateMachine,
			bool inverted,
			KinematicCharacterMotor motor
		)
		{
			BlackBoard = masterStateMachine.BlackBoard as TBlackBoard;
			Assert.IsNotNull(BlackBoard);

			base.Init(masterStateMachine, inverted, motor);
		}

		protected TBlackBoard BlackBoard { get; private set; }
	}

	public abstract class KinematicCharacterCondition<TAgent> : KinematicCharacterCondition
	where TAgent : MonoBehaviour
	{
		public override void Init
		(
			KinematicCharacterStateMachine masterStateMachine,
			bool inverted,
			KinematicCharacterMotor motor
		)
		{
			Agent = masterStateMachine.Owner.GetComponent<TAgent>();
			base.Init
				(
				 masterStateMachine,
				 inverted,
				 motor
				);
		}

		protected TAgent Agent { get; private set; }
	}

	public abstract class KinematicCharacterCondition
	{
		protected KinematicCharacterStateMachine MasterStateMachine { get; private set; }

		protected KinematicCharacterMotor Motor { get; private set; }

		public bool Inverted { get; private set; }

		public virtual void Init
		(
			KinematicCharacterStateMachine masterStateMachine,
			bool inverted,
			KinematicCharacterMotor motor
		)
		{
			MasterStateMachine = masterStateMachine;
			Inverted = inverted;
			Motor = motor;
			OnInit();
		}


		public void BecomeRelevant() =>
			OnBecomeRelevant();

		public bool Check() =>
			Inverted ? !OnCheck() : OnCheck();

		public void CeaseRelevant() =>
			OnCeaseRelevant();

		protected abstract void OnInit();

		protected abstract void OnBecomeRelevant();

		protected abstract bool OnCheck();

		protected abstract void OnCeaseRelevant();
	}
}