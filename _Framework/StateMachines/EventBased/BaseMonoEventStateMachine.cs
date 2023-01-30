using UnityEngine;

namespace Bezoro.StateMachines
{
	[DisallowMultipleComponent]
	public abstract class BaseMonoEventStateMachine : MonoBehaviour, IEventStateMachine
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
			if (_restarted)
			{
				_restarted = false;
				_currentState = ResolveStartState();
				_currentState.Enter();
			}
		}

		private void OnDisable()
		{
			if (DisableMethod is DisableMethod.Restart)
				_restarted = true;
		}

		public void DoTransition(IEventState toState)
		{
			_currentState.Exit();
			_lastState = _currentState;
			_currentState = toState;
			_currentState.Enter();
		}

		#region Abstract Methods

		protected abstract void RegisterStates();

		protected abstract void SetUpTransitions();

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