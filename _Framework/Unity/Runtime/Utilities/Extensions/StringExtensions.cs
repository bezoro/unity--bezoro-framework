using System;
using UnityEngine;

namespace Bezoro.Framework.Unity.Utilities
{
	public static class StringExtensions
	{
		public static Color HTMLColorToRGBA( this string htmlColor )
		{
			if ( ColorUtility.TryParseHtmlString( htmlColor, out var color ) )
			{
				return color;
			}
			throw new ArgumentException( $"Invalid HTML color string: {htmlColor}" );
		}

		// Text formatting
		public static string Bold( this string text ) => $"<b>{text}</b>";

		public static string Italic( this string text ) => $"<i>{text}</i>";

		public static string Underline( this string text ) => $"<u>{text}</u>";

		public static string Strikethrough( this string text ) => $"<s>{text}</s>";

		// Size modifications
		public static string Size( this string text, int size ) => $"<size={size}>{text}</size>";

		// General color application
		public static string Color( this string text, string colorName ) => $"<color={colorName}>{text}</color>";

		public static string Color( this string text, Color color ) =>
			$"<color=#{ColorUtility.ToHtmlStringRGB( color )}>{text}</color>";

		// Common colors
		public static string Red( this string text ) => text.Color( "red" );

		public static string Green( this string text ) => text.Color( "green" );

		public static string Blue( this string text ) => text.Color( "blue" );

		public static string Yellow( this string text ) => text.Color( "yellow" );

		public static string Cyan( this string text ) => text.Color( "cyan" );

		public static string Magenta( this string text ) => text.Color( "magenta" );

		public static string White( this string text ) => text.Color( "white" );

		public static string Black( this string text ) => text.Color( "black" );

		public static string Gray( this string text ) => text.Color( "gray" );

		public static string Orange( this string text ) => text.Color( "orange" );

		public static string Purple( this string text ) => text.Color( "purple" );

		public static string Brown( this string text ) => text.Color( "brown" );

		// Special formatting
		public static string Uppercase( this string text ) => text.ToUpper();

		public static string Lowercase( this string text ) => text.ToLower();

		public static string Capitalize( this string text ) => char.ToUpper( text[0] ) + text.Substring( 1 );
	}
}
