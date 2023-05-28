using UnityEngine;

namespace Bezoro.Framework.StateMachines
{
	public static class KCCStateExtensions
	{
		public static void AddMonoBehaviourToIgnoreCollisionsWith<T>(this IKCCState state)
		where T : MonoBehaviour =>
			state.MonoBehaviourTypesToIgnoreCollisionsWith.Add(typeof(T));
	}
}