using System.Collections;
using System.Collections.Generic;
using Sirenix.Utilities;

namespace Bezoro.Core.Extensions
{
	public static class ArrayExtensions
	{
		public static void Add<T>(this IList<T> list, T element) =>
			list.Add(element);

		public static void AddUnique<T>(this IList<T> list, T element)
		{
			if (list.Contains(element))
				return;
			
			list.Add(element);
		}

		public static void Remove<T>(this IList<T> list, T element) =>
			list.Remove(element);

		public static void RemoveAt<T>(this IList<T> list, int index) =>
			list.RemoveAt(index);
	}
}