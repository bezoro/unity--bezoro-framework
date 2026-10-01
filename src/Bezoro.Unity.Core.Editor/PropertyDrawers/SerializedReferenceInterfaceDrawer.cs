using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Bezoro.Unity.Core.Editor.PropertyDrawers
{
	[UsedImplicitly]
	/// <summary>
	/// Draws managed-reference fields constrained to a selected interface implementation.
	/// </summary>
	public class SerializedReferenceInterfaceDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(
				position,
				label,
				property
			);

			var type = fieldInfo.FieldType;

			if (property.propertyType == SerializedPropertyType.ManagedReference)
			{
				var targetType = type.GetGenericArguments()[0];

				if (targetType.IsInterface &&
					property.managedReferenceValue != null &&
					targetType.IsAssignableFrom(
						property.managedReferenceValue.GetType()
					))
					property.managedReferenceValue =
						EditorGUI.ObjectField(
							position,
							label,
							property.managedReferenceValue as Object,
							targetType,
							true
						);
				else
					EditorGUI.LabelField(
						position,
						label.text,
						"Type not supported."
					);
			}

			EditorGUI.EndProperty();
		}
	}
}
