using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Bezoro.Framework.Unity.Editor.Tools
{
	public class Editor_Tools_Window : EditorWindow
	{
		private static bool isDomainReloadDisabled => EditorApplication.isCompiling;

		private static bool isSceneReloadDisabled => !EditorApplication.isPlaying;

		[MenuItem("Bezoro/Editor Tools Window")]
		public static void ShowWindow()
		{
			var window = GetWindow<Editor_Tools_Window>();
			window.titleContent = new( "Scene Tools" );
			window.Show();
		}

		private void OnGUI()
		{
			Disableable(DomainReloadButton, isDomainReloadDisabled);
			Disableable(SceneReloadButton, isSceneReloadDisabled);
		}

		private static void Disableable(Action renderer, bool disabled)
		{
			EditorGUI.BeginDisabledGroup(disabled);
			renderer();
			EditorGUI.EndDisabledGroup();
		}

		private static void DomainReloadButton()
		{
			if (!GUILayout.Button("Reload Domain")) return;
			EditorUtility.RequestScriptReload();
		}

		private static void SceneReloadButton()
		{
			if (!GUILayout.Button("Reload Scene")) return;
			var scene = SceneManager.GetActiveScene();
			var opts  = new LoadSceneParameters();
			EditorSceneManager.LoadSceneInPlayMode(scene.path, opts);
		}
	}
}
