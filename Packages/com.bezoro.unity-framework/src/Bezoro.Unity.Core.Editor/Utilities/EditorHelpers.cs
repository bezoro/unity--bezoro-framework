using System.IO;
using System.Linq;
using Bezoro.Unity.Core.Scenes;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Bezoro.Unity.Core.Editor.Utilities
{
	/// <summary>
	///     Performs common Unity editor actions such as reopening projects and marking objects dirty.
	/// </summary>
	public static class EditorHelpers
	{
		public static bool Is_Editor_Idle() =>
			!EditorApplication.isCompiling &&
			!EditorApplication.isUpdating &&
			!EditorApplication.isPlayingOrWillChangePlaymode &&
			!EditorApplication.isPlaying &&
			!EditorApplication.isPaused;

		public static void Reopen_Project(bool save = true)
		{
			if (save)
			{
				var openScenes = SceneManager.GetAllLoadedScenes();
				bool savePromptResult = EditorSceneManager.SaveModifiedScenesIfUserWantsTo(
					openScenes.ToArray()
				);

				if (!savePromptResult)
				{
					bool confirmReopen = EditorUtility.DisplayDialog(
						"Reopen Project",
						"Unsaved changes will be lost. Do you still want to reopen the project?",
						"Yes",
						"No"
					);

					if (!confirmReopen) return;
				}
			}

			EditorApplication.OpenProject(
				Directory.GetCurrentDirectory()
			);
		}
	}
}
