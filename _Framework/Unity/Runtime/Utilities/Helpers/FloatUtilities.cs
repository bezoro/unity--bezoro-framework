namespace Bezoro.Framework.Unity.Utilities.Helpers
{
	public static class FloatUtilities
	{
		public static float Map(this float value, float fromMin, float fromMax, float toMin, float toMax)
		{
			return ( value - fromMin ) / ( fromMax - fromMin ) * ( toMax - toMin ) + toMin;
		}

		public static bool IsWithinRange(this float value, float min, float max)
		{
			return value >= min && value <= max;
		}
	}
}
