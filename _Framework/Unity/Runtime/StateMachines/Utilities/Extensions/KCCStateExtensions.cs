using Bezoro.Framework.Unity.States;
using UnityEngine;

namespace Bezoro.Framework.Unity.Utilities
{
	public static class KCCStateExtensions
	{
		public static void AddMonoBehaviourToIgnoreCollisionsWith<T>( this IKCCState state )
		where T : MonoBehaviour
		{
			state.MonoBehaviourTypesToIgnoreCollisionsWith.Add( typeof(T) );
		}
	}
}
