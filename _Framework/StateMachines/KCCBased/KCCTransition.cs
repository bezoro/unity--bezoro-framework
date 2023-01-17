using System.Collections.Generic;

namespace Bezoro.StateMachines
{
	public class KCCTransition
	{
		public IKCCState FromState { get; }

		public IKCCState ToState { get; }

		public ConditionsCriteria ConditionsCriteria { get; private set; } =
			ConditionsCriteria.AnyTrue;

		public BaseKCCStateMachine StateMachine { get; }

		public KCCTransition
		(
			IKCCState fromState,
			IKCCState toState,
			BaseKCCStateMachine stateMachine
		)
		{
			FromState = fromState;
			ToState = toState;
			StateMachine = stateMachine;
		}

		public void WithCondition<TCondition>(bool inverted = false)
		where TCondition : BaseKCCCondition, new()
		{
			var newCondition = new TCondition();
			newCondition.Init(inverted, StateMachine, StateMachine.Motor);
			_conditions.Add(newCondition);
		}

		public void WithConditions<TCondition1, TCondition2>
		(
			ConditionsCriteria conditionsCriteria = ConditionsCriteria.AnyTrue,
			bool inverted1 = false,
			bool inverted2 = false
		)
		where TCondition1 : BaseKCCCondition, new()
		where TCondition2 : BaseKCCCondition, new()
		{
			var newCondition1 = new TCondition1();
			newCondition1.Init(inverted1, StateMachine, StateMachine.Motor);
			_conditions.Add(newCondition1);

			var newCondition2 = new TCondition2();
			newCondition2.Init(inverted2, StateMachine, StateMachine.Motor);
			_conditions.Add(newCondition2);

			ConditionsCriteria = conditionsCriteria;
		}

		public void BecomeRelevant()
		{
			foreach (var condition in _conditions)
				condition.BecomeRelevant();
		}

		public bool CheckConditions()
		{
			switch (ConditionsCriteria)
			{
				case ConditionsCriteria.None:
					break;

				case ConditionsCriteria.AnyTrue:
					foreach (var condition in _conditions)
						if (condition.Check())
							return true;

					return false;

				case ConditionsCriteria.AllTrue:
					foreach (var condition in _conditions)
						if (!condition.Check())
							return false;

					return true;
			}

			return false;
		}

		public void CeaseRelevant()
		{
			foreach (var condition in _conditions)
				condition.CeaseRelevant();
		}

		private readonly List<BaseKCCCondition> _conditions = new(4);
	}
}