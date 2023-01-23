using System.Collections.Generic;

namespace Bezoro.Core.Extensions
{
	public static class ArrayExtensions
	{
		public static void Add<T>(this IList<T> list, T element) =>
			list.Add(element);
	}
}