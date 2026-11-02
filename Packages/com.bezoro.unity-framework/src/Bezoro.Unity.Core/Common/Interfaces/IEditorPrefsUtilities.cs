namespace Bezoro.Unity.Core.Common.Interfaces;

/// <summary>
///     Defines typed access to persisted Unity editor preferences.
/// </summary>
public interface IEditorPrefsUtilities
{
	public bool GetBool(string key);

	public float GetFloat(string key);

	public int GetInt(string key);

	public string GetString(string key);
	public void   SetBool(string   key, bool   value);
	public void   SetFloat(string  key, float  value);
	public void   SetInt(string    key, int    value);
	public void   SetString(string key, string value);
}
