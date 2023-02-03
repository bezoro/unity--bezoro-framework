using UnityEngine;
using Bezoro.Core.Helpers;

namespace Bezoro.StateMachines
{
	public abstract class BaseEventSubStateMachine :
		IEventStateMachine, IEventState
	{
		public void Init(MonoBehaviour masterStateMachine, IEventStateMachine owningStateMachine)
		{
			_masterStateMachine = masterStateMachine;
			_owningStateMachine = owningStateMachine;
			RegisterStates();
			SetUpTransitions();
		}

		public MonoBehaviour MasterStateMachine => _masterStateMachine;

		public IEventState CurrentState => _currentState;

		public IEventState LastState => _lastState;

		public void Enter()
		{
			OnEnter();
			AllTransitionsBecomeRelevant();
			_currentState = ResolveStartState();
			_currentState.Enter();

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
			_currentState.Exit();
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
			newTransition.Init(this, toState, _masterStateMachine);
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
		/// /// Use it like 'Awake' in a MonoBehaviour.
		/// </summary>
		protected abstract void RegisterStates();

		/// <summary>
		/// Called once in the lifetime of the object, when it is created.
		/// Use it to set up all of the <b>States</b> <b>Transitions</b>.
		/// </summary>
		protected abstract void SetUpTransitions();

		/// <summary>
		/// Called when the <b>SubStateMachine</b> is <b>Entered</b>.
		/// Use it to decide which should be the <b>Start</b> <b>State</b>, then return it.
		/// </summary>
		protected abstract IEventState ResolveStartState();

		/// <summary>
		/// Called when the <b>SubStateMachine</b> is <i>Entered</i>.
		/// </summary>
		protected abstract void OnEnter();

		/// <summary>
		/// Called when the <b>SubStateMachine</b> is <i>Exited</i>.
		/// </summary>
		protected abstract void OnExit();

		#endregion

		protected T RegisterState<T>()
		where T : class, IEventState, new()
		{
			var newState = new T();
			newState.Init(_masterStateMachine, this);
			return newState;
		}

		private MonoBehaviour _masterStateMachine;
		private IEventStateMachine _owningStateMachine;
		private IEventState _currentState;
		private IEventState _lastState;
		private EventTransition[] _transitions = new EventTransition[4];
	}
}