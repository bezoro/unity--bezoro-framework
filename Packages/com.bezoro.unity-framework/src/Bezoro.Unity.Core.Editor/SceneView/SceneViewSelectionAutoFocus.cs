using System.Collections.Generic;
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
	private const  string                     ENABLED_KEY = "SceneViewSelectionAutoFocus.Enabled";
	private const  string                     FOCUS_3D_OBJECTS_KEY = "SceneViewSelectionAutoFocus.Focus3DObjects";
	private const  string                     FOCUS_CANVASES_UI_KEY = "SceneViewSelectionAutoFocus.FocusCanvasesUi";
	private const  string                     UI_LAYER_NAME = "UI";
	private static readonly Dictionary<UnityEditor.SceneView, (SceneViewCameraState camera, bool in2D)> FocusedViews = new();
	private static bool _revealedUiLayer;

	[InitializeOnLoadMethod]
	private static void Initialize()
	{
		Selection.selectionChanged -= OnSelectionChanged;
		Selection.selectionChanged += OnSelectionChanged;
		AssemblyReloadEvents.beforeAssemblyReload += RestoreViews;
	}

	public static bool Enabled
	{
		get => EditorPrefs.GetBool(
			ENABLED_KEY,
			true
		);
		set { EditorPrefs.SetBool(ENABLED_KEY, value); if (!value) RestoreViews(); }
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
		set { EditorPrefs.SetBool(FOCUS_CANVASES_UI_KEY, value); if (!value) RestoreViews(); }
	}

	private static void RestoreViews()
	{
		foreach (var view in new List<UnityEditor.SceneView>(FocusedViews.Keys))
			RestoreView(view);
	}

	private static void RestoreView(UnityEditor.SceneView view)
	{
		if (!FocusedViews.TryGetValue(view, out var state)) return;
		FocusedViews.Remove(view);
		if (view != null)
		{
			view.in2DMode = state.in2D;
			view.LookAt(state.camera.Pivot, state.camera.Rotation, state.camera.Size, state.camera.Orthographic, true);
			view.Repaint();
		}
		if (FocusedViews.Count == 0 && _revealedUiLayer)
		{
			LayerMaskHelper.HideLayer(UI_LAYER_NAME);
			_revealedUiLayer = false;
		}
	}

	private static void OnSelectionChanged()
	{
		var view = UnityEditor.SceneView.lastActiveSceneView;
		var selection = Selection.gameObjects;
		if (!Enabled || view == null || view.camera == null || !selection.AreValidSceneGOs())
		{
			RestoreViews();
			return;
		}
		if (!FocusCanvasesUi || !selection.AreAllRectTransforms()) RestoreViews();
		ApplySelection(view, selection, FocusCanvasesUi, Focus3DObjects);
	}

	internal static void ApplySelection(UnityEditor.SceneView view, GameObject[] selection, bool focusCanvas, bool focus3D)
	{
		bool focusUi = focusCanvas && selection.AreAllRectTransforms();
		if (!focusUi) RestoreView(view);
		if (focusUi)
		{
			if (!FocusedViews.ContainsKey(view))
				FocusedViews.Add(view, (SceneViewCameraState.From(view), view.in2DMode));
			if ((UnityEditor.Tools.visibleLayers & LayerMask.GetMask(UI_LAYER_NAME)) == 0)
			{
				LayerMaskHelper.ShowLayer(UI_LAYER_NAME);
				_revealedUiLayer = true;
			}
			if (selection.TryGetCameraFacingCanvasRotation(out var rotation))
			{
				view.in2DMode = false;
				view.rotation = rotation;
			}
			else view.in2DMode = true;
			view.orthographic = true;
		}

		if (SceneViewSelectionBoundsHelper.TryCalculateBoundsForFraming(selection, focusCanvas, focus3D, out var bounds))
			view.Frame(bounds, true);
	}
}
