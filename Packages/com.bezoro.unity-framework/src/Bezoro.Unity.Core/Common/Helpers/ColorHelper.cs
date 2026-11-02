using UnityEngine;

namespace Bezoro.Unity.Core.Common.Helpers;

/// <summary>
///     Builds and converts Unity color values.
/// </summary>
public static class ColorHelper
{
	public static Color HslToRGB(float hue, float saturation, float lightness)
	{
		hue = Mathf.Clamp(hue, 0f, 360f) / 360f;

		saturation = Mathf.Clamp(
						 saturation,
						 0f,
						 100f
					 ) /
					 100f;

		lightness = Mathf.Clamp(
						lightness,
						0f,
						100f
					) /
					100f;

		if (saturation == 0)
			// Achromatic grey
			return new(
				lightness,
				lightness,
				lightness
			);

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

	private static float HueToRGB(float p, float q, float t)
	{
		if (t < 0f) t += 1f;

		if (t > 1f) t -= 1f;

		if (t < 1f / 6f) return p + (q - p) * 6f * t;

		if (t < 1f / 2f) return q;

		if (t < 2f / 3f) return p + (q - p) * (2f / 3f - t) * 6f;

		return p;
	}
}
