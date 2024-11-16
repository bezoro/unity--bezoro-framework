using Unity.Mathematics;
using UnityEngine;

namespace Bezoro.Framework.Unity.Utilities
{
	public struct Position
	{
		public float X { get; set; }

		public float Y { get; set; }

		public float Z { get; set; }

		public static implicit operator Position( Vector3 vector ) =>
			new()
			{
				X = vector.x,
				Y = vector.y,
				Z = vector.z
			};

		public static implicit operator Position( float3 vector ) =>
			new()
			{
				X = vector.x,
				Y = vector.y,
				Z = vector.z
			};
	}
}
