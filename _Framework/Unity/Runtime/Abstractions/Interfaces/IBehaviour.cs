using UnityEngine;

namespace Bezoro.Framework.Unity
{
	public interface IBehaviour<TInterface, TBehaviour>
	where TInterface : class
	where TBehaviour : MonoBehaviour
	{
		public TInterface Model      { get; }
		public TBehaviour Behaviour  { get; }
		public GameObject GameObject { get; }
	}
}
