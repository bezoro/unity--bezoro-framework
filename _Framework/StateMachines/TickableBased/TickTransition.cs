using UnityEngine;
using Bezoro.Core.Helpers;

namespace Bezoro.StateMachines
{
	public class TickTransition :
		ITickTransition
	{
		public void Init
		(ITickableState fromState,
		 ITickableState toState,
		 MonoBehaviour stateMachine)
		{
			_fromState = fromState;
			_toState = toState;
			_stateMachine = stateMachine;
		}

		void ITransition.Init(IState fromState, IState toState, MonoBehaviour stateMachine) =>
			Init(fromState as ITickableState,
				 toState as ITickableState,
				 stateMachine);

		public ITickableState FromState => _fromState;

		public ITickableState ToState => _toState;

		public ITickCondition[] Conditions => _conditions;

		ICondition[] ITransition.Conditions => _conditions;

		public ConditionsCriteria ConditionsCriteria => _conditionsCriteria;

		public MonoBehaviour StateMachine => _stateMachine;

		public void BecomeRelevant() =>
			this.AllConditionsBecomeRelevant();

		public void Tick() =>
			this.TickConditions();


		public void CeaseRelevant() =>
			this.AllConditionsCeaseRelevant();

		public void WithCondition<T>(bool inverted = false)
		where T : class, ITickCondition, new() =>
			ArrayHelpers.Add(ref _conditions, this.CreateCondition<T>(inverted));

		public void WithConditions<T0, T1>
		(ConditionsCriteria conditionsCriteria,
		 bool inverted0 = false,
		 bool inverted1 = false)
		where T0 : class, ITickCondition, new()
		where T1 : class, ITickCondition, new()
		{
			ArrayHelpers.Add(ref _conditions, this.CreateCondition<T0>(inverted0));
			ArrayHelpers.Add(ref _conditions, this.CreateCondition<T1>(inverted1));
			_conditionsCriteria = conditionsCriteria;
		}

		public void WithConditions<T0, T1, T2>
		(ConditionsCriteria conditionsCriteria,
		 bool inverted0 = false,
		 bool inverted1 = false,
		 bool inverted2 = false)
		where T0 : class, ITickCondition, new()
		where T1 : class, ITickCondition, new()
		where T2 : class, ITickCondition, new()
		{
			ArrayHelpers.Add(ref _conditions, this.CreateCondition<T0>(inverted0));
			ArrayHelpers.Add(ref _conditions, this.CreateCondition<T1>(inverted1));
			ArrayHelpers.Add(ref _conditions, this.CreateCondition<T2>(inverted2));
			_conditionsCriteria = conditionsCriteria;
		}

		private ITickCondition[] _conditions = new ITickCondition[4];
		private ConditionsCriteria _conditionsCriteria = ConditionsCriteria.AnyTrue;
		private ITickableState _fromState;
		private ITickableState _toState;
		private MonoBehaviour _stateMachine;
	}
}