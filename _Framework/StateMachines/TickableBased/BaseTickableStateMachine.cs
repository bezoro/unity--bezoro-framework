using Bezoro.Core;
using UnityEngine;

namespace Bezoro.StateMachines
{
	[DisallowMultipleComponent]
	public abstract class BaseTickableStateMachine : Tickable,
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
			if (_restarted)
			{
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
			if (DisableMethod is DisableMethod.Restart)
				_restarted = true;
		}

		public void DoTransition(ITickableState toState)
		{
			_currentState.Exit();
			_lastState = _currentState;
			_currentState = toState;
			_currentState.Enter();
		}

		#region Abstract Methods

		protected abstract void RegisterStates();

		protected abstract void SetUpTransitions();

		protected abstract BaseTickableState ResolveStartState();

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