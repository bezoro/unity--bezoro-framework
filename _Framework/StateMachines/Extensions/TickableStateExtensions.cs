using UnityEngine;

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
}