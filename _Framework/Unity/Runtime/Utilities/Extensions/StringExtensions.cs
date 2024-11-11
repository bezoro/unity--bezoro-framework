using UnityEngine;

namespace Bezoro.Framework.Unity.Utilities.Extensions
{
	public static class StringExtensions
	{
		// Text formatting
		public static string Bold(this string text)
		{
			return $"<b>{text}</b>";
		}

		public static string Italic(this string text)
		{
			return $"<i>{text}</i>";
		}

		public static string Underline(this string text)
		{
			return $"<u>{text}</u>";
		}

		public static string Strikethrough(this string text)
		{
			return $"<s>{text}</s>";
		}

		// Size modifications
		public static string Size(this string text, int size)
		{
			return $"<size={size}>{text}</size>";
		}

		// General color application
		public static string Color(this string text, string colorName)
		{
			return $"<color={colorName}>{text}</color>";
		}

		public static string Color(this string text, Color color)
		{
			return $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{text}</color>";
		}

		// Common colors
		public static string Red(this string text)
		{
			return text.Color("red");
		}

		public static string Green(this string text)
		{
			return text.Color("green");
		}

		public static string Blue(this string text)
		{
			return text.Color("blue");
		}

		public static string Yellow(this string text)
		{
			return text.Color("yellow");
		}

		public static string Cyan(this string text)
		{
			return text.Color("cyan");
		}

		public static string Magenta(this string text)
		{
			return text.Color("magenta");
		}

		public static string White(this string text)
		{
			return text.Color("white");
		}

		public static string Black(this string text)
		{
			return text.Color("black");
		}

		public static string Gray(this string text)
		{
			return text.Color("gray");
		}

		public static string Orange(this string text)
		{
			return text.Color("orange");
		}

		public static string Purple(this string text)
		{
			return text.Color("purple");
		}

		public static string Brown(this string text)
		{
			return text.Color("brown");
		}

		// Special formatting
		public static string Uppercase(this string text)
		{
			return text.ToUpper();
		}

		public static string Lowercase(this string text)
		{
			return text.ToLower();
		}

		public static string Capitalize(this string text)
		{
			return char.ToUpper(text[0]) + text.Substring(1);
		}
	}
}
