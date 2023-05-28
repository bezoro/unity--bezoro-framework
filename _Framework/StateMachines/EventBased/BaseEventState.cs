using UnityEngine;
using Bezoro.Framework.Core.Helpers;

namespace Bezoro.Framework.StateMachines
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

		public void Enable()
		{
			OnEnable();
			AllTransitionsBecomeRelevant();
		}

		public void Enter()
		{
			OnEnter();
			AllTransitionsBecomeRelevant();
		}

		public void OnTransitionRaised(IEventState toState) =>
			_owningStateMachine.DoTransition(toState);

		public void Exit()
		{
			OnExit();
			AllTransitionsCeaseRelevant();
		}

		public void Disable()
		{
			OnDisable();
			AllTransitionsCeaseRelevant();
		}


		public void Destroy()
		{
			OnDestroy();
			AllTransitionsCeaseRelevant();
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
		/// Called when the <b>Owner</b> gets <i>Enabled</i> and was previously <i>Disabled</i>,
		/// while this object is still being executed.
		/// </summary>
		protected abstract void OnEnable();

		/// <summary>
		/// Called when the <b>State</b> is <i>Entered</i>.
		/// </summary>
		protected abstract void OnEnter();

		/// <summary>
		/// Called when the <b>State</b> is <i>Exited</i>.
		/// </summary>
		protected abstract void OnExit();

		/// <summary>
		/// Called when the <b>Owner</b> is <i>Disabled</i> while this object is still being executed.
		/// </summary>
		protected abstract void OnDisable();

		/// <summary>
		/// Called when the <b>Owner</b> is <i>Destroyed</i> while this object is still being executed.
		/// </summary>
		protected abstract void OnDestroy();

		#endregion

		private void AllTransitionsBecomeRelevant()
		{
			for (var i = 0; i < _transitions.Length; i++)
			{
				if (_transitions[i] == null)
					continue;

				_transitions[i].BecomeRelevant();
			}
		}

		private void AllTransitionsCeaseRelevant()
		{
			for (var i = 0; i < _transitions.Length; i++)
			{
				if (_transitions[i] == null)
					continue;

				_transitions[i].CeaseRelevant();
			}
		}

		private EventStateMachineOwner _owner;
		private IEventStateMachine _owningStateMachine;
		private EventTransition[] _transitions = new EventTransition[4];
	}
}