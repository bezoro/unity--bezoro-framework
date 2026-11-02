using UnityEngine;

namespace Bezoro.Unity.Core.Editor.Common.Extensions;

/// <summary>
///     Contains extension methods for Renderer.
/// </summary>
internal static class RendererExtensions
{
	public static Bounds ComputeWorldBounds(this Renderer[] renderers)
	{
		var bounds = new Bounds();

		if (renderers == null || renderers.Length <= 0) return bounds;

		var hasBounds = false;

		foreach (var renderer in renderers)
		{
			if (renderer == null) continue;

			if (!hasBounds)
			{
				bounds    = renderer.bounds;
				hasBounds = true;
				continue;
			}

			bounds.Encapsulate(renderer.bounds);
		}

		return bounds;
	}
}
