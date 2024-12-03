using System;
using UnityEditor;

namespace Bezoro.Framework.Unity.Editor
{
	/// <summary>
	///     Notifies subscribers about Unity Editor play mode state changes
	/// </summary>
	[InitializeOnLoad]
	public static class EditorPlayModeStateNotifier
	{
		private static bool _initialized;

		static EditorPlayModeStateNotifier()
		{
			if ( _initialized ) { return; }

			EditorApplication.playModeStateChanged += OnPlayModeChanged;
			_initialized                           =  true;
		}

		public static PlayModeStateChange               PlayModeState { get; private set; }
		public static event Action<PlayModeStateChange> PlayModeStateChanged;

		private static void OnPlayModeChanged( PlayModeStateChange playModeStateChange )
		{
			PlayModeState = playModeStateChange;
			PlayModeStateChanged?.Invoke( playModeStateChange );
		}

		public static void Cleanup()
		{
			EditorApplication.playModeStateChanged -= OnPlayModeChanged;
			_initialized                           =  false;
		}
	}
}
