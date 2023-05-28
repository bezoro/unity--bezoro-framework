using UnityEditor;

namespace Bezoro.Framework.Core.Editor.Extensions
{
	public static class SerializedPropertyExtensions
	{
		public static SerializedProperty FindPropertyByAutoPropertyName
			(this SerializedObject obj, string propName) =>
			obj.FindProperty($"<{propName}>k__BackingField");
	}
}