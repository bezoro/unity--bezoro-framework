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
			CreateSceneAssetFromSelection();

			const string alreadyOpenScenePath = "Assets/" +
												NewScenePath + "/" +
												NewSceneName + ".unity";

			if (!IsSceneAlreadyOpen(alreadyOpenScenePath,
									out var alreadyOpenScene,
									out var alreadyOpenSceneIndex))
				return;

			UpdateAlreadyOpenScene(alreadyOpenScene, alreadyOpenScenePath, alreadyOpenSceneIndex);
		}

		[MenuItem(MenuPath, true)]
		private static bool Validate_Create() { return Selection.activeGameObject is not null; }

		private static Scene CreateSceneAssetFromSelection()
		{
			var newScene =
				EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);

			var activeGameObject = Selection.activeGameObject;
			var newObject = Object.Instantiate(activeGameObject);
			newObject.name = activeGameObject.name;

			var fullPath = Application.dataPath + "/" +
						   NewScenePath + "/" +
						   NewSceneName +
						   ".unity";

			EditorSceneManager.SaveScene(newScene, fullPath);
			EditorSceneManager.CloseScene(newScene, true);

			return newScene;
		}

		private static bool IsSceneAlreadyOpen(string path, out Scene scene, out int index)
		{
			for (var i = 0; i < SceneManager.sceneCount; i++)
			{
				var sc = SceneManager.GetSceneAt(i);
				if (sc.path != path)
					continue;

				scene = sc;
				index = i;
				return true;
			}

			scene = default;
			index = -1;
			return false;
		}

		private static void UpdateAlreadyOpenScene(Scene scene, string path, int index)
		{
			if (SceneManager.sceneCount == 1)
			{
				var emptyScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,
															 NewSceneMode.Additive);

				EditorSceneManager.CloseScene(scene, true);
				var openScene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
				EditorSceneManager.SaveScene(openScene);
				EditorSceneManager.CloseScene(emptyScene, true);
			}

			else if (index == SceneManager.sceneCount)
			{
				EditorSceneManager.CloseScene(scene, true);
				var openScene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
				EditorSceneManager.SaveScene(openScene);
			}
			else
			{
				EditorSceneManager.CloseScene(scene, true);
				var openScene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
				var relativeScene = SceneManager.GetSceneAt(index);
				EditorSceneManager.MoveSceneBefore(openScene, relativeScene);
				EditorSceneManager.SaveScene(openScene);
			}
		}

		private const string MenuPath = "Bezoro/Scene Creator/Create Scene From Selected Hierarchy";
		private const string NewScenePath = "_Project";
		private const string NewSceneName = "NewScene";
	}
}