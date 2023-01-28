using Bezoro.Core;
using UnityEngine;

namespace Bezoro.StateMachines
{
	[DisallowMultipleComponent]
	public abstract class BaseTickableStateMachine : Tickable,
		IMonoTickableStateMachine
	{
		private void Awake() =>
			this.Awake_Ext();

		public ITickableState CurrentState => _currentState;

		IState IStateMachine.CurrentState => _currentState;

		public ITickableState LastState => _lastState;

		IState IStateMachine.LastState => _lastState;

		public bool Restarted => _restarted;

		[field: SerializeField]
		public DisableMethod DisableMethod { get; private set; }

		private void Start() =>
			this.Start_Ext();

		protected override void OnEnable()
		{
			base.OnEnable();
			this.OnEnable_Ext();
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
			this.OnDisable_Ext();
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

		public void SetRestarted(bool value) =>
			_restarted = value;

		#region Abstract Methods

		public abstract void RegisterStates();

		public abstract void SetUpTransitions();

		public abstract IState ResolveStartState();

		#endregion

		protected T RegisterState<T>()
		where T : class, ITickableState, new() =>
			this.CreateState<T, ITickableStateMachine>(this);

		private ITickableState _currentState = new NullTickableState();
		private ITickableState _lastState = new NullTickableState();
		private bool _restarted;
	}
}