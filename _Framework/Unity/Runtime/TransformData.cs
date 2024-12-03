using UnityEngine;

namespace Bezoro.Framework.Unity
{
	public struct TransformData
	{
		public Vector3    Position { get; set; }
		public Quaternion Rotation { get; set; }
		public Vector3    Scale    { get; set; }
		public Transform  Parent   { get; set; }

		public TransformData(
			Vector3 position = default,
			Quaternion rotation = default,
			Vector3 scale = default,
			Transform parent = null
		)
		{
			Position = position == default ? Vector3.zero : position;
			Rotation = rotation == default ? Quaternion.identity : rotation;
			Scale    = scale == default ? Vector3.one : scale;
			Parent   = parent;
		}
	}
}
