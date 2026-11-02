using System.Collections.Generic;
using System.Linq;
using Bezoro.Unity.Core.Editor.Common.Extensions;
using UnityEngine;
using RectTransformExtensions = Bezoro.Unity.Core.Common.Extensions.RectTransformExtensions;

namespace Bezoro.Unity.Core.Editor.Common.Helpers;

/// <summary>
///     Computes Scene view framing bounds from mixed UI and renderer selections.
/// </summary>
internal static class SceneViewSelectionBoundsHelper
{
	private const float CANVAS_ROOT_BOUNDS_SIZE_MULTIPLIER = .5f;
	private const float RECT_BOUNDS_SIZE_MULTIPLIER        = .75f;
	private const float RENDERER_BOUNDS_SIZE_MULTIPLIER    = .9f;

	public static bool TryCalculateBoundsForFraming(
		IEnumerable<GameObject> selection,
		bool                    includeRectTransforms,
		bool                    includeRenderers,
		out Bounds              bounds
	)
	{
		bounds = default;

		var rects     = new List<RectTransform>();
		var renderers = new List<Renderer>();

		foreach (var go in selection)
		{
			if (go.TryGetComponent(out RectTransform rect) && includeRectTransforms)
				rects.Add(rect);
			else if (includeRenderers)
				renderers.AddRange(go.GetComponentsInChildren<Renderer>());
		}

		if (rects.Count == 0 && renderers.Count == 0)
			return false;

		bounds = CalculateBoundsForFraming(rects, renderers);
		return true;
	}

	private static Bounds CalculateBoundsForFraming(ICollection<RectTransform> rects, ICollection<Renderer> renderers)
	{
		var rectsWorldBounds = RectTransformExtensions.ComputeWorldBounds(
			rects.ToArray()
		);

		bool anyFullStretch = rects.Any(r => r.IsImmediateChildOfRoot() &&
											 r.anchorMin == Vector2.zero &&
											 r.anchorMax == Vector2.one
		);

		if (rects.ContainsRootCanvas() || anyFullStretch)
			rectsWorldBounds.size *= CANVAS_ROOT_BOUNDS_SIZE_MULTIPLIER;
		else
			rectsWorldBounds.size *= RECT_BOUNDS_SIZE_MULTIPLIER;

		var rendererWorldBounds = renderers.ToArray().ComputeWorldBounds();
		rendererWorldBounds.size *= RENDERER_BOUNDS_SIZE_MULTIPLIER;

		return rectsWorldBounds.size != Vector3.zero ? rectsWorldBounds : rendererWorldBounds;
	}
}
