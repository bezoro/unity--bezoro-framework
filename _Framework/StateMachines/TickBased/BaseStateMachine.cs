using Bezoro.Core;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

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

		private void Awake() =>
			SetUp();

		private void Start() =>
			_currentState.Enter();

		private IState _currentState = new NullState();

	#if UNITY_EDITOR
		private void OnDrawGizmos()
		{
			if (!Application.isPlaying)
				return;

			DrawStateLabels(this);

			void DrawStateLabels(IStateMachine stateMachine, int index = 0)
			{
				while (true)
				{
					const float offset = 4f;
					var labelPosition = transform.position + Vector3.up * offset;

					Handles.Label
						(
						 labelPosition - (Vector3.up * index) * 0.75f,
						 stateMachine.CurrentState.GetType().Name
						);

					if (stateMachine.CurrentState is not IStateMachine fsm)
						return;

					index++;
					stateMachine = fsm;
				}
			}
		}
	#endif
	}
}