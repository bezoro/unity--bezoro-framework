using System.Collections.Generic;
using UnityEngine;

namespace Bezoro.StateMachines
{
	public class KCCTransition :
		IKCCTransition
	{
		public KCCTransition
		(
			IKCCState fromState,
			IKCCState toState,
			MonoBehaviour stateMachine
		)
		{
			_fromState = fromState;
			_toState = toState;
			_stateMachine = stateMachine;
		}

		public void WithCondition<TCondition>(bool inverted = false)
		where TCondition : BaseKCCCondition, new()
		{
			var newCondition = new TCondition();
			newCondition.Init(_stateMachine, inverted);
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
			newCondition1.Init(_stateMachine, inverted1);
			_conditions.Add(newCondition1);

			var newCondition2 = new TCondition2();
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

		private readonly IKCCState _fromState;
		private readonly IKCCState _toState;
		private readonly List<BaseKCCCondition> _conditions = new(4);
		private readonly MonoBehaviour _stateMachine;
	}
}