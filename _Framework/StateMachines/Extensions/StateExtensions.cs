using UnityEngine;
using System.Collections;

namespace Bezoro.StateMachines
{
	public static class StateExtensions
	{
		public static void AllTransitionsBecomeRelevant<T>(this T state)
		where T : IState
		{
			var transitions = state.Transitions;
			for (var i = 0; i < transitions.Length; i++)
			{
				if (transitions[i] == null)
					continue;

				transitions[i].BecomeRelevant();
			}
		}

		public static void AllTransitionsCeaseRelevant<T>(this T state)
		where T : IState
		{
			var transitions = state.Transitions;
			for (var i = 0; i < transitions.Length; i++)
			{
				if (transitions[i] == null)
					continue;

				transitions[i].CeaseRelevant();
			}
		}

		public static T CreateTransition<T>(this IState state, IState toState)
		where T : class, ITransition, new()
		{
			var newTransition = new T();
			newTransition.Init(state, toState, state.MasterStateMachine);
			return newTransition;
		}

		public static Coroutine StartCoroutine(this IState state, IEnumerator routine) =>
			state.MasterStateMachine.StartCoroutine(routine);

		public static void StopCoroutine(this IState state, Coroutine routine) =>
			state.MasterStateMachine.StopCoroutine(routine);

		public static void StopAllCoroutines(this IState state) =>
			state.MasterStateMachine.StopAllCoroutines();

		public static bool TryGetComponent<T>(this IState state, out T component)
		where T : Component
		{
			if (state.MasterStateMachine.TryGetComponent(out T c))
			{
				component = c;
				return true;
			}

			component = null;
			return false;
		}

		public static bool IsSubStateMachine<T0, T1>(this T0 state, out T1 outStateMachine)
		where T0 : class, IState
		where T1 : class, ISubStateMachine
		{
			if (state is ISubStateMachine stateMachine)
			{
				outStateMachine = stateMachine as T1;
				return true;
			}

			outStateMachine = null;
			return false;
		}
	}
}