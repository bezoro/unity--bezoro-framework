using System.Collections.Generic;
using System.Linq;
using Bezoro.Unity.Core.Common.Extensions;
using UnityEngine;

namespace Bezoro.Unity.Core.Editor.Common.Extensions;

/// <summary>
///     Provides classification helpers for Scene view selections.
/// </summary>
internal static class SceneViewSelectionExtensions
{
	public static bool AnyIsRectTransform(this ICollection<GameObject> selection) =>
		selection.Any(go => !go.IsUnityNull() && go.TryGetComponent<RectTransform>(out _));

	public static bool AreAllRectTransforms(this ICollection<GameObject> selection) =>
		selection.Count > 0 && selection.AllHaveComponentOfType<RectTransform>();

	public static bool AreAllTransforms(this ICollection<GameObject> selection) =>
		selection.Count > 0 && selection.AllHaveComponentOfType<Transform>();

	public static bool TryGetCameraFacingCanvasRotation(this IEnumerable<GameObject> selection, out Quaternion rotation)
	{
		rotation = default;

		var cameraFacingCanvases = selection
								   .Select(go => go.GetComponentInParent<Canvas>())
								   .Where(canvas => canvas != null &&
													(canvas.renderMode == RenderMode.WorldSpace ||
													 canvas.renderMode == RenderMode.ScreenSpaceCamera)
								   )
								   .Distinct()
								   .ToArray();

		if (cameraFacingCanvases.Length == 0)
			return false;

		var forward = cameraFacingCanvases
					  .Select(canvas => canvas.transform.forward)
					  .Aggregate(Vector3.zero, (current, forward) => current + forward);

		var up = cameraFacingCanvases
				 .Select(canvas => canvas.transform.up)
				 .Aggregate(Vector3.zero, (current, up) => current + up);

		if (forward == Vector3.zero || up == Vector3.zero)
			return false;

		rotation = Quaternion.LookRotation(
			forward.normalized,
			up.normalized
		);

		return true;
	}

	public static bool WasRectTransformSelectionDeselected(
		this ICollection<GameObject> previousSelection,
		ICollection<GameObject>      currentSelection
	) =>
		previousSelection.Count > 0 &&
		currentSelection.Count <= 0 &&
		previousSelection.AllHaveComponentOfType<RectTransform>();
}
