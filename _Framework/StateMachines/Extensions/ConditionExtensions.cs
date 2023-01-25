using UnityEngine;
using System.Collections;

namespace Bezoro.StateMachines
{
	public static class TickConditionExtensions
	{
		public static bool Tick_Ext(this ITickCondition condition) =>
			condition.Inverted ? !condition.OnTick() : condition.OnTick();
	}

	public static class ConditionExtensions
	{
		public static Coroutine StartCoroutine(this ICondition condition, IEnumerator routine) =>
			condition.StateMachine.StartCoroutine(routine);

		public static void StopCoroutine(this ICondition condition, Coroutine routine) =>
			condition.StateMachine.StopCoroutine(routine);

		public static bool TryGetComponent<T>(this ICondition condition, out T component)
		where T : Component
		{
			if (condition.StateMachine.TryGetComponent(out T c))
			{
				component = c;
				return true;
			}

			component = null;
			return false;
		}
	}
}