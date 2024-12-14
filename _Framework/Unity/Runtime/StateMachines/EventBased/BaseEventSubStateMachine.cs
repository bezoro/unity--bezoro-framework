using Bezoro.Framework.Core;
using UnityEngine;

namespace Bezoro.Framework.Unity.StateMachines.EventBased
{
	public abstract class BaseEventSubStateMachine :
		IEventStateMachine,
		IEventState
	{
		private EventStateMachineOwner _owner;
		private IEventStateMachine     _owningStateMachine;
		private EventTransition[]      _transitions = new EventTransition[4];

		public MonoBehaviour Owner => _owner;

		public void Init(EventStateMachineOwner owner, IEventStateMachine owningStateMachine)
		{
			_owner              = owner;
			_owningStateMachine = owningStateMachine;
			RegisterStates();
			SetUpTransitions();
			OnInit();
		}

		public void Enable()
		{
			OnEnable();
			AllTransitionsBecomeRelevant();
			CurrentState.Enable();
		}

		public void Enter()
		{
			OnEnter();
			AllTransitionsBecomeRelevant();
			CurrentState = ResolveStartState();
			CurrentState.Enter();
		}

		public void OnTransitionRaised(IEventState toState)
		{
			_owningStateMachine.DoTransition(toState);
		}

		public void Exit()
		{
			OnExit();
			AllTransitionsCeaseRelevant();
			CurrentState.Exit();
		}

		public void Disable()
		{
			OnDisable();
			AllTransitionsCeaseRelevant();
			CurrentState.Disable();
		}

		public void Destroy()
		{
			OnDestroy();
			AllTransitionsCeaseRelevant();
			CurrentState.Destroy();
		}

		public void DoTransition(IEventState toState)
		{
			CurrentState.Exit();
			LastState    = CurrentState;
			CurrentState = toState;
			CurrentState.Enter();
		}

		public EventTransition TransitionsTo(IEventState toState)
		{
			var new_transition = new EventTransition();
			new_transition.Init(this, toState, _owner);
			Array_Helpers.Add(ref _transitions, new_transition);
			return new_transition;
		}

		public EventTransition[] Transitions => _transitions;

		public IEventState CurrentState { get; private set; }

		public IEventState LastState { get; private set; }

		private void AllTransitionsBecomeRelevant()
		{
			for (var i = 0 ; i < _transitions.Length ; i++)
			{
				if (_transitions[i] == null)
				{
					continue;
				}

				_transitions[i].BecomeRelevant();
			}
		}

		private void AllTransitionsCeaseRelevant()
		{
			for (var i = 0 ; i < _transitions.Length ; i++)
			{
				if (_transitions[i] == null)
				{
					continue;
				}

				_transitions[i].CeaseRelevant();
			}
		}

		protected T RegisterState<T>()
		where T : class, IEventState, new()
		{
			var newState = new T();

			newState.Init(
				_owner,
				this
			);

			return newState;
		}

	#region Abstract Methods

		/// <summary>
		///     Called once in the lifetime of the object, when it is created.
		///     Use it to register all <b>States</b> needed for the <b>SubStateMachine</b> to work.
		/// </summary>
		protected abstract void RegisterStates();

		/// <summary>
		///     Called once in the lifetime of the object, when it is created.
		///     Use it to set up all of the <b>States</b> <b>Transitions</b>.
		/// </summary>
		protected abstract void SetUpTransitions();

		/// <summary>
		///     Called once in the lifetime of the object, when it is created.
		///     Use it like <b>Awake</b> in a <b>MonoBehaviour</b>.
		/// </summary>
		protected abstract void OnInit();

		/// <summary>
		///     Called when the <b>SubStateMachine</b> is <b>Entered</b>.
		///     Use it to decide which should be the <b>Start</b> <b>State</b>, then return it.
		/// </summary>
		protected abstract IEventState ResolveStartState();

		/// <summary>
		///     Called when the <b>Owner</b> gets <i>Enabled</i> and was previously <i>Disabled</i>,
		///     while this object is still being executed.
		/// </summary>
		protected abstract void OnEnable();

		/// <summary>
		///     Called when the <b>SubStateMachine</b> is <i>Entered</i>.
		/// </summary>
		protected abstract void OnEnter();

		/// <summary>
		///     Called when the <b>SubStateMachine</b> is <i>Exited</i>.
		/// </summary>
		protected abstract void OnExit();

		/// <summary>
		///     Called when the <b>Owner</b> is <i>Disabled</i> while this object is still being executed.
		/// </summary>
		protected abstract void OnDisable();

		/// <summary>
		///     Called when the <b>Owner</b> is <i>Destroyed</i> while this object is still being executed.
		/// </summary>
		protected abstract void OnDestroy();

	#endregion
	}
}
