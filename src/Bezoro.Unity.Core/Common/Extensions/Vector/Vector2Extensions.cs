using Bezoro.Unity.Core.Common.Consts;
using UnityEngine;

namespace Bezoro.Unity.Core.Common.Extensions.Vector;

/// <summary>
///     Contains extension methods for Vector2.
/// </summary>
public static class Vector2Extensions
{
	public static float SmallestDeltaAngle(this Vector2 source, Vector2 target)
	{
		float currentAngle = source.ToAngle();
		float targetAngle  = target.ToAngle();
		float deltaAngle   = currentAngle - targetAngle;

		if (Mathf.Abs(
				deltaAngle
			) >
			AngleConstants.HALF_CIRCLE_DEGREES)
			deltaAngle = Mathf.Sign(
							 deltaAngle
						 ) *
						 (Mathf.Abs(
							  deltaAngle
						  ) -
						  AngleConstants.CIRCLE_DEGREES);

		return deltaAngle;
	}

	public static float ToAngle(this Vector2 vector) =>
		(AngleConstants.CIRCLE_DEGREES -
		 AngleConstants.UNITY_ROTATION_ANGLE_OFFSET +
		 Mathf.Atan2(
			 vector.y,
			 vector.x
		 ) *
		 Mathf.Rad2Deg) %
		AngleConstants.CIRCLE_DEGREES;

	public static Vector2 FromAngle(this Vector2 vector, float angle)
	{
		float radians   = (angle + AngleConstants.UNITY_ROTATION_ANGLE_OFFSET) * Mathf.Deg2Rad;
		float magnitude = vector.magnitude;
		vector.x = Mathf.Cos(
			radians
		);

		vector.y = Mathf.Sin(
			radians
		);

		vector *= magnitude;
		return vector;
	}
}
