using System;

namespace Bezoro.Core.Extensions
{
	public static class ArrayExtensions
	{
		public static bool IsNullOrEmpty<T>(this T[] array) =>
			(array == null || array.Length == 0);

		public static void Add<T>(ref T[] array, T element)
		{
			if (element == null)
				return;

			if (array.IsNullOrEmpty())
			{
				Array.Resize(ref array, 1);
				array[0] = element;
				return;
			}

			for (var i = 0; i < array.Length; i++)
			{
				if (array[i] != null)
					continue;

				array[i] = element;
				return;
			}

			var oldLength = array.Length;
			Array.Resize(ref array, array.Length * 2);
			array[oldLength] = element;
		}
	}
}