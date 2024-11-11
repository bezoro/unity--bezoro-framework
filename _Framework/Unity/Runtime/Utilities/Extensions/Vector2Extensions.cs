using UnityEngine;

namespace Bezoro.Framework.Unity.Utilities.Extensions
{
	public static class Vector2Extensions
	{
		private const float UnityRotationAngleOffset = 90;
		private const float HalfCircleDegrees = 180;
		private const float CircleDegrees = 360;

		public static float SmallestDeltaAngle(this Vector2 source, Vector2 target)
		{
			var currentAngle = source.ToAngle();
			var targetAngle = target.ToAngle();
			var deltaAngle = currentAngle - targetAngle;
			if (Mathf.Abs(deltaAngle) > HalfCircleDegrees)
			{
				deltaAngle = Mathf.Sign(deltaAngle) * ( Mathf.Abs(deltaAngle) - CircleDegrees );
			}

			return deltaAngle;
		}

		public static float ToAngle(this Vector2 vector)
		{
			return ( CircleDegrees - UnityRotationAngleOffset + Mathf.Atan2(vector.y, vector.x) * Mathf.Rad2Deg )
				 % CircleDegrees;
		}

		public static Vector2 FromAngle(this Vector2 vector, float angle)
		{
			var radians = ( angle + UnityRotationAngleOffset ) * Mathf.Deg2Rad;
			var magnitude = vector.magnitude;
			vector.x = Mathf.Cos(radians);
			vector.y = Mathf.Sin(radians);
			vector *= magnitude;
			return vector;
		}
	}
}
