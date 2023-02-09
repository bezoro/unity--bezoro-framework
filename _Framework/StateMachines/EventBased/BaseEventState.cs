using UnityEngine;
using Bezoro.Core.Helpers;

namespace Bezoro.StateMachines
{
	public abstract class BaseEventState :
		IEventState
	{
		public void Init(EventStateMachineOwner owner, IEventStateMachine owningStateMachine)
		{
			_owner = owner;
			_owningStateMachine = owningStateMachine;
			OnInit();
		}

		public MonoBehaviour Owner => _owner;

		public EventTransition[] Transitions => _transitions;

		public void Enter()
		{
			OnEnter();
			AllTransitionsBecomeRelevant();

			void AllTransitionsBecomeRelevant()
			{
				for (var i = 0; i < _transitions.Length; i++)
				{
					if (_transitions[i] == null)
						continue;

					_transitions[i].BecomeRelevant();
				}
			}
		}

		public void OnTransitionRaised(IEventState toState) =>
			_owningStateMachine.DoTransition(toState);

		public void Exit()
		{
			AllTransitionsCeaseRelevant();
			OnExit();

			void AllTransitionsCeaseRelevant()
			{
				for (var i = 0; i < _transitions.Length; i++)
				{
					if (_transitions[i] == null)
						continue;

					_transitions[i].CeaseRelevant();
				}
			}
		}

		public EventTransition TransitionsTo(IEventState toState)
		{
			var newTransition = new EventTransition();
			newTransition.Init(this, toState, _owner);
			ArrayHelpers.Add(ref _transitions, newTransition);
			return newTransition;
		}

		#region Abstract Methods

		/// <summary>
		/// Called once in the lifetime of the object, when it is created.
		/// Use it like <b>Awake</b> in a <b>MonoBehaviour</b>.
		/// </summary>
		protected abstract void OnInit();

		/// <summary>
		/// Called once when the <b>State</b> is <i>Entered</i>.
		/// </summary>
		protected abstract void OnEnter();

		/// <summary>
		/// Called once when the <b>State</b> is <i>Exited</i>.
		/// </summary>
		protected abstract void OnExit();

		#endregion

		private EventStateMachineOwner _owner;
		private IEventStateMachine _owningStateMachine;
		private EventTransition[] _transitions = new EventTransition[4];
	}
}