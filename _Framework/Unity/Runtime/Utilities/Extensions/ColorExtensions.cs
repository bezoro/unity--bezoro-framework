using UnityEngine;

namespace Bezoro.Framework.Unity.Utilities.Extensions
{
	public static class ColorExtensions
	{
		public static Color ClassColor()
		{
			return new Color(1f, 0.82f, 0.23f);
		}

		public static Color StructColor()
		{
			return new Color(209, 154, 102);
		}

		public static Color MethodColor()
		{
			return new Color(0.28f, 0.71f, 1f);
		}
	}
}
