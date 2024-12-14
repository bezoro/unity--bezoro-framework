using Bezoro.Framework.Unity.Utilities;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Bezoro.Framework.Unity.Editor.Tools
{
	public static class Scene_Creator
	{
		private const string _MENU_PATH      = "Bezoro/Scene Creator/Create Scene From Selected Hierarchy";
		private const string _NEW_SCENE_NAME = "NewScene";

		[MenuItem(_MENU_PATH)]
		private static void Create()
		{
			var save_path = EditorUtility.SaveFilePanel(
				"Choose a directory to save the Scene to:", Application.dataPath, _NEW_SCENE_NAME, "unity"
			);

			if (save_path.Is_Null_Or_Empty()) return;

			Create_Scene_Asset_From_Selection(save_path);
		}

		private static Scene Create_Scene_Asset_From_Selection(string path)
		{
			var new_scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);

			var active_game_object = Selection.activeGameObject;
			var new_object         = Object.Instantiate(active_game_object);

			new_object.name = active_game_object.name;

			EditorSceneManager.SaveScene(new_scene, path);
			EditorSceneManager.CloseScene(new_scene, true);
			AssetDatabase.Refresh();
			return new_scene;
		}

		[MenuItem(_MENU_PATH, true)]
		private static bool Validate_Create()
		{
			return Selection.activeGameObject is not null;
		}
	}
}
