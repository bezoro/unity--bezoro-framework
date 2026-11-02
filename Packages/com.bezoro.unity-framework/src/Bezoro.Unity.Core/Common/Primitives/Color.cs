using System;

namespace Bezoro.Unity.Core.Common.Primitives
{
	/// <summary>
	///     Represents Color data.
	/// </summary>
	public readonly struct Color
	{
		public Color(UnityEngine.Color color) : this(
			color.r,
			color.g,
			color.b,
			color.a
		) { }

		public Color(Bezoro.Core.Types.Color color) : this(
			color.R,
			color.G,
			color.B,
			color.A
		) { }

		public Color(float r, float g, float b, float a = 1)
		{
			R = (byte)Math.Round(
				Math.Clamp(
					r,
					0f,
					1f
				) *
				255
			);

			G = (byte)Math.Round(
				Math.Clamp(
					g,
					0f,
					1f
				) *
				255
			);

			B = (byte)Math.Round(
				Math.Clamp(
					b,
					0f,
					1f
				) *
				255
			);

			A = (byte)Math.Round(
				Math.Clamp(
					a,
					0f,
					1f
				) *
				255
			);
		}

		public byte A { get; }
		public byte B { get; }
		public byte G { get; }

		public byte R { get; }


		public static implicit operator UnityEngine.Color(Color c) => new(
			c.R,
			c.G,
			c.B,
			c.A
		);

		public static implicit operator Bezoro.Core.Types.Color(Color c) =>
			new(
				c.R,
				c.G,
				c.B,
				c.A
			);

		public static Color Gray() => new(
			0.5f,
			0.5f,
			0.5f
		);
	}
}
