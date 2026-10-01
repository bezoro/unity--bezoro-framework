using UnityEngine;

namespace Bezoro.Unity.Core.Common.Consts;

/// <summary>
///     Defines constants for Material.
/// </summary>
public static class MaterialConstants
{
	public static int EmissionColor => Shader.PropertyToID(
		"_EmissionColor"
	);

	public static int MaterialColorID => Shader.PropertyToID(
		"_Color"
	);
}
