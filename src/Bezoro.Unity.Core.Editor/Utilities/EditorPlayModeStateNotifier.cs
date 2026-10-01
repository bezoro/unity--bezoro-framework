using System;
using Bezoro.Core.Extensions;
using UnityEditor;
using UnityEngine;

namespace Bezoro.Unity.Core.Editor.Utilities
{
	/// <summary>
	///     Notifies subscribers about Unity Editor play mode state changes
	/// </summary>
	[InitializeOnLoad]
	public static class EditorPlayModeStateNotifier
	{
		public static event Action<PlayModeStateChange>? PlayModeStateChanged;

		static EditorPlayModeStateNotifier()
		{
			EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
			UnityEngine.Application.quitting       -= OnApplicationQuit;

			EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
			UnityEngine.Application.quitting       += OnApplicationQuit;
		}

		public static PlayModeStateChange PlayModeState { get; private set; }

		private static void OnApplicationQuit() =>
			Debug.Log("××× Application Quit ×××".Bold());

		private static void OnPlayModeStateChanged(PlayModeStateChange playModeStateChange)
		{
			Debug.Log($"=== {playModeStateChange} ===".LightGray().Bold());

			PlayModeState = playModeStateChange;
			PlayModeStateChanged?.Invoke(playModeStateChange);
		}
	}
}
