using UnityEngine;

namespace Bezoro.Unity.Core.Common.Primitives;

/// <summary>
///     Represents Colour data.
/// </summary>
public readonly struct Colour
{
	private readonly float _hue;
	private readonly float _lightness;
	private readonly float _saturation;

	public Colour(float h, float s, float l)
	{
		_hue = Mathf.Clamp(
			h,
			0f,
			360f
		);

		_saturation = Mathf.Clamp(
			s,
			0f,
			100f
		);

		_lightness = Mathf.Clamp(
			l,
			0f,
			100f
		);
	}

	public UnityEngine.Color RGB => HSL_To_RGB(
		_hue,
		_saturation,
		_lightness
	);

	public string Hex
	{
		get
		{
			var color = RGB;
			return $"#{(int)(color.r * 255):X2}{(int)(color.g * 255):X2}{(int)(color.b * 255):X2}";
		}
	}

	public static implicit operator UnityEngine.Color(Colour colour) =>
		colour.RGB;

	private UnityEngine.Color HSL_To_RGB(float hue, float saturation, float lightness)
	{
		hue        /= 360f;
		saturation /= 100f;
		lightness  /= 100f;

		float q = lightness < 0.5f
					  ? lightness * (1f + saturation)
					  : lightness + saturation - lightness * saturation;

		float p = 2f * lightness - q;
		float r = HueToRGB(
			p,
			q,
			hue + 1f / 3f
		);

		float g = HueToRGB(
			p,
			q,
			hue
		);

		float b = HueToRGB(
			p,
			q,
			hue - 1f / 3f
		);

		return new(
			r,
			g,
			b
		);
	}

	private float HueToRGB(float p, float q, float t)
	{
		if (t < 0f) t += 1f;

		if (t > 1f) t -= 1f;

		if (t < 1f / 6f) return p + (q - p) * 6f * t;

		if (t < 1f / 2f) return q;

		if (t < 2f / 3f) return p + (q - p) * (2f / 3f - t) * 6f;

		return p;
	}
}
