using UnityEngine;

namespace Bezoro.StateMachines
{
	[DisallowMultipleComponent]
	public abstract class BaseMonoEventStateMachine : MonoBehaviour,
		IEventStateMachine
	{
		private void Awake()
		{
			RegisterStates();
			SetUpTransitions();
		}

		public IEventState CurrentState => _currentState;

		public IEventState LastState => _lastState;

		public bool Restarted => _restarted;

		[field: SerializeField]
		public DisableMethod DisableMethod { get; private set; }

		private void Start()
		{
			_currentState = ResolveStartState();
			_currentState.Enter();
		}

		private void OnEnable()
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

		private void OnDisable()
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

		public bool TryFindStateInCurrentHierarchy<T>(out T outState)
		where T : class, IState
		{
			var boolean = StateMachineExtensions.TryFindStateInCurrentHierarchy(this, out T state);
			outState = state;
			return boolean;
		}

		public bool TryFindStateInLastHierarchy<T>(out T outState)
		where T : class, IState
		{
			var boolean = StateMachineExtensions.TryFindStateInLastHierarchy(this, out T state);
			outState = state;
			return boolean;
		}

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
			newState.Init(this, this);
			return newState;
		}

		private IEventState _currentState;
		private IEventState _lastState;
		private bool _restarted;
	}
}