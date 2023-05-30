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
			if (element == null)
			{
				index = -1;
				return;
			}

			if (array.IsNullOrEmpty())
			{
				Array.Resize(ref array, 1);
				array[0] = element;
				index = 0;
				return;
			}

			for (var i = 0; i < array.Length; i++)
			{
				if (array[i] != null)
					continue;

				array[i] = element;
				index = i;
				return;
			}

			var oldLength = array.Length;
			Array.Resize(ref array, array.Length * 2);
			array[oldLength] = element;
			index = oldLength;
		}

		public static void AddUnique<T>(ref T[] array, T element, out int index)
		where T : class
		{
			if (array.Contains(element, out _))
			{
				index = -1;
				return;
			}

			Add(ref array, element, out var i);
			index = i;
		}

		public static void Merge<T>(T[] from, ref T[] to)
		where T : class
		{
			if (from.IsNullOrEmpty())
				return;

			if (to.IsNullOrEmpty())
			{
				Array.Resize(ref to, from.Length);
				for (var i = 0; i < to.Length; i++)
					to[i] = from[i];

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
				if (from is UnityEngine.Object[] fromUnityObjects &&
					to is UnityEngine.Object[] toUnityObjects)
				{
					for (var i = 0; i < toUnityObjects.Length; i++)
					{
						if (toUnityObjects[i] != null)
							continue;

						toUnityObjects[i] = fromUnityObjects[index];
						index++;
						if (index > from.Length - 1)
							return;
					}
				}
				else
				{
					for (var i = 0; i < to.Length; i++)
					{
						if (to[i] != null)
							continue;

						to[i] = from[index];
						index++;
						if (index > from.Length - 1)
							return;
					}
				}
			}
		}

		public static void Remove<T>(ref T[] array, T element, out int index)
		where T : class
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

			if (array.Contains(element, out var i))
			{
				array[i] = default;
				index = i;
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
	}
}