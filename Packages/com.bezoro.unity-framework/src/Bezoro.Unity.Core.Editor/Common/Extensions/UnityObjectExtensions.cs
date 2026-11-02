using UnityEngine;

namespace Bezoro.Unity.Core.Editor.Common.Extensions
{
	/// <summary>
	///     Contains extension methods for Unity Object.
	/// </summary>
	public static class UnityObjectExtensions
	{
		public static bool IsOfType<T>(this Object obj)
			where T : Object =>
			obj is T;
	}
}
