using System.Reflection;
using Bezoro.Framework.Unity.Attributes;
using Bezoro.Framework.Unity.Utilities;
using UnityEditor;
using UnityEngine;

namespace Bezoro.Framework.Unity.Editor.PropertyDrawers
{
	[CustomPropertyDrawer( typeof(EditorGetComponentAttribute) )]
	public class EditorGetComponentDrawer : PropertyDrawer
	{
		public override void OnGUI( Rect position, SerializedProperty property, GUIContent label )
		{
			EditorGUI.BeginProperty( position, label, property );

			var target = property.serializedObject.targetObject as MonoBehaviour;

			if ( target != null && attribute is EditorGetComponentAttribute attr )
			{
				var field = target.GetType().GetField(
					property.name,
					BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
				);

				if ( AssignComponent( target, field, attr ) )
				{
					property.serializedObject.ApplyModifiedProperties();
				}
			}

			EditorGUI.PropertyField( position, property, label );
			EditorGUI.EndProperty();
		}

		private bool AssignComponent( MonoBehaviour mb, FieldInfo field, EditorGetComponentAttribute attr )
		{
			if ( !typeof(Component).IsAssignableFrom( field.FieldType ) )
			{
				EditorGUILayout.HelpBox(
					$"Field {field.Name} in {mb.GetType().Name} must inherit from UnityEngine.Component",
					MessageType.Warning
				);

				return false;
			}

			var currentComponent = field.GetValue( mb ) as Component;
			var newComponent     = mb.gameObject.GetComponent( attr.ComponentType );

			if ( newComponent == null && attr.IncludeChildren )
			{
				newComponent = mb.gameObject.GetComponentInChildren( attr.ComponentType, true );
			}

			if ( newComponent == null && attr.IncludeParents )
			{
				newComponent = mb.gameObject.GetComponentInParents( attr.ComponentType );
			}

			if ( currentComponent != newComponent )
			{
				field.SetValue( mb, newComponent );

				return true;
			}

			if ( newComponent == null )
			{
				EditorGUILayout.HelpBox(
					$"No component of type {attr.ComponentType.Name} found on {mb.gameObject.name}",
					MessageType.Error
				);
			}

			return false;
		}
	}
}
