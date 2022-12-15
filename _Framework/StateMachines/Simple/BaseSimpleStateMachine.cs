using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Bezoro.StateMachines
{
	public abstract class BaseSimpleStateMachine : MonoBehaviour,
		ISimpleStateMachine
	{
		public ISimpleState CurrentState => _currentState;

		public void SwitchCurrentState(ISimpleState stateToSwitchTo)
		{
			_currentState.Exit();
			_currentState = stateToSwitchTo;
			_currentState.Enter();
		}

		protected void RegisterState<TState>(out TState createdState)
		where TState : ISimpleState, new()
		{
			var newState = new TState();
			newState.Init(this, this);
			createdState = newState;

			if (_currentState is NullSimpleState)
				_currentState = newState;
		}

		protected abstract void SetUp();

		private void Awake() => SetUp();

		private void Start() => _currentState.Enter();

		private ISimpleState _currentState = new NullSimpleState();

		#if UNITY_EDITOR
		private void OnDrawGizmos()
		{
			if (Application.isPlaying)
			{
				DrawStateLabels(this);

				void DrawStateLabels(ISimpleStateMachine stateMachine, int index = 0)
				{
					const float offset = 4f;
					var labelPosition = transform.position + Vector3.up * offset;

					Handles.Label
					(
						labelPosition - (Vector3.up * index) * 0.75f,
						stateMachine.CurrentState.GetType().Name
					);

					if (stateMachine.CurrentState is ISimpleStateMachine FSM)
					{
						index++;
						DrawStateLabels(FSM, index);
					}
				}
			}
		}
		#endif
	}
}