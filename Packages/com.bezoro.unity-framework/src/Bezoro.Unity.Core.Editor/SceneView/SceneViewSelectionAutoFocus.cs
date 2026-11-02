using Bezoro.Unity.Core.Editor.Common.Extensions;
using Bezoro.Unity.Core.Editor.Common.Helpers;
using UnityEditor;
using UnityEngine;

namespace Bezoro.Unity.Core.Editor.SceneView;

/// <summary>
///     Automatically adjusts the Scene view when selecting canvases, UI elements, and 3D scene objects.
/// </summary>
/// <remarks>
///     UI selections are framed in a canvas-friendly orthographic view, camera-facing canvases are viewed straight-on,
///     and deselecting UI restores the last non-UI Scene view camera state.
/// </remarks>
internal static class SceneViewSelectionAutoFocus
{
	private const  float                      RESTORE_TRANSITION_DURATION = .25f;
	private const  string                     ENABLED_KEY = "SceneViewSelectionAutoFocus.Enabled";
	private const  string                     FOCUS_3D_OBJECTS_KEY = "SceneViewSelectionAutoFocus.Focus3DObjects";
	private const  string                     FOCUS_CANVASES_UI_KEY = "SceneViewSelectionAutoFocus.FocusCanvasesUi";
	private const  string                     UI_LAYER_NAME = "UI";
	private static EditorSceneViewTransition? _restoreTransition;

	private static GameObject[]          _pastSelection = { };
	private static SceneViewCameraState? _lastNonUiSceneViewCameraState;

	[InitializeOnLoadMethod]
	private static void Initialize()
	{
		Selection.selectionChanged -= OnSelectionChanged;
		Selection.selectionChanged += OnSelectionChanged;
	}

	public static bool Enabled
	{
		get => EditorPrefs.GetBool(
			ENABLED_KEY,
			true
		);
		set => EditorPrefs.SetBool(
			ENABLED_KEY,
			value
		);
	}

	/// <summary>
	///     Gets or sets whether non-UI scene object selections should trigger renderer framing.
	/// </summary>
	public static bool Focus3DObjects
	{
		get => EditorPrefs.GetBool(
			FOCUS_3D_OBJECTS_KEY,
			true
		);
		set => EditorPrefs.SetBool(
			FOCUS_3D_OBJECTS_KEY,
			value
		);
	}

	/// <summary>
	///     Gets or sets whether RectTransform and Canvas selections should trigger canvas-aware framing.
	/// </summary>
	public static bool FocusCanvasesUi
	{
		get => EditorPrefs.GetBool(
			FOCUS_CANVASES_UI_KEY,
			true
		);
		set => EditorPrefs.SetBool(
			FOCUS_CANVASES_UI_KEY,
			value
		);
	}

	private static void ApplySceneViewModeForSelection(UnityEditor.SceneView sceneView, GameObject[] currentSelection)
	{
		// Canvas/UI focus temporarily takes over the Scene view, so deselecting restores the previous non-UI state.
		if (Focus3DObjects && _pastSelection.WasRectTransformSelectionDeselected(currentSelection))
		{
			RestoreLastNonUiSceneViewCameraState(
				sceneView
			);

			LayerMaskHelper.HideLayer(
				UI_LAYER_NAME
			);

			return;
		}

		// World space and screen space camera canvases keep their authored facing direction; overlay canvases use 2D.
		if (FocusCanvasesUi && currentSelection.AreAllRectTransforms())
		{
			StoreLastNonUiSceneViewCameraState(
				sceneView
			);

			if (currentSelection.TryGetCameraFacingCanvasRotation(out var rotation))
				sceneView.OrientToRotationDelayed(
					rotation
				);
			else
				sceneView.SwitchTo2DView();

			LayerMaskHelper.ShowLayer(
				UI_LAYER_NAME
			);

			return;
		}

		// RectTransforms are Transforms too, so keep this path limited to non-UI scene objects.
		if (Focus3DObjects && currentSelection.AreAllTransforms() && !currentSelection.AnyIsRectTransform())
		{
			sceneView.SwitchToPerspective();

			if (_pastSelection.AllHaveComponentOfType<RectTransform>())
			{
				sceneView.SwitchTo3DView();
				LayerMaskHelper.HideLayer(
					UI_LAYER_NAME
				);
			}
		}
	}

	/// <summary>
	///     Frames the active Scene view around selected UI rects or renderer bounds.
	/// </summary>
	private static void FrameSelection(UnityEditor.SceneView sceneView, GameObject[] selection)
	{
		if (!SceneViewSelectionBoundsHelper.TryCalculateBoundsForFraming(
				selection,
				FocusCanvasesUi,
				Focus3DObjects,
				out var bounds
			))
			return;

		sceneView.FrameDelayed(
			bounds
		);
	}

	private static void OnSelectionChanged()
	{
		if (!Enabled) return;
		if (UnityEditor.SceneView.lastActiveSceneView is not { } sceneView) return;
		if (sceneView.camera == null) return;

		GameObject[] currentSelection = Selection.gameObjects;

		if (!currentSelection.AreValidSceneGOs()) return;
		if (_pastSelection == currentSelection) return;

		ApplySceneViewModeForSelection(
			sceneView,
			currentSelection
		);

		FrameSelection(
			sceneView,
			currentSelection
		);

		_pastSelection = currentSelection;
	}

	/// <summary>
	///     Smoothly restores the most recent non-UI Scene view camera state after leaving canvas focus.
	/// </summary>
	private static void RestoreLastNonUiSceneViewCameraState(UnityEditor.SceneView sceneView)
	{
		if (_lastNonUiSceneViewCameraState is not { } cameraState)
		{
			sceneView.SwitchTo3DView();
			return;
		}

		EditorApplication.delayCall += () =>
		{
			if (sceneView == null) return;

			_restoreTransition?.Stop();
			_restoreTransition = new(
				sceneView,
				SceneViewCameraState.From(
					sceneView
				),
				cameraState,
				RESTORE_TRANSITION_DURATION,
				() => _restoreTransition = null
			);

			_restoreTransition.Start();
		};
	}

	/// <summary>
	///     Captures the current Scene view camera state before UI focus changes it.
	/// </summary>
	private static void StoreLastNonUiSceneViewCameraState(UnityEditor.SceneView sceneView)
	{
		if (sceneView.in2DMode)
			return;

		if (_pastSelection.Length > 0 && _pastSelection.AllHaveComponentOfType<RectTransform>())
			return;

		_lastNonUiSceneViewCameraState = SceneViewCameraState.From(
			sceneView
		);
	}
}
