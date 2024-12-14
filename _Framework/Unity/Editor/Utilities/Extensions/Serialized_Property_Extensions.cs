using UnityEditor;

namespace Bezoro.Framework.Unity.Editor.Utilities
{
	public static class Serialized_Property_Extensions
	{
		public static SerializedProperty Find_Property_By_Auto_Property_Name(
			this SerializedObject obj,
			string prop_name
		)
		{
			return obj.FindProperty($"<{prop_name}>k__BackingField");
		}
	}
}
