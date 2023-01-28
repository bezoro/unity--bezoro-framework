using UnityEngine;
using Bezoro.Core.Helpers;

namespace Bezoro.StateMachines
{
	public sealed class EventTransition :
		IEventTransition
	{
		public void Init(IEventState fromState, IEventState toState, MonoBehaviour stateMachine)
		{
			_fromState = fromState;
			_toState = toState;
			_stateMachine = stateMachine;
		}

		void ITransition.Init(IState fromState, IState toState, MonoBehaviour stateMachine) =>
			Init(fromState as IEventState, toState as IEventState, stateMachine);

		public ICondition[] Conditions => _conditions;

		public MonoBehaviour StateMachine => _stateMachine;

		public void WithCondition<T>()
		where T : class, IEventCondition, new() =>
			ArrayHelpers.Add(ref _conditions, this.CreateCondition<T, IEventTransition>());

		public void WithConditions<T0, T1>()
		where T0 : class, IEventCondition, new()
		where T1 : class, IEventCondition, new()
		{
			ArrayHelpers.Add(ref _conditions, this.CreateCondition<T0, IEventTransition>());
			ArrayHelpers.Add(ref _conditions, this.CreateCondition<T1, IEventTransition>());
		}

		public void WithConditions<T0, T1, T2>()
		where T0 : class, IEventCondition, new()
		where T1 : class, IEventCondition, new()
		where T2 : class, IEventCondition, new()
		{
			ArrayHelpers.Add(ref _conditions, this.CreateCondition<T0, IEventTransition>());
			ArrayHelpers.Add(ref _conditions, this.CreateCondition<T1, IEventTransition>());
			ArrayHelpers.Add(ref _conditions, this.CreateCondition<T2, IEventTransition>());
		}

		public void BecomeRelevant() =>
			this.AllConditionsBecomeRelevant();

		public void OnConditionRaised() =>
			_fromState.OnTransitionRaised(_toState);

		public void CeaseRelevant() =>
			this.AllConditionsCeaseRelevant();


		private ICondition[] _conditions = new ICondition[4];
		private IEventState _fromState;
		private IEventState _toState;
		private MonoBehaviour _stateMachine;
	}
}