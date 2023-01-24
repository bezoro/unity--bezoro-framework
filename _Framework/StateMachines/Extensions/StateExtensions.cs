using UnityEngine;
using System.Collections;

namespace Bezoro.StateMachines
{
	public static class TickableStateExtensions
	{
		public static void AllTransitionsTick(this ITickableState state)
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
		public static void AllTransitionsBecomeRelevant(this IState state)
		{
			var transitions = state.Transitions;
			for (var i = 0; i < transitions.Length; i++)
			{
				if (transitions[i] == null)
					continue;

				transitions[i].BecomeRelevant();
			}
		}

		public static void AllTransitionsCeaseRelevant(this IState state)
		{
			var transitions = state.Transitions;
			for (var i = 0; i < transitions.Length; i++)
			{
				if (transitions[i] == null)
					continue;

				transitions[i].CeaseRelevant();
			}
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

		public static bool IsSubStateMachine<T>(this IState state, out T outStateMachine)
		where T : class, IStateMachine
		{
			if (state is IStateMachine stateMachine)
			{
				outStateMachine = stateMachine as T;
				return true;
			}

			outStateMachine = null;
			return false;
		}
	}
}