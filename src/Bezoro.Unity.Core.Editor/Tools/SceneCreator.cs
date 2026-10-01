using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Bezoro.Unity.Core.Editor.Tools
{
	/// <summary>
	///     Creates a new scene asset containing the selected hierarchy object.
	/// </summary>
	public static class SceneCreator
	{
		private const string MENU_PATH      = "Bezoro/Scene Creator/Create Scene From Selected Hierarchy";
		private const string NEW_SCENE_NAME = "NewScene";

		[MenuItem(
			MENU_PATH
		)]
		private static void Create()
		{
			string savePath = EditorUtility.SaveFilePanel(
				"Choose a directory to save the Scene to:",
				UnityEngine.Application.dataPath,
				NEW_SCENE_NAME,
				"unity"
			);

			if (string.IsNullOrEmpty(
					savePath
				)) return;

			Create_Scene_Asset_From_Selection(
				savePath
			);
		}

		[MenuItem(
			MENU_PATH,
			true
		)]
		private static bool Validate_Create() =>
			Selection.activeGameObject is { };

		private static Scene Create_Scene_Asset_From_Selection(string path)
		{
			var newScene = EditorSceneManager.NewScene(
				NewSceneSetup.EmptyScene,
				NewSceneMode.Additive
			);

			var activeGameObject = Selection.activeGameObject;
			var newObject = Object.Instantiate(
				activeGameObject
			);

			newObject.name = activeGameObject.name;

			EditorSceneManager.SaveScene(
				newScene,
				path
			);

			EditorSceneManager.CloseScene(
				newScene,
				true
			);

			AssetDatabase.Refresh();
			return newScene;
		}
	}
}
