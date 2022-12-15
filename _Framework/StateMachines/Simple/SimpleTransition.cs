using System.Collections.Generic;

namespace Bezoro.StateMachines
{
	public sealed class SimpleTransition
	{
		public SimpleTransition
		(
			ISimpleState fromState,
			ISimpleState toState,
			BaseSimpleStateMachine masterStateMachine
		)
		{
			_fromState = fromState;
			_toState = toState;
			_masterStateMachine = masterStateMachine;
		}

		public void WithCondition<TCondition>()
		where TCondition : BaseSimpleCondition, new()
		{
			var newCondition = new TCondition();
			newCondition.Init(this, _masterStateMachine);
			_conditions.Add(newCondition);
		}

		public void BecomeRelevant()
		{
			foreach (var condition in _conditions)
				condition.BecomeRelevant();
		}

		public void NotifyStateOfConditionRaise() =>
			_fromState.SwitchOwningStateMachineCurrentState(_toState);

		public void CeaseRelevant()
		{
			foreach (var condition in _conditions)
				condition.CeaseRelevant();
		}

		private readonly ISimpleState _fromState;
		private readonly ISimpleState _toState;
		private readonly BaseSimpleStateMachine _masterStateMachine;
		private readonly List<BaseSimpleCondition> _conditions = new();
	}
}