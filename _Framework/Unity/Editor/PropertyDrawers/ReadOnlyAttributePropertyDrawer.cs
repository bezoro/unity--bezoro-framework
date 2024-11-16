using Bezoro.Framework.Unity.Attributes;
using UnityEditor;
using UnityEngine;

namespace Bezoro.Framework.Unity.Editor.PropertyDrawers
{
	[CustomPropertyDrawer( typeof(ReadOnlyAttribute) )]
	public class ReadOnlyAttributePropertyDrawer : PropertyDrawer
	{
		public override void OnGUI( Rect position, SerializedProperty property, GUIContent label )
		{
			// Save previous GUI enabled value
			var previousGUIState = GUI.enabled;
			// Disable the GUI
			GUI.enabled = false;
			// Draw the property as usual
			EditorGUI.PropertyField( position, property, label );
			// Restore the GUI state
			GUI.enabled = previousGUIState;
		}
	}
}
