using UnityEngine;

namespace Bezoro.Framework.Unity.Utilities
{
	public static class ColorExtensions
	{
		public static Color ClassColor() => new(1f, 0.82f, 0.23f);

		public static Color StructColor() => new(209, 154, 102);

		public static Color MethodColor() => new(0.28f, 0.71f, 1f);
	}
}
