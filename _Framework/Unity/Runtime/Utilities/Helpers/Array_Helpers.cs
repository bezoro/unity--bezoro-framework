using UnityEngine;

namespace Bezoro.Framework.Unity.Utilities
{
	public static class Array_Helpers
	{
		/// <summary>
		///     Returns a string representation of the given item.
		/// </summary>
		/// <param name="item">The item to format.</param>
		/// <returns>A string representation of the item.</returns>
		/// <remarks>
		///     The format of the string is as follows:
		///     <list type="bullet">
		///         <item>if the item is null, it is represented as "null".</item>
		///         <item>if the item is a <see cref="string" />, it is enclosed in double quotes.</item>
		///         <item>if the item is a <see cref="GameObject" />, it is represented by its name.</item>
		///         <item>otherwise, the item is represented by its result of calling <see cref="object.ToString()" />.</item>
		///     </list>
		/// </remarks>
		public static string Get_Element_Name<T>(T item)
		{
			return item switch
			{
				null         => "null",
				string s     => $"\"{s}\"",
				GameObject o => o.name,
				_            => item.GetType().Name
			};
		}
	}
}
