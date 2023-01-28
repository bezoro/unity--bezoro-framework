using UnityEngine;

namespace Bezoro.StateMachines
{
	public static class StateMachineExtensions
	{
		public static void DoTransition_Ext<T0, T1>(this T0 stateMachine, T1 toState)
		where T0 : IStateMachine
		where T1 : class, IState
		{
			stateMachine.SetLastState(stateMachine.CurrentState);
			stateMachine.CurrentState.Exit();
			stateMachine.SetCurrentState(toState);
			stateMachine.CurrentState.Enter();
		}

		public static T0 CreateState<T0, T1>(this T1 stateMachine, MonoBehaviour masterStateMachine)
		where T0 : class, IState, new()
		where T1 : class, IStateMachine
		{
			var newState = new T0();
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

			if (!currentState.IsSubStateMachine(out ISubStateMachine rootSubStateMachine))
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

				if (!currentState.IsSubStateMachine(out ISubStateMachine subStateMachine))
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

			if (!lastState.IsSubStateMachine(out ISubStateMachine rootSubStateMachine))
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

				if (!lastState.IsSubStateMachine(out ISubStateMachine subStateMachine))
				{
					outState = null;
					return false;
				}

				lastState = subStateMachine.LastState;
			}
		}
	}
}