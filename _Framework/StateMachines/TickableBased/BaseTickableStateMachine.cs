using Bezoro.Core;
using UnityEngine;

namespace Bezoro.StateMachines
{
	[DisallowMultipleComponent]
	public abstract class BaseTickableStateMachine : Tickable,
		ITickableStateMachine
	{
		private void Awake() =>
			SetUp();

		public ITickableState StartState => _startState;

		public ITickableState CurrentState => _currentState;

		public ITickableState LastState => _lastState;

		IState IStateMachine.StartState => _startState;

		IState IStateMachine.CurrentState => _currentState;

		IState IStateMachine.LastState => _lastState;

		[field: SerializeField]
		public DisableMethod DisableMethod { get; private set; }

		private void Start() =>
			this.Start_Ext();

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
			this.Disable_Ext();
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

		protected abstract void SetUp();

		protected void RegisterState<T>(out T outState)
		where T : class, ITickableState, new() =>
			outState = this.CreateState<T>(this);


		protected void SetStartState(ITickableState state) =>
			_startState = state;

		private ITickableState _startState = new NullTickableState();
		private ITickableState _currentState = new NullTickableState();
		private ITickableState _lastState = new NullTickableState();
	}
}