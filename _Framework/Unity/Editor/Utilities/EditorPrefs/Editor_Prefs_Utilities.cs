namespace Bezoro.Framework.Unity.Editor.Utilities.EditorPrefs
{
	public class Editor_Prefs_Utilities : I_Editor_Prefs_Utilities
	{
		public bool Get_Bool(string key)
		{
			return UnityEditor.EditorPrefs.GetBool(key);
		}

		public void Set_Bool(string key, bool value)
		{
			UnityEditor.EditorPrefs.SetBool(key, value);
		}

		public float Get_Float(string key)
		{
			return UnityEditor.EditorPrefs.GetFloat(key);
		}

		public void Set_Float(string key, float value)
		{
			UnityEditor.EditorPrefs.SetFloat(key, value);
		}

		public int Get_Int(string key)
		{
			return UnityEditor.EditorPrefs.GetInt(key);
		}

		public void Set_Int(string key, int value)
		{
			UnityEditor.EditorPrefs.SetInt(key, value);
		}

		public string Get_String(string key)
		{
			return UnityEditor.EditorPrefs.GetString(key);
		}

		public void Set_String(string key, string value)
		{
			UnityEditor.EditorPrefs.SetString(key, value);
		}
	}
}
