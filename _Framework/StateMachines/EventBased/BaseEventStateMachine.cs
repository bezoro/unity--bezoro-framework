using UnityEngine;

namespace Bezoro.StateMachines
{
	public abstract class BaseEventStateMachine : ScriptableObject,
		IEventStateMachine
	{
		protected const string MenuName = "Bezoro/StateMachines/Event/";

		public void Init(EventStateMachineOwner owner)
		{
			_owner = owner;
			RegisterStates();
			SetUpTransitions();
		}

		public IEventState CurrentState => _currentState;

		public IEventState LastState => _lastState;

		[field: SerializeField]
		public DisableMethod DisableMethod { get; private set; }

		public void Start()
		{
			_currentState = ResolveStartState();
			_currentState.Enter();
		}

		public void Enable()
		{
			HandleRestarted();

			void HandleRestarted()
			{
				if (!_restarted)
					return;

				_restarted = false;
				_currentState = ResolveStartState();
				_currentState.Enter();
			}
		}

		public void Disable()
		{
			HandleDisableMethod();

			void HandleDisableMethod()
			{
				if (DisableMethod is DisableMethod.Restart)
					_restarted = true;
			}
		}

		public void DoTransition(IEventState toState)
		{
			_currentState.Exit();
			_lastState = _currentState;
			_currentState = toState;
			_currentState.Enter();
		}

		public void Exit() =>
			_currentState.Exit();

		#region Abstract Methods

		/// <summary>
		/// Called once in the lifetime of the object, in <b>Awake</b>.
		/// Use it to register all <b>States</b> needed for the <b>StateMachine</b> to work.
		/// </summary>
		protected abstract void RegisterStates();

		/// <summary>
		/// Called once in the lifetime of the object, in <b>Awake</b>.
		/// Use it to set up all of the <b>States</b> <b>Transitions</b>.
		/// </summary>
		protected abstract void SetUpTransitions();

		/// <summary>
		/// Called once in the lifetime of the object, in <b>Start</b>.
		/// Use it to decide which should be the <b>Start</b> <b>State</b>, then return it.
		/// </summary>
		protected abstract IEventState ResolveStartState();

		#endregion

		protected T RegisterState<T>()
		where T : class, IEventState, new()
		{
			var newState = new T();
			newState.Init(_owner, this);
			return newState;
		}

		private EventStateMachineOwner _owner;
		private IEventState _currentState;
		private IEventState _lastState;
		private bool _restarted;
	}
}