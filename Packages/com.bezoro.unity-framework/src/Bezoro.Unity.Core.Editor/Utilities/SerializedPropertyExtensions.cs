using UnityEditor;

namespace Bezoro.Unity.Core.Editor.Utilities
{
	/// <summary>
	///     Contains extension methods for Serialized Property.
	/// </summary>
	public static class SerializedPropertyExtensions
	{
		public static SerializedProperty Find_Property_By_Auto_Property_Name(
			this SerializedObject obj,
			string                propName
		) =>
			obj.FindProperty(
				$"<{propName}>k__BackingField"
			);
	}
}
