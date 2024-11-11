using Unity.Mathematics;

namespace Bezoro.Framework.Unity.Utilities.Helpers
{
	public static class MathHelpers
	{
		public static float CalculateFOV(float fieldOfViewWidth, float fieldOfViewDistance)
		{
			return (float)( 2.0 * math.tan(fieldOfViewWidth / 2.0 / fieldOfViewDistance) * ( 180 / math.PI ) );
		}

		public static bool IsInVisionCone
		(
			float3 visionPosition,
			float3 visionForwardVector,
			uint visionAngleDegrees,
			uint visionDistance,
			float3 toPosition
		)
		{
			var vectorToPosition = toPosition - visionPosition;
			var unitVectorToPosition = math.normalize(vectorToPosition);
			var dot = math.dot(visionForwardVector, unitVectorToPosition);
			var angleRadians = math.radians(visionAngleDegrees);
			var visionDistanceSquared = visionDistance * visionDistance;

			return dot                             > 0.0f
				&& math.abs(math.acos(dot))        < angleRadians
				&& math.lengthsq(vectorToPosition) < visionDistanceSquared;
		}

		public static float3 RandomPointOnCircle(float3 origin, float radius, uint randomSeed)
		{
			var random = new Random(randomSeed);
			var angle = random.NextFloat() * math.PI * 2;
			var x = math.cos(angle) * radius + origin.x;
			var y = origin.y;
			var z = math.sin(angle) * radius + origin.z;

			return new float3(x, y, z);
		}
	}
}
