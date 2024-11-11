using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Bezoro.Framework.Unity.Editor.Utilities.Helpers
{
	public static class InputSystemHelpers
	{
		public static SerializedProperty GetActiveInputSystemProperty()
		{
			return GetPlayerSettingsSerializedObject().FindProperty("activeInputHandler");
		}

		public static void SetActiveInputSystem(InputSystem inputSystem)
		{
			var playerSettingsSerializedObject = GetPlayerSettingsSerializedObject();
			var property = playerSettingsSerializedObject.FindProperty("activeInputHandler");
			property.intValue = (int)inputSystem;
			playerSettingsSerializedObject.ApplyModifiedProperties();
		}

		private static SerializedObject GetPlayerSettingsSerializedObject()
		{
			var playerSettings = Resources.FindObjectsOfTypeAll<PlayerSettings>().FirstOrDefault();

			return new SerializedObject(playerSettings);
		}

		public enum InputSystem
		{
			Legacy = 0,
			New = 1,
			Both = 2
		}
	}
}
