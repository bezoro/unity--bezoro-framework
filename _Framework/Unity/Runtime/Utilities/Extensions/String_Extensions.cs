using System;
using UnityEngine;

namespace Bezoro.Framework.Unity.Utilities
{
	public static class String_Extensions
	{
	#region Text Formatting

		public static string Bold( this string text )
		{
			return $"<b>{text}</b>";
		}

		public static string Italic( this string text )
		{
			return $"<i>{text}</i>";
		}

		public static string Underline( this string text )
		{
			return $"<u>{text}</u>";
		}

		public static string Strikethrough( this string text )
		{
			return $"<s>{text}</s>";
		}

		public static string Uppercase( this string text )
		{
			return text.ToUpper();
		}

		public static string Lowercase( this string text )
		{
			return text.ToLower();
		}

		public static string Capitalize( this string text )
		{
			return char.ToUpper( text[ 0 ] ) + text[ 1.. ];
		}

		public static string Size( this string text, int size )
		{
			return $"<size={size}>{text}</size>";
		}

	#endregion

	#region Colour

		public static Color HTML_Color_To_RGBA( this string html_color )
		{
			if ( ColorUtility.TryParseHtmlString( html_color, out var color ) )
				return color;

			throw new ArgumentException( $"Invalid HTML color string: {html_color}" );
		}

		public static string Color( this string text, string colorName )
		{
			return $"<color={colorName}>{text}</color>";
		}

		public static string Color( this string text, Color color )
		{
			return $"<color=#{ColorUtility.ToHtmlStringRGB( color )}>{text}</color>";
		}

		public static string Red( this string text )
		{
			return text.Color( "red" );
		}

		public static string Green( this string text )
		{
			return text.Color( "green" );
		}

		public static string Blue( this string text )
		{
			return text.Color( "blue" );
		}

		public static string Yellow( this string text )
		{
			return text.Color( "yellow" );
		}

		public static string Cyan( this string text )
		{
			return text.Color( "cyan" );
		}

		public static string Magenta( this string text )
		{
			return text.Color( "magenta" );
		}

		public static string White( this string text )
		{
			return text.Color( "white" );
		}

		public static string Black( this string text )
		{
			return text.Color( "black" );
		}

		public static string Gray( this string text )
		{
			return text.Color( "gray" );
		}

		public static string Orange( this string text )
		{
			return text.Color( "orange" );
		}

		public static string Purple( this string text )
		{
			return text.Color( "purple" );
		}

		public static string Brown( this string text )
		{
			return text.Color( "brown" );
		}

	#endregion
	}
}
