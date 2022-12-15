using System.Collections.Generic;

namespace Bezoro.StateMachines
{
	public class KinematicCharacterTransition
	{
		public IKinematicCharacterState FromState { get; }

		public IKinematicCharacterState ToState { get; }

		public List<KinematicCharacterCondition> Conditions { get; } = new(4);

		public ConditionsCriteria ConditionsCriteria { get; private set; } =
			ConditionsCriteria.AnyTrue;

		public KinematicCharacterStateMachine StateMachine { get; }

		public KinematicCharacterTransition
		(
			IKinematicCharacterState fromState,
			IKinematicCharacterState toState,
			KinematicCharacterStateMachine stateMachine
		)
		{
			FromState = fromState;
			ToState = toState;
			StateMachine = stateMachine;
		}

		public void WithCondition<TCondition>(bool inverted = false)
		where TCondition : KinematicCharacterCondition, new()
		{
			var newCondition = new TCondition();
			newCondition.Init(StateMachine, inverted, StateMachine.Motor);
			Conditions.Add(newCondition);
		}

		public void WithConditions<TCondition1, TCondition2>
		(
			ConditionsCriteria conditionsCriteria = ConditionsCriteria.AnyTrue,
			bool inverted1 = false,
			bool inverted2 = false
		)
		where TCondition1 : KinematicCharacterCondition, new()
		where TCondition2 : KinematicCharacterCondition, new()
		{
			var newCondition1 = new TCondition1();
			newCondition1.Init(StateMachine, inverted1, StateMachine.Motor);
			Conditions.Add(newCondition1);

			var newCondition2 = new TCondition2();
			newCondition2.Init(StateMachine, inverted2, StateMachine.Motor);
			Conditions.Add(newCondition2);

			ConditionsCriteria = conditionsCriteria;
		}

		public void BecomeRelevant()
		{
			foreach (var condition in Conditions)
				condition.BecomeRelevant();
		}

		public bool CheckConditions()
		{
			switch (ConditionsCriteria)
			{
				case ConditionsCriteria.None:
					break;

				case ConditionsCriteria.AnyTrue:
					foreach (var condition in Conditions)
						if (condition.Check())
							return true;

					return false;

				case ConditionsCriteria.AllTrue:
					foreach (var condition in Conditions)
						if (!condition.Check())
							return false;

					return true;
			}

			return false;
		}

		public void CeaseRelevant()
		{
			foreach (var condition in Conditions)
				condition.CeaseRelevant();
		}
	}
}