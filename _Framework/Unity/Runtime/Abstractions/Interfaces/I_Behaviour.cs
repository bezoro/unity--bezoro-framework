using UnityEngine;

namespace Bezoro.Framework.Unity
{
	public interface I_Behaviour<T_Interface, T_Behaviour> : I_Behaviour
	where T_Interface : class
	where T_Behaviour : MonoBehaviour
	{
		public T_Behaviour Behaviour { get; }
		public T_Interface Model     { get; }
	}

	public interface I_Behaviour
	{
		public GameObject        GameObject { get; }
		public Serializable_GUID GUID       { get; }
	}
}
