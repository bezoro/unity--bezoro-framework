using Bezoro.Unity.Core.Common.Serialization;
using UnityEngine;

namespace Bezoro.Unity.Core.Common.Interfaces;

/// <summary>
///     Exposes Behaviour-style enabled state through an interface.
/// </summary>
public interface IBehaviour<T_Interface, T_Behaviour> : IBehaviour
	where T_Interface : class
	where T_Behaviour : MonoBehaviour
{
	public T_Behaviour Behaviour { get; }
	public T_Interface Model     { get; }
}

/// <summary>
///     Exposes a writable Behaviour-style enabled state.
/// </summary>
public interface IBehaviour
{
	public GameObject       GameObject { get; }
	public SerializableGUID GUID       { get; }
}
