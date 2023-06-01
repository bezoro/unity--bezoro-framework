using System;

namespace Bezoro.Framework.Core.Extensions
{
	public static class ArrayExtensions
	{
		public static bool IsNull<T>(this T[] array) =>
			array == null;

		public static bool IsNotNull<T>(this T[] array) =>
			array != null;

		public static bool IsEmpty<T>(this T[] array) =>
			array.Length == 0;

		public static bool IsNotEmpty<T>(this T[] array) =>
			array.Length > 0;

		public static bool IsNullOrEmpty<T>(this T[] array) =>
			array.IsNull() || array.IsEmpty();

		public static bool IsNotNullOrEmpty<T>(this T[] array) =>
			array.IsNotNull() || array.IsNotEmpty();

		public static bool IsUnityObject<T>(this T[] array, out T[] outUnityArray)
		{
			if (array is UnityEngine.Object[] unityArray)
			{
				outUnityArray = unityArray as T[];
				return true;
			}

			outUnityArray = null;
			return false;
		}

		public static bool IsNotUnityObject<T>(this T[] array) =>
			array is not UnityEngine.Object[];

		public static bool Contains<T>
			(this T[] array, T element, out int index, bool safetyChecks = true)
		where T : class
		{
			if (safetyChecks)
			{
				if (element.EqualsNull())
				{
					index = -1;
					return false;
				}

				if (array.IsNullOrEmpty())
				{
					index = -1;
					return false;
				}
			}

			if (array.IsUnityObject(out var unityArray) &&
				element.IsUnityObject(out var unityElement))
			{
				for (var i = 0; i < unityArray.Length; i++)
				{
					if (unityArray[i] != unityElement)
						continue;

					index = i;
					return true;
				}
			}
			else
			{
				for (var i = 0; i < array.Length; i++)
				{
					if (array[i] != element)
						continue;

					index = i;
					return true;
				}
			}

			index = -1;
			return false;
		}

		public static bool Contains<T>
			(this T[] array, T element, int validSize, out int index, bool safetyChecks = true)
		where T : class
		{
			if (safetyChecks)
			{
				if (validSize <= 0)
				{
					index = -1;
					return false;
				}

				if (element.EqualsNull())
				{
					index = -1;
					return false;
				}

				if (array.IsNullOrEmpty())
				{
					index = -1;
					return false;
				}
			}

			if (array.IsUnityObject(out var unityArray) &&
				element.IsUnityObject(out var unityElement))
			{
				for (var i = 0; i < validSize; i++)
				{
					if (unityArray[i] != unityElement)
						continue;

					index = i;
					return true;
				}
			}
			else
			{
				for (var i = 0; i < validSize; i++)
				{
					if (array[i] != element)
						continue;

					index = i;
					return true;
				}
			}

			index = -1;
			return false;
		}

		public static bool ContainsStruct<T>
			(this T[] array, T element, out int index, bool safetyChecks = true)
		where T : struct, IEquatable<T>
		{
			if (safetyChecks)
			{
				if (array.IsNullOrEmpty())
				{
					index = -1;
					return false;
				}
			}

			for (var i = 0; i < array.Length; i++)
			{
				if (!array[i].Equals(element))
					continue;

				index = i;
				return true;
			}

			index = -1;
			return false;
		}

		public static int CountEmptyIndices<T>(this T[] array, bool safetyChecks = true)
		where T : class
		{
			if (safetyChecks)
			{
				if (array.IsNullOrEmpty())
					return -1;
			}

			var emptyIndicesCount = 0;
			if (array.IsUnityObject(out var unityArray))
			{
				for (var i = 0; i < unityArray.Length; i++)
					if (unityArray[i].EqualsNull())
						emptyIndicesCount++;
			}
			else
			{
				for (var i = 0; i < array.Length; i++)
					if (array[i].EqualsNull())
						emptyIndicesCount++;
			}

			return emptyIndicesCount;
		}

		public static int CountEmptyIndices<T>
			(this T[] array, int validSize, bool safetyChecks = true)
		where T : class
		{
			if (safetyChecks)
			{
				if (validSize <= 0)
					return -1;

				if (array.IsNullOrEmpty())
					return -1;

				if (validSize > array.Length)
					return -1;
			}

			return array.Length - validSize;
		}

		public static int CountFilledIndices<T>(this T[] array, bool safetyChecks = true)
		where T : class
		{
			if (safetyChecks)
			{
				if (array.IsNullOrEmpty())
					return -1;
			}

			var filledIndicesCount = 0;
			if (array.IsUnityObject(out var unityArray))
			{
				for (var i = 0; i < unityArray.Length; i++)
					if (unityArray[i].NotEqualsNull())
						filledIndicesCount++;
			}
			else
			{
				for (var i = 0; i < array.Length; i++)
					if (array[i].NotEqualsNull())
						filledIndicesCount++;
			}

			return filledIndicesCount;
		}
	}
}