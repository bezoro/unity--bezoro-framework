namespace Bezoro.Framework.Unity.Utilities
{
	public static class FloatUtilities
	{
		public static float Map
			( this float value, float fromMin, float fromMax, float toMin, float toMax ) =>
			( value - fromMin ) / ( fromMax - fromMin ) * ( toMax - toMin ) + toMin;

		public static bool IsWithinRange( this float value, float min, float max ) => value >= min && value <= max;
	}
}
