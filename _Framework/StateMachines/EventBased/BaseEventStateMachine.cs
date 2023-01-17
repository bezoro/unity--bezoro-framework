using UnityEngine;
using UnityEngine.Assertions;

namespace Bezoro.StateMachines
{
	public abstract class BaseEventStateMachine : MonoBehaviour,
		IEventStateMachine
	{
		public IEventState CurrentState => _currentState;

		public void SwitchCurrentState(IEventState stateToSwitchTo)
		{
			_currentState.Exit();
			_currentState = stateToSwitchTo;
			_currentState.Enter();
		}

		protected void RegisterState<TState>(out TState createdState)
		where TState : IEventState, new()
		{
			var newState = new TState();
			newState.Init(this, this);
			createdState = newState;

			if (_currentState is NullEventState)
				_currentState = newState;
		}

		protected abstract void SetUp();

		private void Awake()
		{
			SetUp();
			Assert.IsNotNull(CurrentState);
		}

		private void Start() =>
			_currentState.Enter();

		private IEventState _currentState = new NullEventState();
	}
}