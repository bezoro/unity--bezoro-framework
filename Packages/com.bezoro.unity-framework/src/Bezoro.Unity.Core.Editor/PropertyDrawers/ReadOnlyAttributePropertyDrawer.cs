using Bezoro.Unity.Core.Attributes;
using UnityEditor;
using UnityEngine;

namespace Bezoro.Unity.Core.Editor.PropertyDrawers
{
	/// <summary>
	///     Draws a property field that is disabled and cannot be edited in the Inspector.
	///     This is the property drawer for the <see cref="ReadOnlyAttribute" />.
	/// </summary>
	[CustomPropertyDrawer(
		typeof(ReadOnlyAttribute)
	)]
	/// <summary>
	/// Draws ReadOnlyAttribute fields disabled in the Inspector.
	/// </summary>
	public class ReadOnlyPropertyDrawer : PropertyDrawer
	{
		/// <summary>
		///     We need to override GetPropertyHeight to ensure that complex properties
		///     (like lists or multi-line strings) are rendered with the correct height.
		/// </summary>
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) =>
			// Pass the call to the base EditorGUI implementation, ensuring to include children.
			EditorGUI.GetPropertyHeight(
				property,
				label,
				true
			);

		/// <summary>
		///     Renders the property field as a disabled control.
		/// </summary>
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			// Using a DisabledScope is a cleaner and safer way to handle GUI state.
			// It automatically restores the previous state when the scope is exited.
			using (new EditorGUI.DisabledScope(
					   true
				   ))
			{
				// We pass 'true' for includeChildren to ensure that complex fields
				// (like Vectors or custom structs) are fully drawn.
				EditorGUI.PropertyField(
					position,
					property,
					label,
					true
				);
			}
		}
	}
}
