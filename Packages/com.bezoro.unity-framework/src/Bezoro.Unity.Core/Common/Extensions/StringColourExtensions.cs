using Bezoro.Core.Extensions;
using Bezoro.Core.Types;
using UColor = UnityEngine.Color;

namespace Bezoro.Unity.Core.Common.Extensions
{
	/// <summary>
	///     Contains extension methods for String Colour.
	/// </summary>
	public static class StringColourExtensions
	{
		public static string Color(this string value, UColor color)
		{
			var c = new Color(
				color.r,
				color.g,
				color.b,
				color.a
			);

			return value.Color(
				c
			);
		}
	}
}
