using Unity.Mathematics;

namespace Bezoro.Unity.Core.Common.Extensions;

/// <summary>
///     Contains extension methods for Float2.
/// </summary>
public static class Float2Extensions
{
	public static float Magnitude(this float2 value) =>
		math.sqrt(
			value.Square_Magnitude()
		);

	public static float Square_Magnitude(this float2 value) =>
		value.x * value.x + value.y * value.y;
}
