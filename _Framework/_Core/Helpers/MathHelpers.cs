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
	}
}