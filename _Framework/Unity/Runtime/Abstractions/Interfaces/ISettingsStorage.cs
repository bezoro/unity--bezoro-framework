namespace Bezoro.Framework.Unity
{
	public interface ISettingsStorage
	{
		public bool GetBool( string key );
		public void SetBool( string key, bool value );
	}
}
