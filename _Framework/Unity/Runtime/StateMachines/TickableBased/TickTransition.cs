using Bezoro.Framework.Core;

namespace Bezoro.Framework.Unity.StateMachines.TickableBased
{
	public class TickTransition : ITransition
	{
		private BaseTickCondition[] _conditions         = new BaseTickCondition[4];
		private ConditionsCriteria  _conditionsCriteria = ConditionsCriteria.AnyTrue;
		private ITickableState      _fromState;

		private TickableStateMachineOwner _owner;
		private ITickableState            _toState;

		public void Init(
			ITickableState fromState,
			ITickableState toState,
			TickableStateMachineOwner owner
		)
		{
			_fromState = fromState;
			_toState   = toState;
			_owner     = owner;
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
		where T : BaseTickCondition, new()
		{
			var new_condition = new T();
			new_condition.Init(_owner, inverted);
			ArrayHelpers.Add(ref _conditions, new_condition);
		}

		public void WithConditions<T0, T1>(
			ConditionsCriteria conditionsCriteria = ConditionsCriteria.AnyTrue,
			bool inverted0 = false,
			bool inverted1 = false
		)
		where T0 : BaseTickCondition, new()
		where T1 : BaseTickCondition, new()
		{
			var new_condition0 = new T0();
			new_condition0.Init(_owner, inverted0);
			ArrayHelpers.Add(ref _conditions, new_condition0);

			var new_condition1 = new T1();
			new_condition1.Init(_owner, inverted1);
			ArrayHelpers.Add(ref _conditions, new_condition1);

			_conditionsCriteria = conditionsCriteria;
		}

		public void WithConditions<T0, T1, T2>(
			ConditionsCriteria conditionsCriteria = ConditionsCriteria.AnyTrue,
			bool inverted0 = false,
			bool inverted1 = false,
			bool inverted2 = false
		)
		where T0 : BaseTickCondition, new()
		where T1 : BaseTickCondition, new()
		where T2 : BaseTickCondition, new()
		{
			var new_condition0 = new T0();
			new_condition0.Init(_owner, inverted0);
			ArrayHelpers.Add(ref _conditions, new_condition0);

			var new_condition1 = new T1();
			new_condition1.Init(_owner, inverted1);
			ArrayHelpers.Add(ref _conditions, new_condition1);

			var new_condition2 = new T2();
			new_condition2.Init(_owner, inverted2);
			ArrayHelpers.Add(ref _conditions, new_condition2);

			_conditionsCriteria = conditionsCriteria;
		}
	}
}
