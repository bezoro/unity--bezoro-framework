using System;
using System.Collections.Generic;
using Bezoro.Unity.Core.Common.Serialization;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Bezoro.Unity.Core.Editor.PropertyDrawers
{
	/// <summary>
	///     Edits Unity object and managed implementations without an inspector dependency.
	/// </summary>
	[CustomPropertyDrawer(typeof(SerializableInterface<>), true)]
	[CustomPropertyDrawer(typeof(SerializableInterface<,>), true)]
	public sealed class SerializableInterfaceDrawer : SerializedReferenceInterfaceDrawer
	{
		private readonly Dictionary<string, ReferenceMode> _emptyReferenceModes = new();
		private static readonly string[] SourceLabels = { "Managed object", "Unity object" };
		private static float RowHeight => EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
		private static float UnsupportedHeight => RowHeight * 3;

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			if (!property.isExpanded) return EditorGUIUtility.singleLineHeight;
			var wrapperType = GetWrapperType();
			if (wrapperType == null) return UnsupportedHeight;
			var arguments = wrapperType.GetGenericArguments();
			if (GetUnsupportedReason(property, arguments) != null) return UnsupportedHeight;
			var mode = GetMode(property, arguments);
			var height = RowHeight * (arguments.Length == 1 ? 3 : 2);
			if (mode == ReferenceMode.Raw)
			{
				var raw = property.FindPropertyRelative("rawReference");
				if (raw != null && (raw.propertyType != SerializedPropertyType.ManagedReference || raw.managedReferenceValue != null))
					height += EditorGUI.GetPropertyHeight(raw, true) + EditorGUIUtility.standardVerticalSpacing;
			}
			return height;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);
			var row = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
			property.isExpanded = EditorGUI.Foldout(row, property.isExpanded, label, true);
			var wrapperType = GetWrapperType();
			if (!property.isExpanded)
			{
				EditorGUI.EndProperty();
				return;
			}
			var arguments = wrapperType?.GetGenericArguments();
			var unsupportedReason = arguments == null
				? "The interface reference field type could not be resolved."
				: GetUnsupportedReason(property, arguments);
			if (arguments == null || unsupportedReason != null)
			{
				row.y += RowHeight;
				row.height = RowHeight * 2 - EditorGUIUtility.standardVerticalSpacing;
				EditorGUI.HelpBox(row, unsupportedReason ?? "The interface reference field type could not be resolved.", MessageType.Warning);
				EditorGUI.EndProperty();
				return;
			}

			EditorGUI.indentLevel++;
			var unity = property.FindPropertyRelative("unityReference");
			var raw = property.FindPropertyRelative("rawReference");
			var mode = GetMode(property, arguments);
			row.y += RowHeight;
			if (arguments.Length == 1)
			{
				EditorGUI.BeginChangeCheck();
				var previousMixedValue = EditorGUI.showMixedValue;
				EditorGUI.showMixedValue = HaveMixedSources(property, arguments);
				var selected = (ReferenceMode)EditorGUI.Popup(row, "Source", (int)mode, SourceLabels);
				EditorGUI.showMixedValue = previousMixedValue;
				if (EditorGUI.EndChangeCheck())
				{
					_emptyReferenceModes[GetPropertyKey(property)] = selected;
					unity.objectReferenceValue = null;
					raw.managedReferenceValue = null;
					mode = selected;
				}
				row.y += RowHeight;
			}

			if (mode == ReferenceMode.Unity)
				DrawUnityReference(row, unity, arguments);
			else if (raw.propertyType == SerializedPropertyType.ManagedReference)
			{
				var typeName = raw.hasMultipleDifferentValues ? "Mixed values" : raw.managedReferenceValue?.GetType().Name ?? "None";
				var button = EditorGUI.PrefixLabel(row, new GUIContent("Implementation"));
				if (GUI.Button(button, typeName, EditorStyles.popup)) ShowImplementationMenu(raw, arguments[0]);
				if (raw.managedReferenceValue != null)
				{
					row.y += RowHeight;
					row.height = EditorGUI.GetPropertyHeight(raw, true);
					EditorGUI.PropertyField(row, raw, new GUIContent("Value"), true);
				}
			}
			else
			{
				EditorGUI.LabelField(row, "Implementation", arguments[1].Name);
				row.y += RowHeight;
				row.height = EditorGUI.GetPropertyHeight(raw, true);
				EditorGUI.PropertyField(row, raw, new GUIContent("Value"), true);
			}

			EditorGUI.indentLevel--;
			EditorGUI.EndProperty();
		}

		private static void DrawUnityReference(Rect row, SerializedProperty property, Type[] arguments)
		{
			var objectType = arguments.Length == 2 ? arguments[1] : typeof(Object);
			EditorGUI.BeginChangeCheck();
			var previousMixedValue = EditorGUI.showMixedValue;
			EditorGUI.showMixedValue = property.hasMultipleDifferentValues;
			var selected = EditorGUI.ObjectField(row, new GUIContent("Object"), property.objectReferenceValue, objectType, true);
			EditorGUI.showMixedValue = previousMixedValue;
			if (!EditorGUI.EndChangeCheck()) return;
			if (selected is GameObject gameObject)
			{
				foreach (var component in gameObject.GetComponents<Component>())
				{
					if (component != null && arguments[0].IsInstanceOfType(component) && objectType.IsInstanceOfType(component))
					{
						selected = component;
						break;
					}
				}
			}
			if (selected == null || arguments[0].IsInstanceOfType(selected)) property.objectReferenceValue = selected;
			else Debug.LogWarning($"Assign a Unity object implementing {arguments[0].Name}.");
		}

		private ReferenceMode GetMode(SerializedProperty property, Type[] arguments)
		{
			if (arguments.Length == 2) return typeof(Object).IsAssignableFrom(arguments[1]) ? ReferenceMode.Unity : ReferenceMode.Raw;
			if (property.FindPropertyRelative("unityReference")?.objectReferenceValue != null) return ReferenceMode.Unity;
			var raw = property.FindPropertyRelative("rawReference");
			if (raw != null && raw.propertyType == SerializedPropertyType.ManagedReference && raw.managedReferenceValue != null)
				return ReferenceMode.Raw;
			return _emptyReferenceModes.TryGetValue(GetPropertyKey(property), out var mode) ? mode : ReferenceMode.Unity;
		}

		private static string? GetUnsupportedReason(SerializedProperty property, Type[] arguments)
		{
			var unity = property.FindPropertyRelative("unityReference");
			if (unity == null || unity.propertyType != SerializedPropertyType.ObjectReference)
				return "Unity cannot serialize this interface reference's object field.";
			if (arguments.Length == 2 && typeof(Object).IsAssignableFrom(arguments[1])) return null;
			var raw = property.FindPropertyRelative("rawReference");
			if (raw == null || arguments.Length == 1 && raw.propertyType != SerializedPropertyType.ManagedReference)
				return "Unity cannot serialize this implementation. Use a serializable class with supported fields.";
			return null;
		}

		private bool HaveMixedSources(SerializedProperty property, Type[] arguments)
		{
			if (!property.serializedObject.isEditingMultipleObjects) return false;
			var mode = GetMode(property, arguments);
			foreach (var target in property.serializedObject.targetObjects)
			{
				if (target == null) continue;
				using var serializedObject = new SerializedObject(target);
				var candidate = serializedObject.FindProperty(property.propertyPath);
				if (candidate != null && GetMode(candidate, arguments) != mode) return true;
			}
			return false;
		}

		private static string GetPropertyKey(SerializedProperty property) =>
			property.serializedObject.targetObject.GetEntityId() + ":" + property.propertyPath;

		private Type? GetWrapperType()
		{
			var type = fieldInfo?.FieldType;
			if (type == null) return null;
			if (type.IsArray) type = type.GetElementType();
			else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>)) type = type.GetGenericArguments()[0];
			while (type != null)
			{
				if (type.IsGenericType)
				{
					var definition = type.GetGenericTypeDefinition();
					if (definition == typeof(SerializableInterface<>) || definition == typeof(SerializableInterface<,>)) return type;
				}
				type = type.BaseType;
			}
			return null;
		}
	}
}
