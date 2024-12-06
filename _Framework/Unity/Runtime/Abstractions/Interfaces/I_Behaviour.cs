using UnityEngine;

namespace Bezoro.Framework.Unity
{
	public interface I_Behaviour<T_Interface, T_Behaviour>
	where T_Interface : class
	where T_Behaviour : MonoBehaviour
	{
		public T_Interface Model      { get; }
		public T_Behaviour Behaviour  { get; }
		public GameObject  GameObject { get; }
	}
}
