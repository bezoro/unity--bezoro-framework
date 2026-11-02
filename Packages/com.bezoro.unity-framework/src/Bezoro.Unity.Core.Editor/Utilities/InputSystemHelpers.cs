using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Bezoro.Unity.Core.Editor.Utilities
{
	/// <summary>
	///     Reads and changes Unity's active input handling setting.
	/// </summary>
	public static class InputSystemHelpers
	{
		public static SerializedProperty Get_Active_Input_System_Property() =>
			Get_Player_Settings_Serialized_Object().FindProperty(
				"activeInputHandler"
			);

		public static void Set_Active_Input_System(InputSystem inputSystem)
		{
			var playerSettingsSerializedObject = Get_Player_Settings_Serialized_Object();
			var property                       = Get_Active_Input_System_Property();
			property.intValue = (int)inputSystem;
			playerSettingsSerializedObject.ApplyModifiedProperties();
		}

		private static SerializedObject Get_Player_Settings_Serialized_Object()
		{
			var playerSettings = Resources.FindObjectsOfTypeAll<PlayerSettings>().FirstOrDefault();
			return new(
				playerSettings
			);
		}

		/// <summary>
		///     Lists the supported Input System values.
		/// </summary>
		public enum InputSystem
		{
			Legacy = 0,
			New    = 1,
			Both   = 2
		}
	}
}
