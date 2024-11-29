using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace Bezoro.Framework.Unity.Editor.PropertyDrawers
{
	[UsedImplicitly]
	public class Serialized_Reference_Interface_Drawer : PropertyDrawer
	{
		public override void OnGUI( Rect position, SerializedProperty property, GUIContent label )
		{
			EditorGUI.BeginProperty( position, label, property );
			var type = fieldInfo.FieldType;
			if ( property.propertyType == SerializedPropertyType.ManagedReference )
			{
				var targetType = type.GetGenericArguments()[0];
				if ( targetType.IsInterface
				  && property.managedReferenceValue != null
				  && targetType.IsAssignableFrom( property.managedReferenceValue.GetType() ) )
				{
					property.managedReferenceValue =
						EditorGUI.ObjectField(
							position,
							label,
							property.managedReferenceValue as Object,
							targetType,
							true );
				}
				else
				{
					EditorGUI.LabelField( position, label.text, "Type not supported." );
				}
			}
			EditorGUI.EndProperty();
		}
	}
}
