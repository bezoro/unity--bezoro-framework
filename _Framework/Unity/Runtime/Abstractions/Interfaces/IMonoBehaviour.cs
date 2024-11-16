using UnityEngine;

namespace Bezoro.Framework.Unity
{
	public interface IMonoBehaviour
	{
		// Getters
		public GameObject GameObject { get; }

		public bool IsActive { get; }

		public int Layer { get; }

		public string Tag { get; }

		public Transform Parent { get; }

		public Transform Transform { get; }

		public Vector3 Position { get; }

		public Quaternion Rotation { get; }

		public Vector3 Scale { get; }

		public bool IsInitialized { get; }

		// Constructor
		public void Initialize();

		// Setters
		public void SetName( string newName );
		public void SetActive( bool isActive );
		public void SetLayer( int layer );
		public void SetTag( string tag );
		public void SetPosition( Vector3 position );
		public void SetRotation( Quaternion rotation );
		public void SetScale( Vector3 scale );
		public void SetTransform( Vector3 position, Quaternion rotation, Vector3 scale );
		public void SetParent( Transform parent );

		// Destroy
		public void Destroy();
		public void DestroyImmediate();
	}
}
