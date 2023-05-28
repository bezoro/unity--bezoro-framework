using UnityEngine;
using Bezoro.Framework.Core.Helpers;

namespace Bezoro.Framework.StateMachines
{
	public class KCCTransition : ITransition
	{
		public void Init(IKCCState fromState, IKCCState toState, MonoBehaviour stateMachine)
		{
			_fromState = fromState;
			_toState = toState;
			_stateMachine = stateMachine;
		}

		public void BecomeRelevant()
		{
			AllConditionsBecomeRelevant();

			void AllConditionsBecomeRelevant()
			{
				for (var i = 0; i < _conditions.Length; i++)
				{
					if (_conditions[i] == null)
						continue;

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
					TickAnyTrue();

				else if (_conditionsCriteria is ConditionsCriteria.AllTrue)
					TickAllTrue();

				void TickAnyTrue()
				{
					for (var i = 0; i < _conditions.Length; i++)
					{
						if (_conditions[i] == null)
							continue;

						if (_conditions[i].Tick())
							_fromState.OnTransitionRaised(_toState);
					}
				}

				void TickAllTrue()
				{
					for (var i = 0; i < _conditions.Length; i++)
					{
						if (_conditions[i] == null)
							continue;

						if (!_conditions[i].Tick())
							return;
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
				for (var i = 0; i < _conditions.Length; i++)
				{
					if (_conditions[i] == null)
						continue;

					_conditions[i].CeaseRelevant();
				}
			}
		}

		public void WithCondition<T>(bool inverted = false)
		where T : BaseKCCCondition, new()
		{
			var newCondition = new T();
			newCondition.Init(_stateMachine, inverted);
			ArrayHelpers.Add(ref _conditions, newCondition);
		}

		public void WithConditions<T0, T1>
		(ConditionsCriteria conditionsCriteria = ConditionsCriteria.AnyTrue,
		 bool inverted0 = false,
		 bool inverted1 = false)
		where T0 : BaseKCCCondition, new()
		where T1 : BaseKCCCondition, new()
		{
			var newCondition0 = new T0();
			newCondition0.Init(_stateMachine, inverted0);
			ArrayHelpers.Add(ref _conditions, newCondition0);

			var newCondition1 = new T1();
			newCondition1.Init(_stateMachine, inverted1);
			ArrayHelpers.Add(ref _conditions, newCondition1);

			_conditionsCriteria = conditionsCriteria;
		}

		public void WithConditions<T0, T1, T2>
		(ConditionsCriteria conditionsCriteria = ConditionsCriteria.AnyTrue,
		 bool inverted0 = false,
		 bool inverted1 = false,
		 bool inverted2 = false)
		where T0 : BaseKCCCondition, new()
		where T1 : BaseKCCCondition, new()
		where T2 : BaseKCCCondition, new()
		{
			var newCondition0 = new T0();
			newCondition0.Init(_stateMachine, inverted0);
			ArrayHelpers.Add(ref _conditions, newCondition0);

			var newCondition1 = new T1();
			newCondition1.Init(_stateMachine, inverted1);
			ArrayHelpers.Add(ref _conditions, newCondition1);

			var newCondition2 = new T2();
			newCondition2.Init(_stateMachine, inverted2);
			ArrayHelpers.Add(ref _conditions, newCondition2);

			_conditionsCriteria = conditionsCriteria;
		}

		private MonoBehaviour _stateMachine;
		private IKCCState _fromState;
		private IKCCState _toState;
		private BaseKCCCondition[] _conditions = new BaseKCCCondition[4];
		private ConditionsCriteria _conditionsCriteria = ConditionsCriteria.AnyTrue;
	}
}