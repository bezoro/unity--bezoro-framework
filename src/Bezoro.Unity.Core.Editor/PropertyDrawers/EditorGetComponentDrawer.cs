using Bezoro.Unity.Core.Attributes;
using Bezoro.Unity.Core.Common.Extensions;
using UnityEditor;
using UnityEngine;

namespace Bezoro.Unity.Core.Editor.PropertyDrawers
{
	[CustomPropertyDrawer(
		typeof(EditorGetComponentAttribute)
	)]
	/// <summary>
	/// Draws auto/manual component reference fields backed by EditorGetComponentAttribute.
	/// </summary>
	public class EditorGetComponentDrawer : PropertyDrawer
	{
		private MessageType _messageType;
		private string?     _message;

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			var attr = (EditorGetComponentAttribute)attribute;

			if (attr.OverrideDrawer)
				return EditorGUI.GetPropertyHeight(
					property,
					label,
					true
				);

			CalculateState(
				property
			);

			float propertyHeight = EditorGUI.GetPropertyHeight(
				property,
				label,
				true
			);

			if (_messageType == MessageType.None) return propertyHeight;

			float helpBoxHeight = CalculateHelpBoxHeight(
				_message ?? string.Empty
			);

			return propertyHeight + helpBoxHeight + 2f;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			var attr = (EditorGetComponentAttribute)attribute;

			// Create a smaller toggle button on the right side
			var toggleRect = new Rect(
				position.x + position.width - 60,
				position.y,
				60,
				EditorGUIUtility.singleLineHeight
			);

			// Draw toggle with a custom style to make it more visible
			var originalColor                            = GUI.backgroundColor;
			if (attr.OverrideDrawer) GUI.backgroundColor = Color.green;

			if (GUI.Button(
					toggleRect,
					attr.OverrideDrawer ? "Manual" : "Auto",
					EditorStyles.miniButton
				))
				attr.OverrideDrawer = !attr.OverrideDrawer;

			GUI.backgroundColor = originalColor;

			// Adjust position for the main property (make room for the button)
			var propertyPosition = new Rect(
				position.x,
				position.y,
				position.width - 65,
				position.height
			);

			if (attr.OverrideDrawer)
			{
				// Draw as normal property field when override is enabled
				EditorGUI.PropertyField(
					propertyPosition,
					property,
					label,
					true
				);

				return;
			}

			EditorGUI.BeginProperty(
				propertyPosition,
				label,
				property
			);

			TryApplyComponent(
				property
			);

			var propertyRect = new Rect(
				propertyPosition
			);

			if (_messageType != MessageType.None)
			{
				float helpBoxHeight = CalculateHelpBoxHeight(
					_message ?? string.Empty
				);

				var helpBoxRect = new Rect(
					propertyPosition.x,
					propertyPosition.y,
					propertyPosition.width,
					helpBoxHeight
				);

				EditorGUI.HelpBox(
					helpBoxRect,
					_message,
					_messageType
				);

				propertyRect.y      += helpBoxHeight + 2f;
				propertyRect.height -= helpBoxHeight + 2f;
			}

			using (new EditorGUI.DisabledScope(
					   true
				   ))
			{
				EditorGUI.PropertyField(
					propertyRect,
					property,
					label,
					true
				);
			}

			EditorGUI.EndProperty();
		}

		private static Component? FindComponent(GameObject gameObject, EditorGetComponentAttribute attr)
		{
			var component = gameObject.GetComponent(
				attr.ComponentType
			);

			if (component == null && attr.IncludeChildren)
				component = gameObject.GetComponentInChildren(
					attr.ComponentType,
					true
				);

			if (component == null && attr.IncludeParents)
				component = gameObject.GetComponentInParents(
					attr.ComponentType
				);

			return component;
		}

		private Component? TryFindComponent(SerializedProperty property)
		{
			var attr                = (EditorGetComponentAttribute)attribute;
			var targetMonoBehaviour = property.serializedObject.targetObject as MonoBehaviour;
			return targetMonoBehaviour == null
					   ? null
					   : FindComponent(
						   targetMonoBehaviour.gameObject,
						   attr
					   );
		}

		private float CalculateHelpBoxHeight(string message)
		{
			var content = new GUIContent(
				message
			);

			var style = EditorStyles.helpBox;
			return style.CalcHeight(
				content,
				EditorGUIUtility.currentViewWidth
			);
		}

		private void CalculateState(SerializedProperty property)
		{
			_message     = null;
			_messageType = MessageType.None;

			if (!typeof(Component).IsAssignableFrom(
					fieldInfo.FieldType
				))
			{
				_message     = $"Field '{fieldInfo.Name}' must be of type UnityEngine.Component or a subclass.";
				_messageType = MessageType.Warning;
				return;
			}

			if (UnityEngine.Application.isPlaying) return;

			var component = TryFindComponent(
				property
			);

			if (component == null)
			{
				var    attr             = (EditorGetComponentAttribute)attribute;
				var    targetGameObject = (property.serializedObject.targetObject as MonoBehaviour)?.gameObject;
				string targetName       = targetGameObject != null ? targetGameObject.name : "target";

				_message =
					$"No component of type '{attr.ComponentType.Name}' found on '{targetName}' or its relatives.";

				_messageType = MessageType.Error;
			}
		}

		private void TryApplyComponent(SerializedProperty property)
		{
			if (UnityEngine.Application.isPlaying) return;

			var newComponent = TryFindComponent(
				property
			);

			if (property.objectReferenceValue == newComponent) return;

			property.objectReferenceValue = newComponent;
			property.serializedObject.ApplyModifiedProperties();
		}
	}
}
