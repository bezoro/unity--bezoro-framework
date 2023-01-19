using Bezoro.Core;
using UnityEngine;

namespace Bezoro.StateMachines
{
	public abstract class BaseTickableStateMachine : Tickable,
		ITickableStateMachine
	{
		public ITickableState CurrentState => _currentState;

		public ITickableState LastState => _lastState;

		[field: SerializeField]
		public DisableMethod DisableMethod { get; private set; } = DisableMethod.Continue;

		private void Awake() =>
			SetUp();

		protected abstract void SetUp();

		protected void SetStartState(IEventState state) =>
			_startState = state;

		protected void RegisterState<T>(out T outState)
		where T : class, ITickableState, new() =>
			outState = CreateState<T>();

		private void Start()
		{
			_currentState = _startState;
			_currentState.Enter();
		}

		protected override void OnDisable()
		{
			base.OnDisable();

			_currentState.Disable();

			if (DisableMethod is DisableMethod.Restart)
				_currentState = _startState;
		}

		public override void EarlyTick() =>
			_currentState.EarlyTick();

		public override void Tick() =>
			_currentState.Tick();

		public override void FixedTick() =>
			_currentState.FixedTick();

		public override void LateTick() =>
			_currentState.LateTick();

		public void DoTransition(ITickableState toState)
		{
			_lastState = _currentState;
			_currentState.Exit();
			_currentState = toState;
			_currentState.Enter();
		}

		public bool TryFindStateInCurrentHierarchy<T>(out ITickableState outState)
		where T : class, ITickableState
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

		public bool TryFindStateInLastHierarchy<T>(out ITickableState outState)
		where T : ITickableState
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
		where T : class, ITickableState, new()
		{
			var newState = new T();
			newState.Init(this, this);
			return newState;
		}

		private static bool IsSubStateMachine
		(
			ITickableState state,
			out ITickableStateMachine outSubStateMachine
		)
		{
			if (state is ITickableStateMachine stateMachine)
			{
				outSubStateMachine = stateMachine;
				return true;
			}

			outSubStateMachine = null;
			return false;
		}

		private ITickableState _startState = new NullTickableState();
		private ITickableState _currentState = new NullTickableState();
		private ITickableState _lastState = new NullTickableState();
	}
}