using UnityEngine;

namespace Bezoro.Framework.Unity
{
	public struct Transform_Data
	{
		public Vector3    Position { get; }
		public Quaternion Rotation { get; }
		public Vector3    Scale    { get; }
		// public Transform  Parent   { get; }

		public Transform_Data(
			Vector3 position = default,
			Quaternion rotation = default,
			Vector3 scale = default
			// Transform parent = null
		)
		{
			Position = position == default ? Vector3.zero : position;
			Rotation = rotation == default ? Quaternion.identity : rotation;
			Scale    = scale == default ? Vector3.one : scale;
			// Parent   = parent;
		}
	}
}
