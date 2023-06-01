using System;
using Bezoro.Framework.Core.Extensions;

namespace Bezoro.Framework.Core.Helpers
{
	public static class ArrayHelpers
	{
		public static void Add<T>(ref T[] array, T element)
		where T : class =>
			Add(ref array, element, out _);

		public static void Add<T>(ref T[] array, T element, out int index)
		where T : class
		{
			if (element.EqualsNull())
			{
				index = -1;
				return;
			}

			if (array.IsNullOrEmpty())
			{
				CreateArrayAndAddFirstElement(ref array, element, false);
				index = 0;
				return;
			}

			for (var i = 0; i < array.Length; i++)
			{
				if (array[i].NotEqualsNull())
					continue;

				array[i] = element;
				index = i;
				return;
			}

			ResizeArrayAndAddElement(ref array, element, 2, out var oldLength);
			index = oldLength;
		}

		private static void ResizeArrayAndAddElement<T>
			(ref T[] array, T element, int factor, out int oldLength)
		{
			oldLength = array.Length;
			MultiplySize(ref array, factor);
			array[oldLength] = element;
		}

		public static void Add<T>(ref T[] array, T element, ref int validSize, out int index)
		where T : class
		{
			if (validSize < 0)
			{
				index = -1;
				return;
			}

			if (element.IsNull())
			{
				index = -1;
				return;
			}

			if (array.IsNullOrEmpty())
			{
				CreateArrayAndAddFirstElement(ref array, element);
				validSize = 1;
				index = 0;
				return;
			}

			if (validSize < array.Length)
			{
				array[validSize] = element;
				validSize++;
				index = validSize;
				return;
			}

			ResizeArrayAndAddElement(ref array, element, 2, ref validSize, out var oldLength);
			index = oldLength;
		}

		private static void ResizeArrayAndAddElement<T>
			(ref T[] array, T element, int factor, ref int validSize, out int oldLength)
		{
			ResizeArrayAndAddElement(ref array, element, factor, out oldLength);
			validSize++;
		}

		public static void MultiplySize<T>(ref T[] array, int factor) =>
			Array.Resize(ref array, array.Length * factor);

		public static void AddUnique<T>(ref T[] array, T element, out int index)
		where T : class
		{
			if (element.EqualsNull())
			{
				index = -1;
				return;
			}

			if (array.IsNullOrEmpty())
			{
				CreateArrayAndAddFirstElement(ref array, element);
				index = 0;
				return;
			}

			if (array.Contains(element, out _))
			{
				index = -1;
				return;
			}

			Add(ref array, element, out var i);
			index = i;
		}

		public static void AddUnique<T>(ref T[] array, T element, ref int validSize, out int index)
		where T : class
		{
			if (element.EqualsNull())
			{
				index = -1;
				return;
			}

			if (array.IsNullOrEmpty())
			{
				CreateArrayAndAddFirstElement(ref array, element, false);
				validSize = 1;
				index = 0;
				return;
			}

			if (array.Contains(element, validSize, out _))
			{
				index = -1;
				return;
			}

			Add(ref array, element, ref validSize, out var i);
			index = i;
		}

		public static void CreateArrayAndAddFirstElement<T>
			(ref T[] array, T element, bool safetyChecks = true)
		{
			if (safetyChecks)
			{
				if (element.EqualsNull())
					return;

				if (array.IsNotNullOrEmpty())
					return;
			}

			Array.Resize(ref array, 1);
			array[0] = element;
		}

		public static void Merge<T>(T[] from, ref T[] to)
		where T : class
		{
			if (from.IsNullOrEmpty())
				return;

			if (to.IsNullOrEmpty())
			{
				Array.Resize(ref to, from.Length);
				Copy(from, ref to, false);
				return;
			}

			if (to.Length < from.Length)
			{
				Array.Resize(ref to, from.Length);
				Copy(from, ref to, false);
				return;
			}

			var availableIndices = to.CountEmptyIndices();
			if (availableIndices >= from.Length)
			{
				MergeArrays(from, ref to);
				return;
			}

			var occupiedIndices = to.Length - availableIndices;
			const int newLengthMultiplier = 2;
			var newLength = to.Length * newLengthMultiplier;
			var newAvailableIndices = newLength - occupiedIndices;

			while (newAvailableIndices < from.Length)
				newLength *= newLengthMultiplier;

			Array.Resize(ref to, newLength);
			MergeArrays(from, ref to);

			void MergeArrays(T[] from, ref T[] to)
			{
				var index = 0;
				if (from.IsUnityObject(out var fromUnityArray) &&
					to.IsUnityObject(out var toUnityArray))
				{
					for (var i = 0; i < toUnityArray.Length; i++)
					{
						if (toUnityArray[i].NotEqualsNull())
							continue;

						toUnityArray[i] = fromUnityArray[index];
						index++;
						if (index > from.Length - 1)
							return;
					}
				}
				else
				{
					for (var i = 0; i < to.Length; i++)
					{
						if (to[i].NotEqualsNull())
							continue;

						to[i] = from[index];
						index++;
						if (index > from.Length - 1)
							return;
					}
				}
			}
		}

		public static void Copy<T>(T[] from, ref T[] to, bool safetyChecks = true)
		{
			if (safetyChecks)
			{
				if (from.IsNullOrEmpty())
					return;

				if (to.IsNullOrEmpty())
					Array.Resize(ref to, from.Length);

				if (to.Length < from.Length)
					Array.Resize(ref to, from.Length);
			}

			for (var i = 0; i < to.Length; i++)
				to[i] = from[i];
		}

		public static void Remove<T>
			(ref T[] array, T element, out int index, bool safetyChecks = true)
		where T : class
		{
			if (safetyChecks)
			{
				if (element == null)
				{
					index = -1;
					return;
				}

				if (array.IsNullOrEmpty())
				{
					index = -1;
					return;
				}
			}

			if (array.Contains(element, out var i))
			{
				array[i] = default;
				index = i;
				return;
			}

			index = -1;
		}

		public static void Remove<T>
			(ref T[] array, T element, ref int validSize, out int index, bool safetyChecks = true)
		where T : class
		{
			if (safetyChecks)
			{
				if (validSize <= 0)
				{
					index = -1;
					return;
				}

				if (element.EqualsNull())
				{
					index = -1;
					return;
				}

				if (array.IsNullOrEmpty())
				{
					index = -1;
					return;
				}
			}

			if (array.Contains(element, validSize, out var i, false))
			{
				if (i == validSize - 1)
				{
					array[i] = default;
					validSize = i;
					index = i;
					return;
				}

				array[i] = array[validSize];
				array[validSize] = default;
				validSize--;

				index = i;
				return;
			}

			index = -1;
		}

		public static void Clear<T>(ref T[] array)
		where T : class
		{
			if (array.IsNullOrEmpty())
				return;

			for (var i = 0; i < array.Length; i++)
				array[i] = default;
		}

		public static void Clear<T>(ref T[] array, int validSize)
		where T : class
		{
			if (validSize <= 0)
				return;

			if (array.IsNullOrEmpty())
				return;

			for (var i = 0; i < validSize; i++)
				array[i] = default;
		}
	}
}