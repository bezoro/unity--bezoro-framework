using UnityEngine;
using UnityEditorSceneView = UnityEditor.SceneView;

namespace Bezoro.Unity.Core.Editor.Common.Helpers;

/// <summary>
///     Stores the Scene view camera values commonly modified by editor focus tools.
/// </summary>
internal readonly struct SceneViewCameraState
{
	public SceneViewCameraState(Vector3 pivot, Quaternion rotation, float size, bool orthographic)
	{
		Pivot        = pivot;
		Rotation     = rotation;
		Size         = size;
		Orthographic = orthographic;
	}

	public bool       Orthographic { get; }
	public float      Size         { get; }
	public Quaternion Rotation     { get; }

	public Vector3 Pivot { get; }

	public static SceneViewCameraState From(UnityEditorSceneView sceneView) =>
		new(
			sceneView.pivot,
			sceneView.rotation,
			sceneView.size,
			sceneView.orthographic
		);
}
