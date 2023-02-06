using UnityEngine;

namespace Bezoro.Core.Extensions
{
	public static class ArrayExtensions
	{
		public static bool IsNullOrEmpty<T>(this T[] array) =>
			(array == null || array.Length == 0);

		public static int CountEmptyIndices<T>(this T[] array)
		where T : class
		{
			var numberOfEmptyIndices = 0;
			for (var i = 0; i < array.Length; i++)
				if (array[i] == null)
					numberOfEmptyIndices++;

			return numberOfEmptyIndices;
		}

		public static int CountEmptyIndices(this GameObject[] array)
		{
			var numberOfEmptyIndices = 0;
			for (var i = 0; i < array.Length; i++)
				if (array[i] == null)
					numberOfEmptyIndices++;

			return numberOfEmptyIndices;
		}

		public static int CountFilledIndices<T>(this T[] array)
		where T : class
		{
			var numberOfFilledIndices = 0;
			for (var i = 0; i < array.Length; i++)
			{
				if (array[i] != null)
					numberOfFilledIndices++;
			}

			return numberOfFilledIndices;
		}

		public static int CountFilledIndices(this GameObject[] array)
		{
			var numberOfFilledIndices = 0;
			for (var i = 0; i < array.Length; i++)
			{
				if (array[i] != null)
					numberOfFilledIndices++;
			}

			return numberOfFilledIndices;
		}
	}
}