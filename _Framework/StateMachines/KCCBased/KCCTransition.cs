using UnityEngine;
using Bezoro.Core.Helpers;

namespace Bezoro.StateMachines
{
	public class KCCTransition :
		IKCCTransition
	{
		public void Init(IKCCState fromState, IKCCState toState, MonoBehaviour stateMachine)
		{
			_fromState = fromState;
			_toState = toState;
			_stateMachine = stateMachine;
		}

		void ITickTransition.Init
			(ITickableState fromState, ITickableState toState, MonoBehaviour stateMachine) =>
			Init(fromState as IKCCState, toState as IKCCState, stateMachine);

		void ITransition.Init(IState fromState, IState toState, MonoBehaviour stateMachine) =>
			Init(fromState as IKCCState, toState as IKCCState, stateMachine);

		public ITickableState FromState => _fromState;

		public ITickableState ToState => _toState;

		ITickCondition[] ITickTransition.Conditions => _conditions;

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
			ArrayHelpers.Add(ref _conditions, this.CreateCondition<T, IKCCTransition>(inverted));

		public void WithConditions<T0, T1>
		(ConditionsCriteria conditionsCriteria = ConditionsCriteria.AnyTrue,
		 bool inverted0 = false,
		 bool inverted1 = false)
		where T0 : class, ITickCondition, new()
		where T1 : class, ITickCondition, new()
		{
			ArrayHelpers.Add(ref _conditions, this.CreateCondition<T0, IKCCTransition>(inverted0));
			ArrayHelpers.Add(ref _conditions, this.CreateCondition<T1, IKCCTransition>(inverted1));
			_conditionsCriteria = conditionsCriteria;
		}

		public void WithConditions<T0, T1, T2>
		(ConditionsCriteria conditionsCriteria = ConditionsCriteria.AnyTrue,
		 bool inverted0 = false,
		 bool inverted1 = false,
		 bool inverted2 = false)
		where T0 : class, ITickCondition, new()
		where T1 : class, ITickCondition, new()
		where T2 : class, ITickCondition, new()
		{
			ArrayHelpers.Add(ref _conditions, this.CreateCondition<T0, IKCCTransition>(inverted0));
			ArrayHelpers.Add(ref _conditions, this.CreateCondition<T1, IKCCTransition>(inverted1));
			ArrayHelpers.Add(ref _conditions, this.CreateCondition<T2, IKCCTransition>(inverted2));
			_conditionsCriteria = conditionsCriteria;
		}

		private ITickCondition[] _conditions = new ITickCondition[4];
		private ConditionsCriteria _conditionsCriteria = ConditionsCriteria.AnyTrue;
		private IKCCState _fromState;
		private IKCCState _toState;
		private MonoBehaviour _stateMachine;
	}
}