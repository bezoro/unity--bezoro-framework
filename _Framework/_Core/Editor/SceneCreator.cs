using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace Bezoro.Core.Editor
{
	public static class SceneCreator
	{
		[MenuItem(MenuPath)]
		private static void Create()
		{
			var savePath =
				EditorUtility.SaveFilePanel("Choose a directory to save the Scene to:",
											Application.dataPath,
											NewSceneName,
											"unity");

			if (string.IsNullOrEmpty(savePath))
				return;

			CreateSceneAssetFromSelection(savePath);
		}

		private static Scene CreateSceneAssetFromSelection(string path)
		{
			var newScene =
				EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);

			var activeGameObject = Selection.activeGameObject;
			var newObject = Object.Instantiate(activeGameObject);
			newObject.name = activeGameObject.name;

			EditorSceneManager.SaveScene(newScene, path);
			EditorSceneManager.CloseScene(newScene, true);
			AssetDatabase.Refresh();
			return newScene;
		}

		[MenuItem(MenuPath, true)]
		private static bool Validate_Create() { return Selection.activeGameObject is not null; }

		private const string MenuPath = "Bezoro/Scene Creator/Create Scene From Selected Hierarchy";
		private const string NewSceneName = "NewScene";
	}
}