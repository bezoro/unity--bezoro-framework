using System.Reflection;
using UnityEditor;

namespace Bezoro.Unity.Core.Editor.Common.Extensions
{
	/// <summary>
	///     Contains extension methods for Field Info.
	/// </summary>
	public static class FieldInfoExtensions
	{
		public static T? GetFieldValue<T>(this FieldInfo fieldInfo, SerializedProperty property)
			where T : class =>
			fieldInfo.GetValue(
				property.serializedObject.targetObject
			) as T;
	}
}
