using UnityEngine;

namespace Bezoro.StateMachines
{
	public static class KCCStateExtensions
	{
		public static void AddMonoBehaviourToIgnoreCollisionsWith<T>(this IKCCState state)
		where T : MonoBehaviour
		{
			var type = typeof(T);
			state.MonoBehaviourTypesToIgnoreCollisionsWith.Add(type);
		}
	}
}