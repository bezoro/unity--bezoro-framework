using UnityEngine;

namespace Bezoro.Framework.Core.Extensions
{
	public static class ArrayExtensions
	{
		public static bool IsNullOrEmpty<T>(this T[] array) =>
			(array == null || array.Length == 0);

		public static int CountEmptyIndices<T>(this T[] array)
		where T : class
		{
			var numberOfEmptyIndices = 0;
			if (array is UnityEngine.Object[] unityObjects)
			{
				for (var i = 0; i < unityObjects.Length; i++)
					if (unityObjects[i] == null)
						numberOfEmptyIndices++;
			}
			else
			{
				for (var i = 0; i < array.Length; i++)
					if (array[i] == null)
						numberOfEmptyIndices++;
			}

			return numberOfEmptyIndices;
		}

		public static int CountFilledIndices<T>(this T[] array)
		where T : class
		{
			var numberOfFilledIndices = 0;
			if (array is Object[] unityObjects)
			{
				for (var i = 0; i < unityObjects.Length; i++)
					if (unityObjects[i] != null)
						numberOfFilledIndices++;
			}
			else
			{
				for (var i = 0; i < array.Length; i++)
					if (array[i] != null)
						numberOfFilledIndices++;
			}

			return numberOfFilledIndices;
		}
	}
}