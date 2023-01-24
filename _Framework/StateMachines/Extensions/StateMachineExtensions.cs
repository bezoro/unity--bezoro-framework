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
			if (stateMachine.CurrentState is T foundState0)
			{
				outState = foundState0;
				return true;
			}

			if (!stateMachine.CurrentState.IsSubStateMachine(out var rootSubStateMachine0))
			{
				outState = null;
				return false;
			}

			while (true)
			{
				if (rootSubStateMachine0.CurrentState is T foundState1)
				{
					outState = foundState1;
					return true;
				}

				if (!stateMachine.CurrentState.IsSubStateMachine(out var rootSubStateMachine1))
				{
					outState = null;
					return false;
				}

				rootSubStateMachine0 = rootSubStateMachine1;
			}
		}

		public static bool TryFindStateInLastHierarchy<T>
			(this IStateMachine stateMachine, out T outState)
		where T : class, IState
		{
			var lastState = stateMachine.LastState;

			if (lastState is T foundState0)
			{
				outState = foundState0;
				return true;
			}

			if (!lastState.IsSubStateMachine(out var rootSubStateMachine))
			{
				outState = null;
				return false;
			}

			while (true)
			{
				lastState = rootSubStateMachine.LastState;
				if (lastState is T foundState1)
				{
					outState = foundState1;
					return true;
				}

				if (!lastState.IsSubStateMachine(out var subStateMachine))
				{
					outState = null;
					return false;
				}

				rootSubStateMachine = subStateMachine;
			}
		}
	}
}