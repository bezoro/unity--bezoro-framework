using UnityEngine;

namespace Bezoro.StateMachines
{
	public abstract class BaseEventStateMachine : MonoBehaviour,
		IEventStateMachine
	{
		public IEventState CurrentState => _currentState;

		public IEventState LastState => _lastState;

		[field: SerializeField]
		public DisableMethod DisableMethod { get; private set; } = DisableMethod.Continue;

		private void Awake() =>
			SetUp();

		protected abstract void SetUp();

		protected void RegisterStartState<T>()
		where T : class, IEventState, new() =>
			_startState = CreateState<T>();

		protected void RegisterState<T>(out T createdState)
		where T : BaseEventState, new() =>
			createdState = CreateState<T>();

		private void Start()
		{
			_currentState = _startState;
			_currentState.Enter();
		}

		private void OnDisable()
		{
			_currentState.Disable();

			if (DisableMethod is DisableMethod.Restart)
				_currentState = _startState;
		}

		public void DoTransition(IEventState toState)
		{
			_lastState = _currentState;
			_currentState.Exit();
			_currentState = toState;
			_currentState.Enter();
		}

		public bool TryFindStateInCurrentHierarchy<T>(out IEventState outState)
		where T : class, IEventState
		{
			if (_currentState is T)
			{
				outState = _currentState;
				return true;
			}

			if (!IsSubStateMachine(_currentState, out var rootSuperState))
			{
				outState = null;
				return false;
			}

			var parentState = rootSuperState;

			while (true)
			{
				var currentState = parentState.CurrentState;

				if (currentState is T)
				{
					outState = currentState;
					return true;
				}

				if (!IsSubStateMachine(currentState, out var superState))
				{
					outState = null;
					return false;
				}

				parentState = superState;
			}
		}

		public bool TryFindStateInLastHierarchy<T>(out IEventState outState)
		where T : IEventState
		{
			if (_lastState is T)
			{
				outState = _lastState;
				return true;
			}

			if (!IsSubStateMachine(_lastState, out var rootSuperState))
			{
				outState = null;
				return false;
			}

			var parentState = rootSuperState;

			while (true)
			{
				var lastState = parentState.LastState;

				if (lastState is T)
				{
					outState = lastState;
					return true;
				}

				if (!IsSubStateMachine(lastState, out var superState))
				{
					outState = null;
					return false;
				}

				parentState = superState;
			}
		}

		private T CreateState<T>()
		where T : class, IEventState, new()
		{
			var newState = new T();
			newState.Init(this, this);
			return newState;
		}

		private static bool IsSubStateMachine
		(
			IEventState state,
			out IEventStateMachine outStateMachine
		)
		{
			if (state is IEventStateMachine stateMachine)
			{
				outStateMachine = stateMachine;
				return true;
			}

			outStateMachine = null;
			return false;
		}

		private IEventState _startState = new NullEventState();
		private IEventState _currentState = new NullEventState();
		private IEventState _lastState = new NullEventState();
	}

	[Tooltip
		(
			"What to do when Disabling the State Machine." + "/n" +
			"Restart = Restart as if it just started for the first time." +
			"Continue = Continue from where it was."
		)]
	public enum DisableMethod
	{
		None = 0,
		Restart = 1,
		Continue = 2
	}
}