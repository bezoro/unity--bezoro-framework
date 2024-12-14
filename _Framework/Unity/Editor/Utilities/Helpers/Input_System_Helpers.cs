using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Bezoro.Framework.Unity.Editor.Utilities.Helpers
{
	public static class Input_System_Helpers
	{
		public static SerializedProperty Get_Active_Input_System_Property()
		{
			return Get_Player_Settings_Serialized_Object().FindProperty("activeInputHandler");
		}

		public static void Set_Active_Input_System(Input_System inputSystem)
		{
			var player_settings_serialized_object = Get_Player_Settings_Serialized_Object();
			var property                          = Get_Active_Input_System_Property();
			property.intValue = (int)inputSystem;
			player_settings_serialized_object.ApplyModifiedProperties();
		}

		private static SerializedObject Get_Player_Settings_Serialized_Object()
		{
			var player_settings = Resources.FindObjectsOfTypeAll<PlayerSettings>().FirstOrDefault();
			return new( player_settings );
		}

		public enum Input_System
		{
			Legacy = 0,
			New    = 1,
			Both   = 2
		}
	}
}
