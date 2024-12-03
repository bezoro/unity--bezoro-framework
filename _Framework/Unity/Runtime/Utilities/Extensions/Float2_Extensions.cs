using Unity.Mathematics;

namespace Bezoro.Framework.Unity.Utilities
{
	public static class Float2_Extensions
	{
		public static float Magnitude( this float2 value )
		{
			return math.sqrt( Square_Magnitude( value ) );
		}

		public static float Square_Magnitude( this float2 value )
		{
			return value.x * value.x + value.y * value.y;
		}
	}
}
