using System;
using System.Collections;
using System.Reflection;
using UnityEditor;

namespace Bezoro.Unity.Core.Editor.Utilities
{
	/// <summary>
	///     Reads and writes values behind Unity SerializedProperty instances.
	/// </summary>
	public static class SerializedPropertyUtils
	{
		public static object? GetValue(SerializedProperty property)
		{
			object? obj = property.serializedObject.targetObject;

			string path = property.propertyPath.Replace(
				".Array.data[",
				"["
			);

			string[] elements = path.Split(
				'.'
			);

			foreach (string element in elements)
			{
				if (element.Contains(
						"["
					))
				{
					string elementName = element.Substring(
						0,
						element.IndexOf(
							"[",
							StringComparison.Ordinal
						)
					);

					var index = Convert.ToInt32(
						element.Substring(
								   element.IndexOf(
									   "[",
									   StringComparison.Ordinal
								   )
							   )
							   .Replace(
								   "[",
								   ""
							   )
							   .Replace(
								   "]",
								   ""
							   )
					);

					obj = GetValue_Imp(
						obj,
						elementName,
						index
					);
				}
				else
				{
					obj = GetValue_Imp(
						obj,
						element
					);
				}
			}

			return obj;
		}

		private static object? GetValue_Imp(object? source, string name, int index = -1)
		{
			if (source == null) return null;

			var type = source.GetType();

			var field = type.GetField(
				name,
				BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance
			);

			if (field == null) return null;

			object? val = field.GetValue(
				source
			);

			if (index >= 0)
				if (val is IEnumerable enumerable)
				{
					var enumerator = enumerable.GetEnumerator();

					while (index-- >= 0)
					{
						if (!enumerator.MoveNext())
							return null;
					}

					return enumerator.Current;
				}

			return val;
		}
	}
}
