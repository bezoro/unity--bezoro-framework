using System;
using System.Diagnostics;
using UnityEngine;

namespace Bezoro.Unity.Core.Common.Helpers;

/// <summary>
///     Performs shared vector calculations and conversions.
/// </summary>
public static class VectorHelpers
{
	public static float AngleBetween(Vector3 from, Vector3 to) =>
		Vector3.Angle(
			from,
			to
		);

	public static Vector3 Average(params Vector3[] vectors)
	{
		if (vectors == null || vectors.Length == 0)
			throw new ArgumentException("At least one vector is required.", nameof(vectors));

		var sum = Vector3.zero;

		foreach (var vec in vectors) sum += vec;

		return sum / vectors.Length;
	}

	public static Vector3 CalculateCellCenter(
		int     columnIndex,
		int     rowIndex,
		Vector2 cellDimensions,
		Bounds  boundingBox
	)
	{
		IntHelpers.ValidateIntegerInRange(
			columnIndex,
			0,
			(int)boundingBox.size.x
		);

		IntHelpers.ValidateIntegerInRange(
			rowIndex,
			0,
			(int)boundingBox.size.y
		);

		ValidateVector2(
			cellDimensions
		);

		ValidateVector2(
			boundingBox.size
		);

		var   minimumBounds = boundingBox.min;
		float centerX       = (columnIndex + 0.5f) * cellDimensions.x;
		float centerY       = boundingBox.size.y;
		float centerZ       = (rowIndex + 0.5f) * cellDimensions.y;

		var cellCenter = minimumBounds +
						 new Vector3(
							 centerX,
							 centerY,
							 centerZ
						 );

		return cellCenter;
	}

	public static Vector3 ClampComponents(this Vector3 vector, Vector3 min, Vector3 max) =>
		new(
			Mathf.Clamp(
				vector.x,
				min.x,
				max.x
			),
			Mathf.Clamp(
				vector.y,
				min.y,
				max.y
			),
			Mathf.Clamp(
				vector.z,
				min.z,
				max.z
			)
		);

	public static Vector3 DivideComponents(this Vector3 vector, Vector3 other) =>
		new(
			vector.x / other.x,
			vector.y / other.y,
			vector.z / other.z
		);

	public static Vector3 MultiplyComponents(this Vector3 vector, Vector3 other) =>
		new(
			vector.x * other.x,
			vector.y * other.y,
			vector.z * other.z
		);

	public static Vector3 Perpendicular(Vector3 vector) =>
		Vector3.Cross(
			vector,
			Vector3.up
		);

	public static Vector3 RotateAroundPivot(this Vector3 point, Vector3 pivot, Vector3 angles) =>
		Quaternion.Euler(
			angles
		) *
		(point - pivot) +
		pivot;

	[Conditional(
		"UNITY_EDITOR"
	)]
	public static void ValidateVector2(Vector2 vector)
	{
		if (float.IsNaN(
				vector.x
			) ||
			float.IsNaN(
				vector.y
			))
			throw new ArgumentException(
				"Vector contains NaN values"
			);
	}

	[Conditional(
		"UNITY_EDITOR"
	)]
	public static void ValidateVector3(Vector3 vector)
	{
		if (float.IsNaN(
				vector.x
			) ||
			float.IsNaN(
				vector.y
			) ||
			float.IsNaN(
				vector.z
			))
			throw new ArgumentException(
				"Vector contains NaN values"
			);
	}
}
