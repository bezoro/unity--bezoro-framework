using UnityEngine;
using Bezoro.Framework.Core.Helpers;

namespace Bezoro.Framework.StateMachines
{
	public abstract class BaseEventSubStateMachine :
		IEventStateMachine, IEventState
	{
		public void Init(EventStateMachineOwner owner, IEventStateMachine owningStateMachine)
		{
			_owner = owner;
			_owningStateMachine = owningStateMachine;
			RegisterStates();
			SetUpTransitions();
			OnInit();
		}

		public MonoBehaviour Owner => _owner;

		public IEventState CurrentState => _currentState;

		public IEventState LastState => _lastState;

		public EventTransition[] Transitions => _transitions;

		public void Enable()
		{
			OnEnable();
			AllTransitionsBecomeRelevant();
			_currentState.Enable();
		}

		public void Enter()
		{
			OnEnter();
			AllTransitionsBecomeRelevant();
			_currentState = ResolveStartState();
			_currentState.Enter();
		}

		public void OnTransitionRaised(IEventState toState) =>
			_owningStateMachine.DoTransition(toState);

		public void Exit()
		{
			OnExit();
			AllTransitionsCeaseRelevant();
			_currentState.Exit();
		}

		public void Disable()
		{
			OnDisable();
			AllTransitionsCeaseRelevant();
			_currentState.Disable();
		}

		public void Destroy()
		{
			OnDestroy();
			AllTransitionsCeaseRelevant();
			_currentState.Destroy();
		}

		public EventTransition TransitionsTo(IEventState toState)
		{
			var newTransition = new EventTransition();
			newTransition.Init(this, toState, _owner);
			ArrayHelpers.Add(ref _transitions, newTransition);
			return newTransition;
		}

		public void DoTransition(IEventState toState)
		{
			_currentState.Exit();
			_lastState = _currentState;
			_currentState = toState;
			_currentState.Enter();
		}

		#region Abstract Methods

		/// <summary>
		/// Called once in the lifetime of the object, when it is created.
		/// Use it to register all <b>States</b> needed for the <b>SubStateMachine</b> to work.
		/// </summary>
		protected abstract void RegisterStates();

		/// <summary>
		/// Called once in the lifetime of the object, when it is created.
		/// Use it to set up all of the <b>States</b> <b>Transitions</b>.
		/// </summary>
		protected abstract void SetUpTransitions();

		/// <summary>
		/// Called once in the lifetime of the object, when it is created.
		/// Use it like <b>Awake</b> in a <b>MonoBehaviour</b>.
		/// </summary>
		protected abstract void OnInit();

		/// <summary>
		/// Called when the <b>SubStateMachine</b> is <b>Entered</b>.
		/// Use it to decide which should be the <b>Start</b> <b>State</b>, then return it.
		/// </summary>
		protected abstract IEventState ResolveStartState();

		/// <summary>
		/// Called when the <b>Owner</b> gets <i>Enabled</i> and was previously <i>Disabled</i>,
		/// while this object is still being executed.
		/// </summary>
		protected abstract void OnEnable();

		/// <summary>
		/// Called when the <b>SubStateMachine</b> is <i>Entered</i>.
		/// </summary>
		protected abstract void OnEnter();

		/// <summary>
		/// Called when the <b>SubStateMachine</b> is <i>Exited</i>.
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

		protected T RegisterState<T>()
		where T : class, IEventState, new()
		{
			var newState = new T();
			newState.Init(_owner, this);
			return newState;
		}

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
		private IEventState _currentState;
		private IEventState _lastState;
		private EventTransition[] _transitions = new EventTransition[4];
	}
}