using Bezoro.Framework.Unity.StateMachines;
using Bezoro.Framework.Unity.States;

namespace Bezoro.Framework.Unity.Utilities
{
	public static class StateMachineExtensions
	{
		public static bool TryFindStateInCurrentHierarchy<T0, T1>
			( this IStateMachine<T0> stateMachine, out T1 outState )
		where T0 : class, IState
		where T1 : class, IState
		{
			var currentState = stateMachine.CurrentState;
			if ( currentState is T1 )
			{
				outState = currentState as T1;

				return true;
			}

			if ( currentState is not IStateMachine<T0> rootSubStateMachine )
			{
				outState = null;

				return false;
			}

			currentState = rootSubStateMachine.CurrentState;
			while (true)
			{
				if ( currentState is T1 )
				{
					outState = currentState as T1;

					return true;
				}

				if ( currentState is not IStateMachine<T0> subStateMachine )
				{
					outState = null;

					return false;
				}

				currentState = subStateMachine.CurrentState;
			}
		}

		public static bool TryFindStateInLastHierarchy<T0, T1>
			( this IStateMachine<T0> stateMachine, out T1 outState )
		where T0 : class, IState
		where T1 : class, IState
		{
			var lastState = stateMachine.LastState;
			if ( lastState is T1 )
			{
				outState = lastState as T1;

				return true;
			}

			if ( lastState is not IStateMachine<T0> rootSubStateMachine )
			{
				outState = null;

				return false;
			}

			lastState = rootSubStateMachine.LastState;
			while (true)
			{
				if ( lastState is T1 )
				{
					outState = lastState as T1;

					return true;
				}

				if ( lastState is not IStateMachine<T0> subStateMachine )
				{
					outState = null;

					return false;
				}

				lastState = subStateMachine.LastState;
			}
		}
	}
}
