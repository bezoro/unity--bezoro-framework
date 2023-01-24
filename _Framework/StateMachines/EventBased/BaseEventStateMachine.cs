using UnityEngine;

namespace Bezoro.StateMachines
{
	[DisallowMultipleComponent]
	public abstract class BaseEventStateMachine : MonoBehaviour,
		IEventStateMachine
	{
		private void Awake() =>
			SetUp();

		public IState StartState => _startState;

		public IEventState CurrentState => _currentState;

		public IEventState LastState => _lastState;

		IEventState IEventStateMachine.StartState => _startState;

		IState IStateMachine.CurrentState => _currentState;

		IState IStateMachine.LastState => _lastState;

		[field: SerializeField]
		public DisableMethod DisableMethod { get; private set; } = DisableMethod.Continue;

		private void Start() =>
			this.Start_Ext();

		private void OnDisable() =>
			this.Disable_Ext();

		public void DoTransition(IEventState toState) =>
			this.DoTransition_Ext(toState);

		void IStateMachine.DoTransition(IState toState) =>
			this.DoTransition_Ext(toState);

		public void SetCurrentState(IEventState state) =>
			_currentState = state;

		public void SetLastState(IEventState state) =>
			_lastState = state;

		void IStateMachine.SetCurrentState(IState state) =>
			_currentState = state as IEventState;

		void IStateMachine.SetLastState(IState state) =>
			_lastState = state as IEventState;

		protected abstract void SetUp();

		protected void SetStartState(IEventState state) =>
			_startState = state;

		protected void RegisterState<T>(out T createdState)
		where T : class, IEventState, new() =>
			createdState = this.CreateState<T>(this);

		private IEventState _startState = new NullEventState();
		private IEventState _currentState = new NullEventState();
		private IEventState _lastState = new NullEventState();
	}
}