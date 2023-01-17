using Bezoro.Core;
using UnityEngine;
using UnityEngine.Assertions;

namespace Bezoro.StateMachines
{
	public abstract class BaseStateMachine : Tickable,
		IStateMachine
	{
		public IState CurrentState => _currentState;

		public void SwitchCurrentState(IState stateToSwitchTo)
		{
			_currentState.Exit();
			_currentState = stateToSwitchTo;
			_currentState.Enter();
		}

		public override void EarlyTick() =>
			_currentState.EarlyTick();

		public override void Tick() =>
			_currentState.Tick();

		public override void FixedTick() =>
			_currentState.FixedTick();

		public override void LateTick() =>
			_currentState.LateTick();

		protected abstract void SetUp();

		protected void RegisterState<TState>(out TState createdState) where TState : IState, new()
		{
			var newState = new TState();
			newState.Init(this, this);
			createdState = newState;

			if (_currentState is NullState)
				_currentState = newState;
		}

		private void Awake()
		{
			SetUp();
			Assert.IsNotNull(_currentState);
		}

		private void Start() =>
			_currentState.Enter();

		private IState _currentState = new NullState();
	}
}