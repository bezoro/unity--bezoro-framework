using UnityEngine;

namespace Bezoro.Unity.Core.Editor.Common.Helpers;

/// <summary>
///     Converts between layer indices, names, and LayerMask values.
/// </summary>
public static class LayerMaskHelper
{
	public static void HideLayer(LayerMask lm)
	{
		UnityEditor.Tools.visibleLayers &= ~lm;
	}

	public static void HideLayer(string layerName)
	{
		int lm = LayerMask.GetMask(
			layerName
		);

		HideLayer(
			lm
		);
	}

	public static void ShowLayer(LayerMask lm)
	{
		UnityEditor.Tools.visibleLayers |= lm;
	}

	public static void ShowLayer(string layerName)
	{
		int lm = LayerMask.GetMask(
			layerName
		);

		ShowLayer(
			lm
		);
	}
}
