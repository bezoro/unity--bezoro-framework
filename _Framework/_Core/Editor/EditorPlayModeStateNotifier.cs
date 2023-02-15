using UnityEditor;

namespace Bezoro.Core.Editor
{
	[InitializeOnLoad]
	public static class EditorPlayModeStateNotifier
	{
		static EditorPlayModeStateNotifier()
		{
			if (Initialized)
				return;

			EditorApplication.playModeStateChanged += OnPlayModeChanged;
			Initialized = true;
		}

		public static PlayModeStateChange PlayModeState { get; private set; }

		private static void OnPlayModeChanged(PlayModeStateChange playModeStateChange) =>
			PlayModeState = playModeStateChange;

		private static readonly bool Initialized;
	}
}