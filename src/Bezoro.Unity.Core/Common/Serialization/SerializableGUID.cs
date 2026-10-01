#if UNITY_EDITOR
using System.Collections;
using UnityEditor;
#endif
using System;
using System.Reflection;
using Bezoro.Core.Extensions;
using UnityEngine;
using Logger = Bezoro.Logging.Logger;

namespace Bezoro.Unity.Core.Common.Serialization;

/// <summary>
///     Validates, creates, and formats serialized GUID strings.
/// </summary>
public static class GUIDUtils
{
	public static bool Check_GUID_Validity(string guid)
	{
		if (string.IsNullOrEmpty(guid)) return false;
		if (Guid.TryParse(guid, out _)) return true;

		Logger.LogWarning($"Failed to parse GUID: {guid}");
		return false;
	}
}

/// <summary>
///     Stores a Unity-serializable GUID string.
/// </summary>
[Serializable]
public sealed class SerializableGUID
{
	#region Serialized Fields

	[SerializeField] private string guid = string.Empty;

	#endregion

	public string Value => guid ?? string.Empty;

	// Generates a new GUID *only if* the current one is invalid or force is true.
	public bool Generate_New_Valid_GUID(bool force = false)
	{
		bool needsGeneration = force || !GUIDUtils.Check_GUID_Validity(guid);

		if (needsGeneration)
		{
			var newGuid = Guid.NewGuid().ToString();
			guid = newGuid;
			Logger.LogSuccess($"Generated new valid GUID: {guid.Bold()}");
			return true; // Indicate that a new GUID was generated
		}

		return false; // Indicate no change was made
	}

	// Allows setting the GUID externally. Use with caution.
	public void Set_GUID(string newValue) =>
		guid = newValue ?? string.Empty;
}

#if UNITY_EDITOR
/// <summary>
///     Draws SerializableGUID values in the Inspector.
/// </summary>
[CustomPropertyDrawer(typeof(SerializableGUID))]
public sealed class SerializableGUIDDrawer : PropertyDrawer
{
	private const float BUTTON_HEIGHT = 20f;
	private const float SPACING       = 5f;

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label) =>
		// Calculate height needed for the text field and the button
		EditorGUIUtility.singleLineHeight + BUTTON_HEIGHT + SPACING;

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		EditorGUI.BeginProperty(
			position,
			label,
			property
		);

		var guidProp = property.FindPropertyRelative("guid");

		if (guidProp == null)
		{
			EditorGUI.HelpBox(
				position,
				"Failed to find 'guid' property.",
				MessageType.Error
			);

			EditorGUI.EndProperty();
			return;
		}

		var serializableGuidInstance = GetValue(property) as SerializableGUID;

		if (serializableGuidInstance == null)
		{
			EditorGUI.HelpBox(
				position,
				"Could not resolve target Serializable_GUID instance.",
				MessageType.Warning
			);

			EditorGUI.EndProperty();
			return;
		}

		// Ensure GUID is generated if invalid (e.g., on first creation)
		// Check validity before potentially modifying
		bool generatedNewGuid;

		if (!serializableGuidInstance.IsNull() && !GUIDUtils.Check_GUID_Validity(serializableGuidInstance.Value))
		{
			// Using Undo.RecordObject is important if generation happens automatically
			Undo.RecordObject(property.serializedObject.targetObject, "Auto-Generate GUID");

			generatedNewGuid = serializableGuidInstance.Generate_New_Valid_GUID(true); // Force generation if invalid

			if (generatedNewGuid)
			{
				guidProp.stringValue = serializableGuidInstance.Value; // Update serialized property
				property.serializedObject
						.ApplyModifiedPropertiesWithoutUndo(); // Apply change without creating a new undo state for this automatic step
			}
		}

		// Define areas for the label/field and the button
		var fieldRect = new Rect(
			position.x,
			position.y,
			position.width,
			EditorGUIUtility.singleLineHeight
		);

		var buttonRect = new Rect(
			position.x,
			position.y + EditorGUIUtility.singleLineHeight + SPACING,
			position.width,
			BUTTON_HEIGHT
		);

		// Draw the GUID field (disabled)
		EditorGUI.BeginDisabledGroup(true);

		EditorGUI.TextField(
			fieldRect,
			label,
			guidProp.stringValue
		);

		EditorGUI.EndDisabledGroup();

		// Draw the "Generate New GUID" button
		if (GUI.Button(buttonRect, "Generate New GUID"))
		{
			if (serializableGuidInstance != null)
			{
				Undo.RecordObject(property.serializedObject.targetObject, "Generate New GUID");

				serializableGuidInstance.Generate_New_Valid_GUID(true); // Force generation on button click

				guidProp.stringValue = serializableGuidInstance.Value; // Update the SerializedProperty
				property.serializedObject.ApplyModifiedProperties();   // Apply changes and record undo
			}
			else
			{
				Logger.LogError("Serializable_GUID instance is null, cannot generate GUID.");
			}
		}

		EditorGUI.EndProperty();
	}

	private static object? GetValue(SerializedProperty property)
	{
		object? obj = property.serializedObject.targetObject;

		string path = property.propertyPath.Replace(
			".Array.data[",
			"["
		);

		string[]? elements = path.Split('.');

		foreach (string element in elements)
		{
			if (element.Contains("["))
			{
				string elementName = element[..element.IndexOf("[", StringComparison.Ordinal)];
				var index = Convert.ToInt32(
					element[element.IndexOf("[", StringComparison.Ordinal)..]
						.Replace("[", string.Empty)
						.Replace("]", string.Empty)
				);

				obj = GetValue(obj, elementName, index);
			}
			else
			{
				obj = GetValue(obj, element);
			}
		}

		return obj;
	}

	private static object? GetValue(object? source, string name, int index = -1)
	{
		if (source == null) return null;

		var field = source.GetType()
						  .GetField(
							  name,
							  BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance
						  );

		if (field == null) return null;

		object? value = field.GetValue(source);

		if (index < 0) return value;
		if (value is not IEnumerable enumerable) return null;

		var enumerator = enumerable.GetEnumerator();

		while (index-- >= 0)
			if (!enumerator.MoveNext())
				return null;

		return enumerator.Current;
	}
}
#endif
