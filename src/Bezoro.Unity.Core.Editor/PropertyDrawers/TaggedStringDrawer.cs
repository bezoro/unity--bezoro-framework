using System;
using Bezoro.Unity.Core.Common.Serialization;
using UnityEditor;
using UnityEngine;

namespace Bezoro.Unity.Core.Editor.PropertyDrawers
{
	/// <summary>
	///     Edits tag templates and previews their processed text with native Inspector controls.
	/// </summary>
	[CustomPropertyDrawer(typeof(TaggedString))]
	public sealed class TaggedStringDrawer : PropertyDrawer
	{
		private static float Spacing => EditorGUIUtility.standardVerticalSpacing;
		private static float LineHeight => EditorGUIUtility.singleLineHeight;

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			var template = property.FindPropertyRelative("template");
			var height = LineHeight + Spacing + EditorGUI.GetPropertyHeight(template, true);
			if (string.IsNullOrEmpty(template.stringValue)) return height;
			return height + Spacing + LineHeight + Spacing + PreviewHeight(template.stringValue) + Spacing + LineHeight;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);
			var row = new Rect(position.x, position.y, position.width, LineHeight);
			EditorGUI.LabelField(row, label);
			row.y += LineHeight + Spacing;
			var template = property.FindPropertyRelative("template");
			row.height = EditorGUI.GetPropertyHeight(template, true);
			EditorGUI.PropertyField(row, template, new GUIContent("Template"), true);
			if (!string.IsNullOrEmpty(template.stringValue))
			{
				row.y += row.height + Spacing;
				row.height = LineHeight;
				EditorGUI.LabelField(row, "Preview");
				row.y += LineHeight + Spacing;
				row.height = PreviewHeight(template.stringValue);
				try
				{
					EditorGUI.SelectableLabel(row, new TaggedString(template.stringValue).Processed, EditorStyles.textArea);
				}
				catch (Exception exception)
				{
					EditorGUI.HelpBox(row, exception.Message, MessageType.Error);
				}

				row.y += row.height + Spacing;
				row.height = LineHeight;
				row.width = (position.width - Spacing) * 0.5f;
				if (GUI.Button(row, "Clear Template")) template.stringValue = string.Empty;
				row.x += row.width + Spacing;
				if (GUI.Button(row, "Validate Tags"))
				{
					try
					{
						string processed = new TaggedString(template.stringValue).Processed;
						Debug.Log($"✓ Tags validated successfully. Result: '{processed}'", property.serializedObject.targetObject);
					}
					catch (Exception exception)
					{
						Debug.LogError($"✗ Tag validation failed: {exception.Message}", property.serializedObject.targetObject);
					}
				}
			}
			EditorGUI.EndProperty();
		}

		private static float PreviewHeight(string template)
		{
			try
			{
				var content = new GUIContent(new TaggedString(template).Processed);
				return Mathf.Max(LineHeight * 2, EditorStyles.textArea.CalcHeight(content,
					Mathf.Max(80f, EditorGUIUtility.currentViewWidth - 50f)));
			}
			catch (Exception)
			{
				return LineHeight * 3;
			}
		}
	}
}
