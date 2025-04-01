using Bezoro.Framework.Core;

namespace Bezoro.Framework.Unity.StateMachines.EventBased
{
	public sealed class EventTransition : ITransition
	{
		private BaseEventCondition[] _conditions = new BaseEventCondition[4];
		private IEventState          _fromState;

		private EventStateMachineOwner _owner;
		private IEventState            _toState;

		public void Init(IEventState fromState, IEventState toState, EventStateMachineOwner owner)
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

		public void OnConditionRaised()
		{
			_fromState.OnTransitionRaised(_toState);
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

		public void WithCondition<T>()
		where T : BaseEventCondition, new()
		{
			var new_condition = new T();
			new_condition.Init(_owner, this);
			ArrayHelpers.Add(ref _conditions, new_condition);
		}

		public void WithConditions<T0, T1>()
		where T0 : BaseEventCondition, new()
		where T1 : BaseEventCondition, new()
		{
			var condition_1 = new T0();
			condition_1.Init(_owner, this);
			ArrayHelpers.Add(ref _conditions, condition_1);

			var condition_2 = new T1();
			condition_2.Init(_owner, this);
			ArrayHelpers.Add(ref _conditions, condition_2);
		}

		public void WithConditions<T0, T1, T2>()
		where T0 : BaseEventCondition, new()
		where T1 : BaseEventCondition, new()
		where T2 : BaseEventCondition, new()
		{
			var condition_1 = new T0();
			condition_1.Init(_owner, this);
			ArrayHelpers.Add(ref _conditions, condition_1);

			var condition_2 = new T1();
			condition_2.Init(_owner, this);
			ArrayHelpers.Add(ref _conditions, condition_2);

			var condition_3 = new T2();
			condition_3.Init(_owner, this);
			ArrayHelpers.Add(ref _conditions, condition_3);
		}
	}
}
