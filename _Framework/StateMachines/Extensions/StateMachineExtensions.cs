using UnityEngine;

namespace Bezoro.StateMachines
{
	public static class StateMachineExtensions
	{
		public static void Start_Ext(this IStateMachine stateMachine)
		{
			stateMachine.SetCurrentState(stateMachine.StartState);
			stateMachine.CurrentState.Enter();
		}

		public static void Disable_Ext(this IStateMachine stateMachine)
		{
			stateMachine.CurrentState.Disable();
			if (stateMachine.DisableMethod is DisableMethod.Restart)
				stateMachine.SetCurrentState(stateMachine.StartState);
		}

		public static void DoTransition_Ext(this IStateMachine stateMachine, IState toState)
		{
			stateMachine.SetLastState(stateMachine.CurrentState);
			stateMachine.CurrentState.Exit();
			stateMachine.SetCurrentState(toState);
			stateMachine.CurrentState.Enter();
		}

		public static T CreateState<T>
			(this IStateMachine stateMachine, MonoBehaviour masterStateMachine)
		where T : class, IState, new()
		{
			var newState = new T();
			newState.Init(masterStateMachine, stateMachine);
			return newState;
		}

		public static bool TryFindStateInCurrentHierarchy<T>
			(this IStateMachine stateMachine, out T outState)
		where T : class, IState
		{
			var currentState = stateMachine.CurrentState;
			if (currentState is T)
			{
				outState = currentState as T;
				return true;
			}

			if (!currentState.IsSubStateMachine(out IStateMachine rootSubStateMachine))
			{
				outState = null;
				return false;
			}

			currentState = rootSubStateMachine.CurrentState;
			while (true)
			{
				if (currentState is T)
				{
					outState = currentState as T;
					return true;
				}

				if (!currentState.IsSubStateMachine(out IStateMachine subStateMachine))
				{
					outState = null;
					return false;
				}

				currentState = subStateMachine.CurrentState;
			}
		}

		public static bool TryFindStateInLastHierarchy<T>
			(this IStateMachine stateMachine, out T outState)
		where T : class, IState
		{
			var lastState = stateMachine.LastState;
			if (lastState is T)
			{
				outState = lastState as T;
				return true;
			}

			if (!lastState.IsSubStateMachine(out IStateMachine rootSubStateMachine))
			{
				outState = null;
				return false;
			}

			lastState = rootSubStateMachine.LastState;
			while (true)
			{
				if (lastState is T foundState1)
				{
					outState = foundState1;
					return true;
				}

				if (!lastState.IsSubStateMachine(out IStateMachine subStateMachine))
				{
					outState = null;
					return false;
				}

				lastState = subStateMachine.LastState;
			}
		}
	}
}