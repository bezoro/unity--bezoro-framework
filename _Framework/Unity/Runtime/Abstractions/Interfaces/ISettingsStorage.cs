namespace Bezoro.Framework.Unity
{
	public interface ISettingsStorage
	{
		bool GetBool(string key);
		void SetBool(string key, bool value);
	}
}
