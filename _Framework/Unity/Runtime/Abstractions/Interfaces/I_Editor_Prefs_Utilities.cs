namespace Bezoro.Framework.Unity
{
	public interface I_Editor_Prefs_Utilities
	{
		public bool Get_Bool(string key);
		public void Set_Bool(string key, bool value);

		public float Get_Float(string key);
		public void Set_Float(string key, float value);

		public int Get_Int(string key);
		public void Set_Int(string key, int value);

		public string Get_String(string key);
		public void Set_String(string key, string value);
	}
}
