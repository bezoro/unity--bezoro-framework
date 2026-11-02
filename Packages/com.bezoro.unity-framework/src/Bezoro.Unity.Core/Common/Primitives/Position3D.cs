using System;
using Unity.Mathematics;
using UnityEngine;

namespace Bezoro.Unity.Core.Common.Primitives;

/// <summary>
///     Translates Unity's Vector3/float3 to x, y, z and back
/// </summary>
public readonly struct Position3D
{
	public Position3D(float x, float y, float z)
	{
		X = x;
		Y = y;
		Z = z;
	}

	public float X { get; }
	public float Y { get; }
	public float Z { get; }

	public static bool operator ==(Position3D left, Position3D right) =>
		left.Equals(
			right
		);

	public static implicit operator float3(Position3D position3D) =>
		new(
			position3D.X,
			position3D.Y,
			position3D.Z
		);

	public static implicit operator Position3D(Vector3 vector) =>
		new(
			vector.x,
			vector.y,
			vector.z
		);

	public static implicit operator Position3D(float3 vector) =>
		new(
			vector.x,
			vector.y,
			vector.z
		);

	public static implicit operator Vector3(Position3D position3D) =>
		new(
			position3D.X,
			position3D.Y,
			position3D.Z
		);

	public static bool operator !=(Position3D left, Position3D right) => !left.Equals(right);

	#region Equality

	public override bool Equals(object? obj) =>
		obj is Position3D position && Equals(position);

	public bool Equals(Position3D other) =>
		X == other.X && Y == other.Y && Z == other.Z;

	public override int GetHashCode() => HashCode.Combine(X, Y, Z);

	#endregion
}
