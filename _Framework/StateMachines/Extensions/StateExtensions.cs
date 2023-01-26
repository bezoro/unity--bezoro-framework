using UnityEngine;
using System.Collections;

namespace Bezoro.StateMachines
{
	public static class TickableStateExtensions
	{
		public static void AllTransitionsTick<T>(this T state)
		where T : ITickableState
		{
			var transitions = state.Transitions;
			for (var i = 0; i < transitions.Length; i++)
			{
				if (transitions[i] == null)
					continue;

				transitions[i].Tick();
			}
		}

		public static void AddMonoBehaviourTypeToIgnoreCollisionsWith<T>(this IKCCState state)
		where T : MonoBehaviour
		{
			var type = typeof(T);
			state.MonoBehaviourTypesToIgnoreCollisionsWith.Add(type);
		}
	}

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

		public static T0 CreateTransition<T0, T1, T2>(this T1 state, T2 toState)
		where T0 : class, ITransition, new()
		where T1 : class, IState
		where T2 : class, IState
		{
			var newTransition = new T0();
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

		public static bool IsSubStateMachine<T0, T1>(this T1 state, out T0 outStateMachine)
		where T0 : class, IStateMachine
		where T1 : class, IState
		{
			if (state is IStateMachine stateMachine)
			{
				outStateMachine = stateMachine as T0;
				return true;
			}

			outStateMachine = null;
			return false;
		}
	}
}