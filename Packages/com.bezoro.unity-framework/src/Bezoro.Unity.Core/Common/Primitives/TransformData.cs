using UnityEngine;

namespace Bezoro.Unity.Core.Common.Primitives;

/// <summary>
///     Represents Transform Data data.
/// </summary>
public struct TransformData
{
	public TransformData(Transform transform)
	{
		Position = transform.position;
		Rotation = transform.rotation;
		Scale    = transform.localScale;
		Parent   = transform.parent;
	}

	public Quaternion Rotation { get; }
	public Transform  Parent   { get; }
	public Vector3    Position { get; }
	public Vector3    Scale    { get; }
}
