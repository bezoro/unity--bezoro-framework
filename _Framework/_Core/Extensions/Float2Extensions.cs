using Unity.Mathematics;

namespace Bezoro.Framework.Core.Extensions
{
	public static class Float2Extensions
	{
		public static float Magnitude(this float2 value) =>
			math.sqrt(sqrMagnitude(value));

		public static float sqrMagnitude(this float2 value) =>
			value.x * value.x + value.y * value.y;
	}
}