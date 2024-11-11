using UnityEngine;

namespace Bezoro.Framework.Unity
{
	public interface IBehaviour<TInterface, TBehaviour>
	where TInterface : class
	where TBehaviour : MonoBehaviour
	{
		TInterface Model { get; }
		TBehaviour Behaviour { get; }
		GameObject GameObject { get; }
	}
}
