#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Bezoro.Unity.Core.Application;

/// <summary>
///     Tracks whether Unity is currently quitting, including editor play-mode resets.
/// </summary>
#if UNITY_EDITOR
[InitializeOnLoad]
#endif
internal static class ApplicationState
{
	private static bool _isInitialized;

	static ApplicationState()
	{
		Initialize();
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void InitializeRuntimeState()
	{
		_isInitialized = false;
		Initialize();
	}

	private static void Initialize()
	{
		if (_isInitialized) return;

		_isInitialized        = true;
		IsApplicationQuitting = false;

		UnityEngine.Application.quitting -= OnApplicationQuitting;
		UnityEngine.Application.quitting += OnApplicationQuitting;

#if UNITY_EDITOR
		EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
		EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#endif
	}

	public static bool IsApplicationQuitting { get; private set; }

	private static void OnApplicationQuitting() => IsApplicationQuitting = true;

#if UNITY_EDITOR
	private static void OnPlayModeStateChanged(PlayModeStateChange state)
	{
		if (state is PlayModeStateChange.ExitingPlayMode or PlayModeStateChange.EnteredEditMode)
			ResetApplicationQuittingState();
	}
#endif

	private static void ResetApplicationQuittingState() => IsApplicationQuitting = false;
}
