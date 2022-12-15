using System;
using System.Collections.Generic;

namespace Bezoro.StateMachines
{
	public class Transition
	{
		public Transition
		(
			IState fromState,
			IState toState,
			BaseStateMachine masterStateMachine
		)
		{
			_fromState = fromState;
			_toState = toState;
			_masterStateMachine = masterStateMachine;
		}

		public void WithCondition<TCondition>(bool inverted = false)
		where TCondition : BaseCondition, new()
		{
			var newCondition = new TCondition();
			newCondition.Init(_masterStateMachine, inverted);
			_conditions.Add(newCondition);
		}

		public void WithConditions<TCondition1, TCondition2>
		(
			ConditionsCriteria conditionsCriteria,
			bool inverted1 = false,
			bool inverted2 = false
		)
		where TCondition1 : BaseCondition, new()
		where TCondition2 : BaseCondition, new()
		{
			var newCondition1 = new TCondition1();
			newCondition1.Init(_masterStateMachine, inverted1);
			_conditions.Add(newCondition1);

			var newCondition2 = new TCondition2();
			newCondition2.Init(_masterStateMachine, inverted2);
			_conditions.Add(newCondition2);

			_conditionsCriteria = conditionsCriteria;
		}

		public void BecomeRelevant()
		{
			for (var i = 0; i < _conditions.Count; i++)
			{
				var condition = _conditions[i];
				condition.BecomeRelevant();
			}
		}

		public bool CheckConditions()
		{
			switch (_conditionsCriteria)
			{
				case ConditionsCriteria.None:
					break;

				case ConditionsCriteria.AnyTrue:
					for (var i = 0; i < _conditions.Count; i++)
						if (_conditions[i].Check())
						{
							_fromState.SwitchOwningStateMachineCurrentState(_toState);
							return true;
						}

					return false;

				case ConditionsCriteria.AllTrue:
					for (var i = 0; i < _conditions.Count; i++)
						if (!_conditions[i].Check())
							return false;

					_fromState.SwitchOwningStateMachineCurrentState(_toState);
					return true;
				default:
					throw new ArgumentOutOfRangeException();
			}

			return false;
		}

		public void CeaseRelevant()
		{
			for (var i = 0; i < _conditions.Count; i++)
				_conditions[i].CeaseRelevant();
		}

		private ConditionsCriteria _conditionsCriteria = ConditionsCriteria.AnyTrue;

		private readonly IState _fromState;
		private readonly IState _toState;
		private readonly List<BaseCondition> _conditions = new();
		private readonly BaseStateMachine _masterStateMachine;
	}

	public enum ConditionsCriteria
	{
		None = -1,
		AnyTrue,
		AllTrue
	}
}