using UnityEngine;
using UnityEngine.Assertions;
#if UNITY_EDITOR
using UnityEditor;
#endif

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

	#if UNITY_EDITOR
		private void OnDrawGizmos()
		{
			if (Application.isPlaying)
			{
				DrawStateLabels(this);

				void DrawStateLabels(IEventStateMachine stateMachine, int index = 0)
				{
					const float offset = 4f;
					var labelPosition = transform.position + Vector3.up * offset;

					Handles.Label
						(
						 labelPosition - (Vector3.up * index) * 0.75f,
						 stateMachine.CurrentState.GetType().Name
						);

					if (stateMachine.CurrentState is IEventStateMachine FSM)
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