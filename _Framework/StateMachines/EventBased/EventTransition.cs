using System.Collections.Generic;

namespace Bezoro.StateMachines
{
	public sealed class EventTransition
	{
		public EventTransition
		(
			IEventState fromState,
			IEventState toState,
			BaseEventStateMachine masterStateMachine
		)
		{
			_fromState = fromState;
			_toState = toState;
			_masterStateMachine = masterStateMachine;
		}

		public void WithCondition<TCondition>()
		where TCondition : BaseEventCondition, new()
		{
			var newCondition = new TCondition();
			newCondition.Init(this, _masterStateMachine);
			_conditions.Add(newCondition);
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
		private readonly BaseEventStateMachine _masterStateMachine;
		private readonly List<BaseEventCondition> _conditions = new();
	}
}