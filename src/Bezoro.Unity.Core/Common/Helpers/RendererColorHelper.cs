using UnityEngine;

namespace Bezoro.Unity.Core.Common.Helpers;

/// <summary>
///     Reads and applies material colors on Renderer components.
/// </summary>
public static class RendererColorHelper
{
	public static readonly int BaseColorProperty     = Shader.PropertyToID("_BaseColor");
	public static readonly int Color0Property        = Shader.PropertyToID("_Color0");
	public static readonly int ColorProperty         = Shader.PropertyToID("_Color");
	public static readonly int EmissionColorProperty = Shader.PropertyToID("_EmissionColor");
	public static readonly int FaceColorProperty     = Shader.PropertyToID("_FaceColor");
	public static readonly int OutlineColorProperty  = Shader.PropertyToID("_OutlineColor");
	public static readonly int TintColorProperty     = Shader.PropertyToID("_TintColor");

	private static readonly int[] BaseColorProperties =
	{
		BaseColorProperty,
		ColorProperty
	};

	public static void Clear(Renderer renderer, MaterialPropertyBlock propertyBlock)
	{
		propertyBlock.Clear();
		renderer.SetPropertyBlock(propertyBlock);
	}

	public static void SetBaseColor(Renderer renderer, MaterialPropertyBlock propertyBlock, Color color)
	{
		SetColorProperties(renderer, propertyBlock, BaseColorProperties, color);
	}

	public static void SetColor0(Renderer renderer, MaterialPropertyBlock propertyBlock, Color color)
	{
		SetColorProperty(renderer, propertyBlock, Color0Property, color);
	}

	public static void SetEmissionColor(Renderer renderer, MaterialPropertyBlock propertyBlock, Color color)
	{
		SetEmissiveColor(renderer, propertyBlock, color);
	}

	public static void SetEmissiveColor(Renderer renderer, MaterialPropertyBlock propertyBlock, Color color)
	{
		SetColorProperty(renderer, propertyBlock, EmissionColorProperty, color);
	}

	public static void SetFaceColor(Renderer renderer, MaterialPropertyBlock propertyBlock, Color color)
	{
		SetColorProperty(renderer, propertyBlock, FaceColorProperty, color);
	}

	public static void SetOutlineColor(Renderer renderer, MaterialPropertyBlock propertyBlock, Color color)
	{
		SetColorProperty(renderer, propertyBlock, OutlineColorProperty, color);
	}

	public static void SetTintColor(Renderer renderer, MaterialPropertyBlock propertyBlock, Color color)
	{
		SetColorProperty(renderer, propertyBlock, TintColorProperty, color);
	}

	private static void SetColorProperties(
		Renderer              renderer,
		MaterialPropertyBlock propertyBlock,
		int[]                 properties,
		Color                 color)
	{
		renderer.GetPropertyBlock(propertyBlock);

		for (var i = 0; i < properties.Length; i++)
			propertyBlock.SetColor(properties[i], color);

		renderer.SetPropertyBlock(propertyBlock);
	}

	private static void SetColorProperty(
		Renderer              renderer,
		MaterialPropertyBlock propertyBlock,
		int                   property,
		Color                 color)
	{
		renderer.GetPropertyBlock(propertyBlock);
		propertyBlock.SetColor(property, color);
		renderer.SetPropertyBlock(propertyBlock);
	}
}
