using System;
using System.Collections.Generic;

namespace Bezoro.StateMachines
{
	public class TickTransition
	{
		public TickTransition
		(
			ITickableState fromState,
			ITickableState toState,
			BaseTickableStateMachine stateMachine
		)
		{
			_fromState = fromState;
			_toState = toState;
			_stateMachine = stateMachine;
		}

		public void WithCondition<T>(bool inverted = false)
		where T : BaseTickCondition, new()
		{
			var newCondition = new T();
			newCondition.Init(_stateMachine, inverted);
			_conditions.Add(newCondition);
		}

		public void WithConditions<T0, T1>
		(
			ConditionsCriteria conditionsCriteria,
			bool inverted0 = false,
			bool inverted1 = false
		)
		where T0 : BaseTickCondition, new()
		where T1 : BaseTickCondition, new()
		{
			var newCondition0 = new T0();
			newCondition0.Init(_stateMachine, inverted0);
			_conditions.Add(newCondition0);

			var newCondition1 = new T1();
			newCondition1.Init(_stateMachine, inverted1);
			_conditions.Add(newCondition1);

			_conditionsCriteria = conditionsCriteria;
		}

		public void WithConditions<T0, T1, T2>
		(
			ConditionsCriteria conditionsCriteria,
			bool inverted0 = false,
			bool inverted1 = false,
			bool inverted2 = false
		)
		where T0 : BaseTickCondition, new()
		where T1 : BaseTickCondition, new()
		where T2 : BaseTickCondition, new()
		{
			var newCondition0 = new T0();
			newCondition0.Init(_stateMachine, inverted0);
			_conditions.Add(newCondition0);

			var newCondition1 = new T1();
			newCondition1.Init(_stateMachine, inverted1);
			_conditions.Add(newCondition1);

			var newCondition2 = new T2();
			newCondition2.Init(_stateMachine, inverted2);
			_conditions.Add(newCondition2);

			_conditionsCriteria = conditionsCriteria;
		}

		public void BecomeRelevant()
		{
			for (var i = 0; i < _conditions.Count; i++)
				_conditions[i].BecomeRelevant();
		}

		public void Tick() =>
			TickConditions();

		private void TickConditions()
		{
			if (_conditionsCriteria is ConditionsCriteria.AnyTrue)
				TickAnyTrue();

			else if (_conditionsCriteria is ConditionsCriteria.AllTrue)
				TickAllTrue();

			void TickAnyTrue()
			{
				for (var i = 0; i < _conditions.Count; i++)
					if (_conditions[i].Tick())
						_fromState.OnTransitionTrue(_toState);
			}

			void TickAllTrue()
			{
				for (var i = 0; i < _conditions.Count; i++)
					if (!_conditions[i].Tick())
						return;

				_fromState.OnTransitionTrue(_toState);
			}
		}

		public void CeaseRelevant()
		{
			for (var i = 0; i < _conditions.Count; i++)
				_conditions[i].CeaseRelevant();
		}

		private ConditionsCriteria _conditionsCriteria = ConditionsCriteria.AnyTrue;

		private readonly ITickableState _fromState;
		private readonly ITickableState _toState;
		private readonly List<BaseTickCondition> _conditions = new(4);
		private readonly BaseTickableStateMachine _stateMachine;
	}

	public enum ConditionsCriteria
	{
		None = -1,
		AnyTrue,
		AllTrue
	}
}