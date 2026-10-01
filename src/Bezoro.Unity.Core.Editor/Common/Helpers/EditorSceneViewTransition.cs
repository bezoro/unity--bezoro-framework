using System;
using UnityEditor;
using UnityEngine;
using UnityEditorSceneView = UnityEditor.SceneView;

namespace Bezoro.Unity.Core.Editor.Common.Helpers;

/// <summary>
///     Interpolates Scene view camera state over editor update ticks.
/// </summary>
internal sealed class EditorSceneViewTransition
{
	private readonly Action               _onCompleted;
	private readonly double               _startTime;
	private readonly float                _duration;
	private readonly SceneViewCameraState _from;
	private readonly SceneViewCameraState _to;
	private readonly UnityEditorSceneView _sceneView;

	public EditorSceneViewTransition(
		UnityEditorSceneView sceneView,
		SceneViewCameraState from,
		SceneViewCameraState to,
		float                duration,
		Action               onCompleted
	)
	{
		_sceneView   = sceneView;
		_from        = from;
		_to          = to;
		_duration    = duration;
		_onCompleted = onCompleted;
		_startTime   = EditorApplication.timeSinceStartup;
	}

	public void Start()
	{
		EditorApplication.update += Update;
		Update();
	}

	public void Stop()
	{
		EditorApplication.update -= Update;
	}

	private void Complete()
	{
		Stop();
		_onCompleted();
	}

	private void Update()
	{
		if (_sceneView == null)
		{
			Complete();
			return;
		}

		var   elapsed = (float)(EditorApplication.timeSinceStartup - _startTime);
		float t       = Mathf.Clamp01(elapsed / _duration);
		float eased   = Mathf.SmoothStep(0f, 1f, t);

		_sceneView.in2DMode     = false;
		_sceneView.pivot        = Vector3.Lerp(_from.Pivot, _to.Pivot, eased);
		_sceneView.rotation     = Quaternion.Slerp(_from.Rotation, _to.Rotation, eased);
		_sceneView.size         = Mathf.Lerp(_from.Size, _to.Size, eased);
		_sceneView.orthographic = t >= 1f ? _to.Orthographic : _from.Orthographic;
		_sceneView.Repaint();

		if (t >= 1f)
			Complete();
	}
}
