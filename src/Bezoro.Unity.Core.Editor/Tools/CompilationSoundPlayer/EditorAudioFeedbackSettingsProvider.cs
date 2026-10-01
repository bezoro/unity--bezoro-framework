using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Bezoro.Unity.Core.Editor.Tools.CompilationSoundPlayer
{
	internal static class EditorAudioFeedbackSettingsProvider
	{
		private const string SETTINGS_PATH = "Project/Bezoro/Editor Audio Feedback";

		[SettingsProvider]
		private static SettingsProvider CreateSettingsProvider()
		{
			return new SettingsProvider(SETTINGS_PATH, SettingsScope.Project)
			{
				label = "Editor Audio Feedback",
				guiHandler = _ => DrawSettings(),
				keywords = new HashSet<string>
				{
					"audio",
					"build",
					"compilation",
					"editor",
					"feedback",
					"sound",
					"test",
					"volume"
				}
			};
		}

		private static void DrawSettings()
		{
			var settings         = EditorAudioFeedbackSettings.instance;
			var serializedObject = new SerializedObject(
				settings
			);

			serializedObject.Update();

			EditorGUI.BeginChangeCheck();

			DrawProperty(serializedObject, "play", "Enabled");
			DrawVolumeSlider(serializedObject, "masterVolume", "Master Volume");

			EditorGUILayout.Space();

			DrawEventSettings(
				serializedObject,
				"Compilation Started",
				"compilationStartClip",
				"compilationStartVolume"
			);
			DrawEventSettings(
				serializedObject,
				"Compilation Finished",
				"compilationEndClip",
				"compilationEndVolume"
			);
			DrawEventSettings(
				serializedObject,
				"Unity Ready",
				"unityFullyInitializedClip",
				"unityFullyInitializedVolume"
			);
			DrawEventSettings(
				serializedObject,
				"Build Started",
				"buildStartedClip",
				"buildStartedVolume"
			);
			DrawEventSettings(
				serializedObject,
				"Build Succeeded",
				"buildCompletedSuccessfullyClip",
				"buildCompletedSuccessfullyVolume"
			);
			DrawEventSettings(
				serializedObject,
				"Build Failed",
				"buildFailedClip",
				"buildFailedVolume"
			);
			DrawEventSettings(
				serializedObject,
				"Tests Started",
				"testStartClip",
				"testStartVolume"
			);
			DrawEventSettings(
				serializedObject,
				"Tests Finished",
				"testEndClip",
				"testEndVolume"
			);

			if (!EditorGUI.EndChangeCheck()) return;

			serializedObject.ApplyModifiedProperties();
			settings.NormalizeVolumes();
			EditorUtility.SetDirty(settings);
			settings.SaveSettings();
			EditorAudioFeedbackSettings.InitializeAudioFeedback();
		}

		private static void DrawEventSettings(
			SerializedObject serializedObject,
			string label,
			string clipPropertyName,
			string volumePropertyName
		)
		{
			EditorGUILayout.LabelField(
				label,
				EditorStyles.boldLabel
			);
			EditorGUI.indentLevel++;
			DrawProperty(
				serializedObject,
				clipPropertyName,
				"Clip"
			);
			DrawVolumeSlider(
				serializedObject,
				volumePropertyName,
				"Volume"
			);
			EditorGUI.indentLevel--;
			EditorGUILayout.Space();
		}

		private static void DrawProperty(
			SerializedObject serializedObject,
			string propertyName,
			string label
		)
		{
			EditorGUILayout.PropertyField(
				serializedObject.FindProperty(
					propertyName
				),
				new GUIContent(
					label
				)
			);
		}

		private static void DrawVolumeSlider(
			SerializedObject serializedObject,
			string propertyName,
			string label
		)
		{
			var property = serializedObject.FindProperty(
				propertyName
			);
			property.floatValue = EditorGUILayout.Slider(
				label,
				Mathf.Clamp01(
					property.floatValue
				),
				0f,
				1f
			);
		}
	}
}
