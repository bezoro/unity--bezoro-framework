using System;
using UnityEngine;
using Bezoro.Core.Extensions;

namespace Bezoro.Core.Helpers
{
	public static class ArrayHelpers
	{
		public static void Add<T>(ref T[] array, T element)
		where T : class
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

		public static void Merge<T>(T[] from, ref T[] to)
		where T : class
		{
			if (from.IsNullOrEmpty())
				return;

			if (to.IsNullOrEmpty())
			{
				ResizeAndFill(from, ref to);
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

		public static void Merge(GameObject[] from, ref GameObject[] to)
		{
			if (from.IsNullOrEmpty())
				return;

			if (to.IsNullOrEmpty())
			{
				ResizeAndFill(from, ref to);
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

			void MergeArrays(GameObject[] from, ref GameObject[] to)
			{
				var index = 0;
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

		public static void Remove<T>(ref T[] array, T element)
		where T : class
		{
			if (element == null)
				return;

			if (array.IsNullOrEmpty())
				return;

			for (var i = 0; i < array.Length; i++)
				if (array[i] == element)
				{
					array[i] = default;
					return;
				}
		}

		public static void Clear<T>(ref T[] array)
		where T : class
		{
			if (array.IsNullOrEmpty())
				return;

			for (var i = 0; i < array.Length; i++)
				array[i] = default;
		}

		private static void ResizeAndFill<T>(T[] from, ref T[] to)
		where T : class
		{
			Array.Resize(ref to, from.Length);
			for (var i = 0; i < to.Length; i++)
				to[i] = from[i];
		}
	}
}