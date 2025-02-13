namespace Bezoro.Framework.Unity.Utilities.String
{
	public static class Formatting
	{
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
			return char.ToUpper(text[0]) + text[1..];
		}

		public static string Size(this string text, int size)
		{
			return $"<size={size}>{text}</size>";
		}
	}
}
