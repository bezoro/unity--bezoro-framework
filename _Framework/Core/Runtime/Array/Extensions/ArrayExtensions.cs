namespace Bezoro.Framework.Core.Extensions
{
	public static class ArrayExtensions
	{
		public static bool IsNull<T>(this T[] array)
		{
			return array == null;
		}

		public static bool IsNotNull<T>(this T[] array)
		{
			return array != null;
		}

		public static bool IsEmpty<T>(this T[] array)
		{
			return array.Length == 0;
		}

		public static bool IsNotEmpty<T>(this T[] array)
		{
			return array.Length > 0;
		}

		public static bool EqualsNull<T>(this T[] obj)
		{
			return obj == null;
		}

		public static bool EqualsEmpty<T>(this T[] obj)
		{
			return obj.Length == 0;
		}

		public static bool EqualsNullOrEmpty<T>(this T[] obj)
		{
			return EqualsNull(obj) || EqualsEmpty(obj);
		}

		public static bool IsNullOrEmpty<T>(this T[] array)
		{
			return IsNull(array) || IsEmpty(array);
		}

		public static bool IsNotNullOrEmpty<T>(this T[] array)
		{
			return IsNotNull(array) || array.IsNotEmpty();
		}
	}
}
