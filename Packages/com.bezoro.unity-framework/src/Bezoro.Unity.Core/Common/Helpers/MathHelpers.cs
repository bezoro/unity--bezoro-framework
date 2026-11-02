using Unity.Mathematics;

namespace Bezoro.Unity.Core.Common.Helpers;

/// <summary>
///     Provides shared numeric and geometry calculations.
/// </summary>
public static class MathHelpers
{
	public static bool IsInVisionCone(
		float3 visionPosition,
		float3 visionForwardVector,
		uint   visionAngleDegrees,
		uint   visionDistance,
		float3 toPosition
	)
	{
		var vectorToPosition = toPosition - visionPosition;
		var unitVectorToPosition = math.normalize(
			vectorToPosition
		);

		float dot = math.dot(
			visionForwardVector,
			unitVectorToPosition
		);

		float angleRadians = math.radians(
			visionAngleDegrees
		);

		uint visionDistanceSquared = visionDistance * visionDistance;

		return dot > 0.0f &&
			   math.abs(
				   math.acos(
					   dot
				   )
			   ) <
			   angleRadians &&
			   math.lengthsq(
				   vectorToPosition
			   ) <
			   visionDistanceSquared;
	}

	public static float CalculateFOV(float fieldOfViewWidth, float fieldOfViewDistance) =>
		(float)(2.0 *
				math.tan(
					fieldOfViewWidth / 2.0 / fieldOfViewDistance
				) *
				(180 / math.PI));

	public static float3 RandomPointOnCircle(float3 origin, float radius, uint randomSeed)
	{
		var random = new Random(
			randomSeed
		);

		float angle = random.NextFloat() * math.PI * 2;
		float x = math.cos(
					  angle
				  ) *
				  radius +
				  origin.x;

		float y = origin.y;
		float z = math.sin(
					  angle
				  ) *
				  radius +
				  origin.z;

		return new(
			x,
			y,
			z
		);
	}
}
