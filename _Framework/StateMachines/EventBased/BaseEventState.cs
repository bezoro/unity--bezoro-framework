using UnityEngine;
using Bezoro.Core.Helpers;

namespace Bezoro.StateMachines
{
	public abstract class BaseEventState : IEventState
	{
		public void Init(MonoBehaviour masterStateMachine, IEventStateMachine owningStateMachine)
		{
			_masterStateMachine = masterStateMachine;
			_owningStateMachine = owningStateMachine;
			OnInit();
		}

		public MonoBehaviour MasterStateMachine => _masterStateMachine;

		public EventTransition[] Transitions => _transitions;

		public void Enter()
		{
			OnEnter();
			for (var i = 0; i < _transitions.Length; i++)
			{
				if (_transitions[i] == null)
					continue;

				_transitions[i].BecomeRelevant();
			}
		}

		public void OnTransitionRaised(IEventState toState) =>
			_owningStateMachine.DoTransition(toState);

		public void Exit()
		{
			for (var i = 0; i < _transitions.Length; i++)
			{
				if (_transitions[i] == null)
					continue;

				_transitions[i].CeaseRelevant();
			}

			OnExit();
		}

		public EventTransition TransitionsTo(IEventState toState)
		{
			var newTransition = new EventTransition();
			newTransition.Init(this, toState, _masterStateMachine);
			ArrayHelpers.Add(ref _transitions, newTransition);
			return newTransition;
		}

		#region Abstract Methods

		protected abstract void OnInit();

		protected abstract void OnEnter();

		protected abstract void OnExit();

		#endregion

		private MonoBehaviour _masterStateMachine;
		private IEventStateMachine _owningStateMachine;
		private EventTransition[] _transitions = new EventTransition[4];
	}
}