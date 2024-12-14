using Bezoro.Framework.Core;
using UnityEngine;

namespace Bezoro.Framework.Unity.StateMachines.KCCBased
{
	public class KCCTransition : ITransition
	{
		private BaseKCCCondition[] _conditions         = new BaseKCCCondition[4];
		private ConditionsCriteria _conditionsCriteria = ConditionsCriteria.AnyTrue;
		private IKCCState          _fromState;

		private MonoBehaviour _stateMachine;
		private IKCCState     _toState;

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
				for (var i = 0 ; i < _conditions.Length ; i++)
				{
					if (_conditions[i] == null)
					{
						continue;
					}

					_conditions[i].BecomeRelevant();
				}
			}
		}

		public void Tick()
		{
			TickConditions();

			void TickConditions()
			{
				if (_conditionsCriteria is ConditionsCriteria.AnyTrue)
				{
					TickAnyTrue();
				}

				else if (_conditionsCriteria is ConditionsCriteria.AllTrue)
				{
					TickAllTrue();
				}

				void TickAnyTrue()
				{
					for (var i = 0 ; i < _conditions.Length ; i++)
					{
						if (_conditions[i] == null)
						{
							continue;
						}

						if (_conditions[i].Tick())
						{
							_fromState.OnTransitionRaised(_toState);
						}
					}
				}

				void TickAllTrue()
				{
					for (var i = 0 ; i < _conditions.Length ; i++)
					{
						if (_conditions[i] == null)
						{
							continue;
						}

						if (!_conditions[i].Tick())
						{
							return;
						}
					}

					_fromState.OnTransitionRaised(_toState);
				}
			}
		}

		public void CeaseRelevant()
		{
			AllConditionsCeaseRelevant();

			void AllConditionsCeaseRelevant()
			{
				for (var i = 0 ; i < _conditions.Length ; i++)
				{
					if (_conditions[i] == null)
					{
						continue;
					}

					_conditions[i].CeaseRelevant();
				}
			}
		}

		public void WithCondition<T>(bool inverted = false)
		where T : BaseKCCCondition, new()
		{
			var new_condition = new T();
			new_condition.Init(_stateMachine, inverted);
			Array_Helpers.Add(ref _conditions, new_condition);
		}

		public void WithConditions<T0, T1>(
			ConditionsCriteria conditionsCriteria = ConditionsCriteria.AnyTrue,
			bool inverted0 = false,
			bool inverted1 = false
		)
		where T0 : BaseKCCCondition, new()
		where T1 : BaseKCCCondition, new()
		{
			var new_condition0 = new T0();
			new_condition0.Init(_stateMachine, inverted0);
			Array_Helpers.Add(ref _conditions, new_condition0);

			var new_condition1 = new T1();
			new_condition1.Init(_stateMachine, inverted1);
			Array_Helpers.Add(ref _conditions, new_condition1);

			_conditionsCriteria = conditionsCriteria;
		}

		public void WithConditions<T0, T1, T2>(
			ConditionsCriteria conditionsCriteria = ConditionsCriteria.AnyTrue,
			bool inverted0 = false,
			bool inverted1 = false,
			bool inverted2 = false
		)
		where T0 : BaseKCCCondition, new()
		where T1 : BaseKCCCondition, new()
		where T2 : BaseKCCCondition, new()
		{
			var new_condition0 = new T0();
			new_condition0.Init(_stateMachine, inverted0);
			Array_Helpers.Add(ref _conditions, new_condition0);

			var new_condition1 = new T1();
			new_condition1.Init(_stateMachine, inverted1);
			Array_Helpers.Add(ref _conditions, new_condition1);

			var new_condition2 = new T2();
			new_condition2.Init(_stateMachine, inverted2);
			Array_Helpers.Add(ref _conditions, new_condition2);

			_conditionsCriteria = conditionsCriteria;
		}
	}
}
