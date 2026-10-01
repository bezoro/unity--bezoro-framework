using System.Collections.Generic;
using UnityEngine;

namespace Bezoro.Unity.Core.StateMachines.KCCBased;

/// <summary>
///     Links KCC states with a condition and destination state.
/// </summary>
public class KCCTransition : ITransition
{
	private readonly List<BaseKCCCondition> _conditions = new(
		4
	);
	private ConditionsCriteria _conditionsCriteria = ConditionsCriteria.AnyTrue;
	private IKCCState          _fromState = null!;
	private IKCCState          _toState = null!;
	private MonoBehaviour _stateMachine = null!;
	public void Init(IKCCState fromState, IKCCState toState, MonoBehaviour stateMachine)
	{
		_fromState    = fromState;
		_toState      = toState;
		_stateMachine = stateMachine;
	}

	public void BecomeRelevant()
	{
		AllConditionsBecomeRelevant();

		void AllConditionsBecomeRelevant()
		{
			for (var i = 0; i < _conditions.Count; i++)
			{
				if (_conditions[i] == null) continue;

				_conditions[i].BecomeRelevant();
			}
		}
	}

	public void CeaseRelevant()
	{
		AllConditionsCeaseRelevant();

		void AllConditionsCeaseRelevant()
		{
			for (var i = 0; i < _conditions.Count; i++)
			{
				if (_conditions[i] == null) continue;

				_conditions[i].CeaseRelevant();
			}
		}
	}

	public void Tick()
	{
		TickConditions();

		void TickConditions()
		{
			if (_conditionsCriteria is ConditionsCriteria.AnyTrue)
				TickAnyTrue();

			else if (_conditionsCriteria is ConditionsCriteria.AllTrue) TickAllTrue();

			void TickAnyTrue()
			{
				for (var i = 0; i < _conditions.Count; i++)
				{
					if (_conditions[i] == null) continue;

					if (_conditions[i].Tick())
						_fromState.OnTransitionRaised(
							_toState
						);
				}
			}

			void TickAllTrue()
			{
				for (var i = 0; i < _conditions.Count; i++)
				{
					if (_conditions[i] == null) continue;

					if (!_conditions[i].Tick()) return;
				}

				_fromState.OnTransitionRaised(
					_toState
				);
			}
		}
	}

	public void WithCondition<T>(bool inverted = false)
		where T : BaseKCCCondition, new()
	{
		var newCondition = new T();
		newCondition.Init(
			_stateMachine,
			inverted
		);

		_conditions.Add(
			newCondition
		);
	}

	public void WithConditions<T0, T1>(
		ConditionsCriteria conditionsCriteria = ConditionsCriteria.AnyTrue,
		bool               inverted0          = false,
		bool               inverted1          = false
	)
		where T0 : BaseKCCCondition, new()
		where T1 : BaseKCCCondition, new()
	{
		var newCondition0 = new T0();
		newCondition0.Init(
			_stateMachine,
			inverted0
		);

		_conditions.Add(
			newCondition0
		);

		var newCondition1 = new T1();
		newCondition1.Init(
			_stateMachine,
			inverted1
		);

		_conditions.Add(
			newCondition1
		);

		_conditionsCriteria = conditionsCriteria;
	}

	public void WithConditions<T0, T1, T2>(
		ConditionsCriteria conditionsCriteria = ConditionsCriteria.AnyTrue,
		bool               inverted0          = false,
		bool               inverted1          = false,
		bool               inverted2          = false
	)
		where T0 : BaseKCCCondition, new()
		where T1 : BaseKCCCondition, new()
		where T2 : BaseKCCCondition, new()
	{
		var newCondition0 = new T0();
		newCondition0.Init(
			_stateMachine,
			inverted0
		);

		_conditions.Add(
			newCondition0
		);

		var newCondition1 = new T1();
		newCondition1.Init(
			_stateMachine,
			inverted1
		);

		_conditions.Add(
			newCondition1
		);

		var newCondition2 = new T2();
		newCondition2.Init(
			_stateMachine,
			inverted2
		);

		_conditions.Add(
			newCondition2
		);

		_conditionsCriteria = conditionsCriteria;
	}
}
