using System.Collections.Generic;
using UnityEngine;

namespace Bezoro.StateMachines
{
	public sealed class EventTransition :
		IEventTransition
	{
		public EventTransition
		(IEventState fromState,
		 IEventState toState,
		 MonoBehaviour stateMachine)
		{
			_fromState = fromState;
			_toState = toState;
			_stateMachine = stateMachine;
		}

		public void WithCondition<T>()
		where T : BaseEventCondition, new()
		{
			var newCondition = new T();
			newCondition.Init(this, _stateMachine);
			_conditions.Add(newCondition);
		}

		public void WithConditions<T0, T1>()
		where T0 : BaseEventCondition, new()
		where T1 : BaseEventCondition, new()
		{
			var newCondition0 = new T0();
			newCondition0.Init(this, _stateMachine);
			_conditions.Add(newCondition0);

			var newCondition1 = new T1();
			newCondition1.Init(this, _stateMachine);
			_conditions.Add(newCondition1);
		}

		public void WithConditions<T0, T1, T2>()
		where T0 : BaseEventCondition, new()
		where T1 : BaseEventCondition, new()
		where T2 : BaseEventCondition, new()
		{
			var newCondition0 = new T0();
			newCondition0.Init(this, _stateMachine);
			_conditions.Add(newCondition0);

			var newCondition1 = new T1();
			newCondition1.Init(this, _stateMachine);
			_conditions.Add(newCondition1);

			var newCondition2 = new T2();
			newCondition2.Init(this, _stateMachine);
			_conditions.Add(newCondition2);
		}

		public void BecomeRelevant()
		{
			for (var i = 0; i < _conditions.Count; i++)
				_conditions[i].BecomeRelevant();
		}

		public void OnConditionRaised() =>
			_fromState.OnTransitionRaised(_toState);

		public void CeaseRelevant()
		{
			for (var i = 0; i < _conditions.Count; i++)
				_conditions[i].CeaseRelevant();
		}

		private readonly IEventState _fromState;
		private readonly IEventState _toState;
		private readonly MonoBehaviour _stateMachine;
		private readonly List<BaseEventCondition> _conditions = new();
	}
}