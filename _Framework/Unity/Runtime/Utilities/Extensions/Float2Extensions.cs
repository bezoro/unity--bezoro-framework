using Unity.Mathematics;

namespace Bezoro.Framework.Unity.Utilities.Extensions
{
	public static class Float2Extensions
	{
		public static float Magnitude(this float2 value)
		{
			return math.sqrt(sqrMagnitude(value));
		}

		public static float sqrMagnitude(this float2 value)
		{
			return value.x * value.x + value.y * value.y;
		}
	}
}
