using UnityEngine;

namespace Bezoro.Framework.Unity.Utilities
{
	public static class Vector2_Extensions
	{
		private const float _UNITY_ROTATION_ANGLE_OFFSET = 90;
		private const float _HALF_CIRCLE_DEGREES         = 180;
		private const float _CIRCLE_DEGREES              = 360;

		public static float Smallest_Delta_Angle( this Vector2 source, Vector2 target )
		{
			var current_angle = source.To_Angle();
			var target_angle  = target.To_Angle();
			var delta_angle   = current_angle - target_angle;

			if ( Mathf.Abs( delta_angle ) > _HALF_CIRCLE_DEGREES )
				delta_angle = Mathf.Sign( delta_angle ) * ( Mathf.Abs( delta_angle ) - _CIRCLE_DEGREES );

			return delta_angle;
		}

		public static float To_Angle( this Vector2 vector )
		{
			return ( _CIRCLE_DEGREES - _UNITY_ROTATION_ANGLE_OFFSET +
					 Mathf.Atan2( vector.y, vector.x ) * Mathf.Rad2Deg ) % _CIRCLE_DEGREES;
		}

		public static Vector2 From_Angle( this Vector2 vector, float angle )
		{
			var radians   = ( angle + _UNITY_ROTATION_ANGLE_OFFSET ) * Mathf.Deg2Rad;
			var magnitude = vector.magnitude;
			vector.x =  Mathf.Cos( radians );
			vector.y =  Mathf.Sin( radians );
			vector   *= magnitude;
			return vector;
		}
	}
}
