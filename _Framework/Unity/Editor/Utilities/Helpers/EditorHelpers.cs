using System.IO;
using UnityEditor;

namespace Bezoro.Framework.Unity.Editor.Utilities.Helpers
{
	public static class EditorHelpers
	{
		public static void ReopenProject()
		{
			EditorApplication.OpenProject(Directory.GetCurrentDirectory());
		}

		public static bool IsEditorIdle()
		{
			return !EditorApplication.isCompiling
				&& !EditorApplication.isUpdating
				&& !EditorApplication.isPlayingOrWillChangePlaymode
				&& !EditorApplication.isPlaying
				&& !EditorApplication.isPaused;
		}
	}
}
