using System;
using Bezoro.Unity.Core.Common.Extensions;
using UnityEditor;
using UnityEngine;
using UnityEditorSceneView = UnityEditor.SceneView;

namespace Bezoro.Unity.Core.Editor.Common.Extensions
{
	/// <summary>
	///     Editor-only helpers for applying SceneView camera changes after Unity has
	///     completed the current editor event. Delayed execution avoids operating on
	///     disposed SceneView instances during window focus, overlay, and selection
	///     callbacks.
	/// </summary>
	public static class SceneViewExtensions
	{
		/// <summary>
		///     Gets Unity's last active SceneView when one still exists.
		/// </summary>
		public static bool TryGetLastActiveSceneView(out UnityEditorSceneView? sceneView)
		{
			sceneView = UnityEditorSceneView.lastActiveSceneView;
			return !sceneView.IsUnityNull();
		}

		/// <summary>
		///     Frames bounds on the next editor delay callback and repaints the view
		///     when the SceneView is still alive.
		/// </summary>
		public static void FrameDelayed(this UnityEditorSceneView sceneView, Bounds bounds, bool instant = false) =>
			sceneView.DelayIfValid(validSceneView =>
				{
					validSceneView.Frame(bounds, instant);
					validSceneView.Repaint();
				}
			);

		/// <summary>
		///     Frames the current selection on the next editor delay callback.
		/// </summary>
		public static void FrameSelectedDelayed(this UnityEditorSceneView sceneView) =>
			sceneView.DelayIfValid(validSceneView =>
				{
					validSceneView.FrameSelected();
					validSceneView.Repaint();
				}
			);

		/// <summary>
		///     Moves the SceneView pivot on the next editor delay callback.
		/// </summary>
		public static void LookAtDelayed(this UnityEditorSceneView sceneView, Vector3 pivot) =>
			sceneView.DelayIfValid(validSceneView =>
				{
					validSceneView.LookAt(pivot);
					validSceneView.Repaint();
				}
			);

		/// <summary>
		///     Orients the SceneView to look straight at a rotation on the next editor delay callback.
		/// </summary>
		public static void OrientToRotationDelayed(this UnityEditorSceneView sceneView, Quaternion rotation) =>
			sceneView.DelayIfValid(validSceneView =>
				{
					validSceneView.in2DMode     = false;
					validSceneView.rotation     = rotation;
					validSceneView.orthographic = true;
					validSceneView.Repaint();
				}
			);

		/// <summary>
		///     Repaints the SceneView on the next editor delay callback.
		/// </summary>
		public static void RepaintDelayed(this UnityEditorSceneView sceneView) =>
			sceneView.DelayIfValid(validSceneView => validSceneView.Repaint());

		/// <summary>
		///     Immediately sets SceneView 2D mode and repaints the view. This does
		///     not change the camera projection by itself.
		/// </summary>
		public static void Set2DMode(this UnityEditorSceneView sceneView, bool enabled)
		{
			if (sceneView.IsUnityNull()) return;

			sceneView.in2DMode = enabled;
			sceneView.Repaint();
		}

		/// <summary>
		///     Switches SceneView 2D mode on the next editor delay callback. Enabling
		///     2D mode also forces the backing camera to orthographic when available.
		/// </summary>
		public static void Set2DViewDelayed(this UnityEditorSceneView sceneView, bool enabled) =>
			sceneView.DelayIfValid(validSceneView =>
				{
					validSceneView.Set2DMode(enabled);

					if (!validSceneView.camera.IsUnityNull())
						validSceneView.SetOrthographic(enabled);

					validSceneView.Repaint();
				}
			);

		/// <summary>
		///     Immediately sets the backing SceneView camera projection and repaints
		///     the view.
		/// </summary>
		public static void SetOrthographic(this UnityEditorSceneView sceneView, bool enabled)
		{
			if (sceneView.IsUnityNull()) return;
			if (sceneView.camera.IsUnityNull()) return;

			sceneView.camera.orthographic = enabled;
			sceneView.Repaint();
		}

		/// <summary>
		///     Enables SceneView 2D mode.
		/// </summary>
		public static void SwitchTo2DView(this UnityEditorSceneView sceneView) =>
			sceneView.Set2DViewDelayed(true);

		/// <summary>
		///     Disables SceneView 2D mode.
		/// </summary>
		public static void SwitchTo3DView(this UnityEditorSceneView sceneView) =>
			sceneView.Set2DViewDelayed(false);

		/// <summary>
		///     Switches the SceneView camera to orthographic projection.
		/// </summary>
		public static void SwitchToOrthographic(this UnityEditorSceneView sceneView) =>
			sceneView.SetOrthographic(true);

		/// <summary>
		///     Switches the SceneView camera to perspective projection.
		/// </summary>
		public static void SwitchToPerspective(this UnityEditorSceneView sceneView) =>
			sceneView.SetOrthographic(false);

		private static void DelayIfValid(this UnityEditorSceneView sceneView, Action<UnityEditorSceneView> action)
		{
			EditorApplication.delayCall += () =>
			{
				if (sceneView.IsUnityNull()) return;

				action(sceneView);
			};
		}
	}
}
