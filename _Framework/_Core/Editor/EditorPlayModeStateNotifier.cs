using UnityEditor;

namespace Bezoro.Core.Editor
{
	[InitializeOnLoad]
	public static class EditorPlayModeStateNotifier
	{
		public static PlayModeStateChange PlayModeState { get; private set; }

		static EditorPlayModeStateNotifier() =>
			EditorApplication.playModeStateChanged += OnPlayModeChanged;

		private static void OnPlayModeChanged(PlayModeStateChange playModeStateChange) =>
			PlayModeState = playModeStateChange;
	}
}