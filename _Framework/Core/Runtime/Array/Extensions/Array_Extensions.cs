namespace Bezoro.Framework.Core.Extensions
{
	public static class Array_Extensions
	{
		public static bool Is_Null<T>(this T[] array)
		{
			return array == null;
		}

		public static bool Is_Not_Null<T>(this T[] array)
		{
			return array != null;
		}

		public static bool Is_Empty<T>(this T[] array)
		{
			return array.Length == 0;
		}

		public static bool Is_Not_Empty<T>(this T[] array)
		{
			return array.Length > 0;
		}

		public static bool Equals_Null<T>(this T[] obj)
		{
			return obj == null;
		}

		public static bool Equals_Empty<T>(this T[] obj)
		{
			return obj.Length == 0;
		}

		public static bool Equals_Null_Or_Empty<T>(this T[] obj)
		{
			return Equals_Null(obj) || Equals_Empty(obj);
		}

		public static bool Is_Null_Or_Empty<T>(this T[] array)
		{
			return Is_Null(array) || Is_Empty(array);
		}

		public static bool Is_Not_Null_Or_Empty<T>(this T[] array)
		{
			return Is_Not_Null(array) || array.Is_Not_Empty();
		}
	}
}
