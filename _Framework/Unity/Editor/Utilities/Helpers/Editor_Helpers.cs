using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Bezoro.Framework.Unity.Editor.Utilities
{
	public static class Editor_Helpers
	{
		public static void Reopen_Project(bool save = true)
		{
			if (save)
			{
				var open_scenes        = SceneManager.Get_All_Loaded_Scenes();
				var save_prompt_result = EditorSceneManager.SaveModifiedScenesIfUserWantsTo(open_scenes.ToArray());

				if (!save_prompt_result)
				{
					var confirm_reopen = EditorUtility.DisplayDialog(
						"Reopen Project",
						"Unsaved changes will be lost. Do you still want to reopen the project?",
						"Yes",
						"No"
					);

					if (!confirm_reopen) return;
				}
			}

			EditorApplication.OpenProject(Directory.GetCurrentDirectory());
		}

		public static bool Is_Editor_Idle()
		{
			return !EditorApplication.isCompiling
				   && !EditorApplication.isUpdating
				   && !EditorApplication.isPlayingOrWillChangePlaymode
				   && !EditorApplication.isPlaying
				   && !EditorApplication.isPaused;
		}
	}
}
