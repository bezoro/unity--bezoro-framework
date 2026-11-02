using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Bezoro.Unity.Core.Editor.Tools
{
	/// <summary>
	///     Provides manual domain and scene reload controls for the Unity editor.
	/// </summary>
	public class EditorToolsWindow : EditorWindow
	{
		private static bool IsDomainReloadDisabled => EditorApplication.isCompiling;

		private static bool IsSceneReloadDisabled => !EditorApplication.isPlaying;

		#region Unity Lifecycle

		private void OnGUI()
		{
			Disableable(
				DomainReloadButton,
				IsDomainReloadDisabled
			);

			Disableable(
				SceneReloadButton,
				IsSceneReloadDisabled
			);
		}

		#endregion

		[MenuItem(
			"Bezoro/Editor Tools Window"
		)]
		public static void ShowWindow()
		{
			var window = GetWindow<EditorToolsWindow>();
			window.titleContent = new(
				"Scene Tools"
			);

			window.Show();
		}

		private static void Disableable(Action renderer, bool disabled)
		{
			EditorGUI.BeginDisabledGroup(
				disabled
			);

			renderer();
			EditorGUI.EndDisabledGroup();
		}

		private static void DomainReloadButton()
		{
			if (!GUILayout.Button(
					"Reload Domain"
				)) return;

			EditorUtility.RequestScriptReload();
		}

		private static void SceneReloadButton()
		{
			if (!GUILayout.Button(
					"Reload Scene"
				)) return;

			var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
			var opts  = new LoadSceneParameters();
			EditorSceneManager.LoadSceneInPlayMode(
				scene.path,
				opts
			);
		}
	}
}
