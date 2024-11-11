using System;
using UnityEngine;

namespace Bezoro.Framework.Unity.Utilities.Extensions
{
	public static class EnumExtensions
	{
		public static bool IsDefined<T>(this T value) where T : Enum
		{
			return Enum.IsDefined(typeof(T), value);
		}

		// Text formatting
		public static string Bold(this Enum value)
		{
			return value.ToString().Bold();
		}

		public static string Italic(this Enum value)
		{
			return value.ToString().Italic();
		}

		public static string Underline(this Enum value)
		{
			return value.ToString().Underline();
		}

		public static string Strikethrough(this Enum value)
		{
			return value.ToString().Strikethrough();
		}

		// Size modifications
		public static string Size(this Enum value, int size)
		{
			return value.ToString().Size(size);
		}

		// Color applications
		public static string Color(this Enum value, string colorName)
		{
			return value.ToString().Color(colorName);
		}

		public static string Color(this Enum value, Color color)
		{
			return value.ToString().Color(color);
		}

		// Specific colors
		public static string Red(this Enum value)
		{
			return value.ToString().Red();
		}

		public static string Green(this Enum value)
		{
			return value.ToString().Green();
		}

		public static string Blue(this Enum value)
		{
			return value.ToString().Blue();
		}

		public static string Yellow(this Enum value)
		{
			return value.ToString().Yellow();
		}

		public static string Cyan(this Enum value)
		{
			return value.ToString().Cyan();
		}

		public static string Magenta(this Enum value)
		{
			return value.ToString().Magenta();
		}

		public static string White(this Enum value)
		{
			return value.ToString().White();
		}

		public static string Black(this Enum value)
		{
			return value.ToString().Black();
		}

		public static string Gray(this Enum value)
		{
			return value.ToString().Gray();
		}

		public static string Orange(this Enum value)
		{
			return value.ToString().Orange();
		}

		public static string Purple(this Enum value)
		{
			return value.ToString().Purple();
		}

		public static string Brown(this Enum value)
		{
			return value.ToString().Brown();
		}

		// Case modifications
		public static string Uppercase(this Enum value)
		{
			return value.ToString().Uppercase();
		}

		public static string Lowercase(this Enum value)
		{
			return value.ToString().Lowercase();
		}

		public static string Capitalize(this Enum value)
		{
			return value.ToString().Capitalize();
		}
	}
}
