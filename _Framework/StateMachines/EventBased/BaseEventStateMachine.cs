using UnityEngine;

namespace Bezoro.StateMachines
{
	[DisallowMultipleComponent]
	public abstract class BaseEventStateMachine : MonoBehaviour,
		IMonoEventStateMachine
	{
		private void Awake() =>
			this.Awake_Ext();

		public IEventState CurrentState => _currentState;

		IState IStateMachine.CurrentState => _currentState;

		public IEventState LastState => _lastState;

		IState IStateMachine.LastState => _lastState;

		public bool Restarted => _restarted;

		[field: SerializeField]
		public DisableMethod DisableMethod { get; private set; }

		private void Start() =>
			this.Start_Ext();

		private void OnEnable() =>
			this.OnEnable_Ext();

		private void OnDisable() =>
			this.OnDisable_Ext();

		public void DoTransition(IEventState toState) =>
			this.DoTransition_Ext(toState);

		void IStateMachine.DoTransition(IState toState) =>
			this.DoTransition_Ext(toState);

		public void SetCurrentState(IEventState state) =>
			_currentState = state;

		void IStateMachine.SetCurrentState(IState state) =>
			_currentState = state as IEventState;

		public void SetLastState(IEventState state) =>
			_lastState = state;

		void IStateMachine.SetLastState(IState state) =>
			_lastState = state as IEventState;

		public void SetRestarted(bool value) =>
			_restarted = value;

		#region Abstract Methods

		public abstract void RegisterStates();

		public abstract void SetUpTransitions();

		public abstract IState ResolveStartState();

		#endregion

		protected T RegisterState<T>()
		where T : class, IEventState, new() =>
			this.CreateState<T, IEventStateMachine>(this);

		private IEventState _currentState = new NullEventState();
		private IEventState _lastState = new NullEventState();
		private bool _restarted;
	}
}