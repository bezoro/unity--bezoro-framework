using Bezoro.Unity.Core.Common.Interfaces;
using UnityEditor;

namespace Bezoro.Unity.Core.Editor.Utilities
{
	/// <summary>
	///     Saves and restores typed values in Unity EditorPrefs.
	/// </summary>
	public class EditorPrefsUtilities : IEditorPrefsUtilities
	{
		public bool GetBool(string key) =>
			EditorPrefs.GetBool(
				key
			);

		public float GetFloat(string key) =>
			EditorPrefs.GetFloat(
				key
			);

		public int GetInt(string key) =>
			EditorPrefs.GetInt(
				key
			);

		public string GetString(string key) =>
			EditorPrefs.GetString(
				key
			);

		public void SetBool(string key, bool value) =>
			EditorPrefs.SetBool(
				key,
				value
			);

		public void SetFloat(string key, float value) =>
			EditorPrefs.SetFloat(
				key,
				value
			);

		public void SetInt(string key, int value) =>
			EditorPrefs.SetInt(
				key,
				value
			);

		public void SetString(string key, string value) =>
			EditorPrefs.SetString(
				key,
				value
			);
	}
}
