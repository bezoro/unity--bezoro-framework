using Bezoro.Unity.Core.StateMachines.KCCBased;
using UnityEngine;

namespace Bezoro.Unity.Core.StateMachines.Utilities;

/// <summary>
///     Contains extension methods for KCC State.
/// </summary>
public static class KCCStateExtensions
{
	public static void AddMonoBehaviourToIgnoreCollisionsWith<T>(this IKCCState state)
		where T : MonoBehaviour =>
		state.MonoBehaviourTypesToIgnoreCollisionsWith.Add(
			typeof(T)
		);
}
