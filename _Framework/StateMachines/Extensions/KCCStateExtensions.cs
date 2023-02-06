using UnityEngine;

namespace Bezoro.StateMachines
{
	public static class KCCStateExtensions
	{
		public static void AddMonoBehaviourToIgnoreCollisionsWith<T>(this IKCCState state)
		where T : MonoBehaviour =>
			state.MonoBehaviourTypesToIgnoreCollisionsWith.Add(typeof(T));
	}
}