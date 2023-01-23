using System;
using Bezoro.Core.Extensions;

namespace Bezoro.Core.Helpers
{
	public static class ArrayHelpers
	{
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

		public static void Remove<T>(ref T[] array, T element)
		where T : class
		{
			if (array.IsNullOrEmpty())
				return;

			if (element == null)
				return;

			for (var i = 0; i < array.Length; i++)
				if (array[i] == element)
				{
					array[i] = null;
					return;
				}
		}

		public static void Clear<T>(ref T[] array)
		where T : class
		{
			if (array.IsNullOrEmpty())
				return;

			for (var i = 0; i < array.Length; i++)
				array[i] = null;
		}
	}
}