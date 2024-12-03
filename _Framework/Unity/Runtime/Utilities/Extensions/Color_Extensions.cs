using UnityEngine;

namespace Bezoro.Framework.Unity.Utilities
{
	public static class Color_Extensions
	{
		// Background and Default Text
		public static Color Background_Color()
		{
			return new( 0.12f, 0.12f, 0.13f );
		}

		public static Color Default_Text_Color()
		{
			return new( 0.85f, 0.85f, 0.85f );
		}

		// Semantic Colors
		public static Color Class_Color()
		{
			return new( 1.0f, 0.87f, 0.33f );
		}

		public static Color Struct_Color()
		{
			return new( 0.85f, 0.58f, 0.48f );
		}

		public static Color Method_Color()
		{
			return new( 0.35f, 0.65f, 0.93f );
		}

		public static Color Field_Color()
		{
			return new( 0.63f, 0.85f, 1.0f );
		}

		public static Color Property_Color()
		{
			return new( 0.81f, 0.56f, 0.77f );
		}

		public static Color Event_Color()
		{
			return new( 0.88f, 0.78f, 0.50f );
		}

		public static Color Delegate_Color()
		{
			return new( 0.91f, 0.48f, 0.48f );
		}

		public static Color Enum_Color()
		{
			return new( 0.86f, 0.55f, 0.30f );
		}

		public static Color Keyword_Color()
		{
			return new( 0.82f, 0.53f, 0.90f );
		}

		public static Color Interface_Color()
		{
			return new( 0.32f, 0.77f, 0.77f );
		}

		public static Color Namespace_Color()
		{
			return new( 0.28f, 0.58f, 0.54f );
		}

		// Extras (Strings, Numbers, Comments)
		public static Color Comment_Color()
		{
			return new( 0.5f, 0.45f, 0.4f );
		}

		public static Color String_Color()
		{
			return new( 0.56f, 0.75f, 0.31f );
		}

		public static Color Number_Color()
		{
			return new( 0.77f, 0.88f, 0.73f );
		}
	}
}
