namespace Bezoro.Framework.Unity.Editor.Utilities.EditorPrefs
{
	public class EditorPrefsSettingsStorage : ISettingsStorage
	{
		public bool GetBool( string key ) => UnityEditor.EditorPrefs.GetBool( key );

		public void SetBool( string key, bool value )
		{
			UnityEditor.EditorPrefs.SetBool( key, value );
		}
	}
}
