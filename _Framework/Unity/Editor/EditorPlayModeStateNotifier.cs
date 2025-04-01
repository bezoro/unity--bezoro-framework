using System;
using UnityEditor;
using UnityEngine;

namespace Bezoro.Framework.Unity.Editor
{
	/// <summary>
	///     Notifies subscribers about Unity Editor play mode state changes
	/// </summary>
	[InitializeOnLoad]
	public static class EditorPlayModeStateNotifier
	{
		static EditorPlayModeStateNotifier()
		{
			EditorApplication.playModeStateChanged    += OnPlayModeChanged;
			AssemblyReloadEvents.beforeAssemblyReload += Cleanup;
		}

		public static PlayModeStateChange               PlayModeState { get; private set; }
		public static event Action<PlayModeStateChange> PlayModeStateChanged;

		private static void OnPlayModeChanged(PlayModeStateChange playModeStateChange)
		{
			PlayModeState = playModeStateChange;
			PlayModeStateChanged?.Invoke(playModeStateChange);

			Logger.Log(PlayModeState.ToString(), nameof(EditorPlayModeStateNotifier));
		}

		public static void Cleanup()
		{
			EditorApplication.playModeStateChanged    -= OnPlayModeChanged;
			AssemblyReloadEvents.beforeAssemblyReload -= Cleanup;
		}
	}
}
