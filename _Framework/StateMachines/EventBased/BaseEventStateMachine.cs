using UnityEngine;

namespace Bezoro.StateMachines
{
	[DisallowMultipleComponent]
	public abstract class BaseEventStateMachine : MonoBehaviour,
		IEventStateMachine
	{
		private void Awake()
		{
			RegisterStates();
			SetUpTransitions();
		}

		public IEventState CurrentState => _currentState;

		IState IStateMachine.CurrentState => _currentState;

		public IEventState LastState => _lastState;

		IState IStateMachine.LastState => _lastState;

		[field: SerializeField]
		public DisableMethod DisableMethod { get; private set; }

		private void Start()
		{
			_currentState = ResolveStartState();
			_currentState.Enter();
		}

		private void OnEnable()
		{
			if (_restarted)
			{
				_currentState = ResolveStartState();
				_currentState.Enter();
			}
		}

		private void OnDisable()
		{
			if (DisableMethod is DisableMethod.Restart)
				_restarted = true;
		}

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

		#region Abstract Methods

		protected abstract void RegisterStates();

		protected abstract void SetUpTransitions();

		protected abstract IEventState ResolveStartState();

		#endregion

		protected T RegisterState<T>()
		where T : class, IEventState, new() =>
			this.CreateState<T, IEventStateMachine>(this);

		private IEventState _currentState = new NullEventState();
		private IEventState _lastState = new NullEventState();
		private bool _restarted;
	}
}