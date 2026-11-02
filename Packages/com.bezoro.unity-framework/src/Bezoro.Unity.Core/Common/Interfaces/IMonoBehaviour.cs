using UnityEngine;

namespace Bezoro.Unity.Core.Common.Interfaces;

/// <summary>
///     Exposes MonoBehaviour-style enabled state through an interface.
/// </summary>
public interface IMonoBehaviour
{
	public bool IsActive { get; }

	public bool IsInitialized { get; }

	// Getters
	public GameObject GameObject { get; }

	public int Layer { get; }

	public Quaternion Rotation { get; }

	public string Tag { get; }

	public Transform Parent { get; }

	public Transform Transform { get; }

	public Vector3 Position { get; }

	public Vector3 Scale { get; }

	// Destroy
	public void Destroy();
	public void DestroyImmediate();

	// Constructor
	public void Initialize();
	public void SetActive(bool isActive);
	public void SetLayer(int   layer);

	// Setters
	public void SetName(string         newName);
	public void SetParent(Transform    parent);
	public void SetPosition(Vector3    position);
	public void SetRotation(Quaternion rotation);
	public void SetScale(Vector3       scale);
	public void SetTag(string          tag);
	public void SetTransform(Vector3   position, Quaternion rotation, Vector3 scale);
}
