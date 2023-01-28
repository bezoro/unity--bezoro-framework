using UnityEngine;

namespace Bezoro.StateMachines
{
	public interface IKCCTransition : ITickTransition
	{
		public void Init(IKCCState fromState, IKCCState toState, MonoBehaviour stateMachine);
	}

	public interface ITickTransition : ITransition
	{
		public void Init
			(ITickableState fromState, ITickableState toState, MonoBehaviour stateMachine);

		public ITickableState FromState { get; }

		public ITickableState ToState { get; }

		public new ITickCondition[] Conditions { get; }

		public ConditionsCriteria ConditionsCriteria { get; }

		public void WithCondition<T>(bool inverted = false)
		where T : class, ITickCondition, new();

		public void WithConditions<T0, T1>
		(ConditionsCriteria conditionsCriteria,
		 bool inverted0 = false,
		 bool inverted1 = false)
		where T0 : class, ITickCondition, new()
		where T1 : class, ITickCondition, new();

		public void WithConditions<T0, T1, T2>
		(ConditionsCriteria conditionsCriteria,
		 bool inverted0 = false,
		 bool inverted1 = false,
		 bool inverted2 = false)
		where T0 : class, ITickCondition, new()
		where T1 : class, ITickCondition, new()
		where T2 : class, ITickCondition, new();

		public void Tick();
	}

	public interface IEventTransition : ITransition
	{
		public void Init(IEventState fromState, IEventState toState, MonoBehaviour stateMachine);

		public void WithCondition<T>()
		where T : class, IEventCondition, new();

		public void WithConditions<T0, T1>()
		where T0 : class, IEventCondition, new()
		where T1 : class, IEventCondition, new();

		public void WithConditions<T0, T1, T2>()
		where T0 : class, IEventCondition, new()
		where T1 : class, IEventCondition, new()
		where T2 : class, IEventCondition, new();

		public void OnConditionRaised();
	}

	public interface ITransition
	{
		public void Init(IState fromState, IState toState, MonoBehaviour stateMachine);

		public ICondition[] Conditions { get; }

		public MonoBehaviour StateMachine { get; }

		public void BecomeRelevant();

		public void CeaseRelevant();
	}
}