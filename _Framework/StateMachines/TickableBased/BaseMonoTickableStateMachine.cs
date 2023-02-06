using Bezoro.Core;
using UnityEngine;

namespace Bezoro.StateMachines
{
	[DisallowMultipleComponent]
	public abstract class BaseMonoTickableStateMachine : Tickable,
		ITickableStateMachine
	{
		private void Awake()
		{
			RegisterStates();
			SetUpTransitions();
		}

		public ITickableState CurrentState => _currentState;

		public ITickableState LastState => _lastState;

		[field: SerializeField]
		public DisableMethod DisableMethod { get; private set; }

		private void Start()
		{
			_currentState = ResolveStartState();
			_currentState.Enter();
		}

		protected override void OnEnable()
		{
			base.OnEnable();
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

		public override void EarlyTick() =>
			_currentState.EarlyTick();

		public override void Tick() =>
			_currentState.Tick();

		public override void FixedTick() =>
			_currentState.FixedTick();

		public override void LateTick() =>
			_currentState.LateTick();

		protected override void OnDisable()
		{
			base.OnDisable();
			HandleDisableMethod();

			void HandleDisableMethod()
			{
				if (DisableMethod is DisableMethod.Restart)
					_restarted = true;
			}
		}

		public void DoTransition(ITickableState toState)
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
		protected abstract ITickableState ResolveStartState();

		#endregion

		protected T RegisterState<T>()
		where T : class, ITickableState, new()
		{
			var newState = new T();
			newState.Init(this, this);
			return newState;
		}

		private ITickableState _currentState;
		private ITickableState _lastState;
		private bool _restarted;
	}
}