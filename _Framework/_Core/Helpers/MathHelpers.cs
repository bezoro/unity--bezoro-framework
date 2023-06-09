using Unity.Mathematics;

namespace Bezoro.Framework.Core.Helpers
{
	public static class MathHelpers
	{
		public static float CalculateFOV(float fieldOfViewWidth, float fieldOfViewDistance)
		{
			return (float)(2.0 * math.tan(fieldOfViewWidth / 2.0 / fieldOfViewDistance) *
						   (180 / math.PI));
		}

		public static bool IsInVisionCone
		(float3 visionPosition,
		 float3 visionForwardVector,
		 uint visionAngleDegrees,
		 uint visionDistance,
		 float3 toPosition)
		{
			var vectorToPosition = toPosition - visionPosition;
			var unitVectorToPosition = math.normalize(vectorToPosition);
			var dot = math.dot(visionForwardVector, unitVectorToPosition);
			var angleRadians = math.radians(visionAngleDegrees);
			return dot > 0.0f && math.abs(math.acos(dot)) < angleRadians &&
				   math.lengthsq(vectorToPosition) < visionDistance;
		}
	}
}