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

		IState IStateMachine.CurrentState => _currentState;

		public ITickableState LastState => _lastState;

		IState IStateMachine.LastState => _lastState;

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

		public void DoTransition(ITickableState toState) =>
			this.DoTransition_Ext(toState);

		public void SetCurrentState(ITickableState state) =>
			_currentState = state;

		public void SetLastState(ITickableState state) =>
			_lastState = state;

		void IStateMachine.DoTransition(IState toState) =>
			this.DoTransition_Ext(toState);

		void IStateMachine.SetCurrentState(IState state) =>
			_currentState = state as ITickableState;

		void IStateMachine.SetLastState(IState state) =>
			_lastState = state as ITickableState;

		#region Abstract Methods

		protected abstract void RegisterStates();

		protected abstract void SetUpTransitions();

		protected abstract ITickableState ResolveStartState();

		#endregion

		protected T RegisterState<T>()
		where T : class, ITickableState, new() =>
			this.CreateState<T, ITickableStateMachine>(this);

		private ITickableState _currentState = new NullTickableState();
		private ITickableState _lastState = new NullTickableState();
		private bool _restarted;
	}
}