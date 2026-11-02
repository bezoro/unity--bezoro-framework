using System;

namespace Bezoro.Unity.Core.Common.Primitives
{
	[Serializable]
	/// <summary>
	/// Represents Color With Intensity data.
	/// </summary>
	public struct ColorWithIntensity : IEquatable<ColorWithIntensity>
	{
		public UnityEngine.Color color;
		public float             intensity;

		public ColorWithIntensity(UnityEngine.Color color, float intensity = 1f)
		{
			this.color     = color;
			this.intensity = intensity;
		}

		public static implicit operator UnityEngine.Color(ColorWithIntensity cwi) => cwi.color;

		public static implicit operator ColorWithIntensity(UnityEngine.Color c) => new(
			c
		);

		#region Equality

		public bool Equals(ColorWithIntensity other) => color.Equals(
															other.color
														) &&
														intensity.Equals(
															other.intensity
														);

		public override bool Equals(object? obj) => obj is ColorWithIntensity other &&
													Equals(
														other
													);

		public override int GetHashCode() => HashCode.Combine(
			color,
			intensity
		);

		#endregion
	}
}
