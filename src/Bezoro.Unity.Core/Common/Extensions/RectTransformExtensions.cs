using System.Collections.Generic;
using UnityEngine;

namespace Bezoro.Unity.Core.Common.Extensions;

/// <summary>
///     Contains extension methods for Rect Transform.
/// </summary>
public static class RectTransformExtensions
{
	/* ────────────────────────────────────────────────────────────
	 * Helpers
	 * ────────────────────────────────────────────────────────────*/
	// Shared 4-vector buffer (safe: methods are synchronous)
	private static readonly Vector3[] _corners = new Vector3[4];

	/* ────────────────────────────────────────────────────────────
	 * Single RectTransform
	 * ────────────────────────────────────────────────────────────*/
	public static Bounds ComputeWorldBounds(this RectTransform rt)
	{
		if (!rt)
			return new(
				Vector3.zero,
				Vector3.zero
			);

		// Re-use a static buffer to avoid per-call allocations
		rt.GetWorldCorners(
			_corners
		);

		var bounds = new Bounds(
			_corners[0],
			Vector3.zero
		);

		for (var i = 1; i < 4; i++)
		{
			bounds.Encapsulate(
				_corners[i]
			);
		}

		return bounds;
	}

	/* ────────────────────────────────────────────────────────────
	 * Multiple RectTransforms (array)
	 * ───────────────────────────────────────────────*/
	public static Bounds ComputeWorldBounds(this RectTransform[] rts)
	{
		if (rts == null || rts.Length == 0)
			return new(
				Vector3.zero,
				Vector3.zero
			);

		// Initialise bounds with the first non-null RectTransform
		var firstIdx = 0;
		while (firstIdx < rts.Length && !rts[firstIdx]) firstIdx++;

		if (firstIdx == rts.Length)
			return new(
				Vector3.zero,
				Vector3.zero
			);

		var bounds = rts[firstIdx].ComputeWorldBounds();

		for (int i = firstIdx + 1; i < rts.Length; i++)
		{
			if (!rts[i]) continue;

			bounds.Encapsulate(
				rts[i].ComputeWorldBounds()
			);
		}

		return bounds;
	}

	/* ────────────────────────────────────────────────────────────
	 * IEnumerable convenience overload
	 * ────────────────────────────────────────────────────────────*/
	public static Bounds ComputeWorldBounds(this IEnumerable<RectTransform> rts)
	{
		if (rts == null)
			return new(
				Vector3.zero,
				Vector3.zero
			);

		var    hasInit = false;
		Bounds bounds  = default;

		foreach (var rt in rts)
		{
			if (!rt) continue;

			if (!hasInit)
			{
				bounds  = rt.ComputeWorldBounds();
				hasInit = true;
			}
			else
			{
				bounds.Encapsulate(
					rt.ComputeWorldBounds()
				);
			}
		}

		return bounds;
	}
}
