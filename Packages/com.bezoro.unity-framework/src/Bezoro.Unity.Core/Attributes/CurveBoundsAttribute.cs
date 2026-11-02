using System;
using UnityEngine;

namespace Bezoro.Unity.Core.Attributes;

/// <summary>
///     Keeps every key of an AnimationCurve inside the given rectangle.
///     • If <paramref name="lockViewRect" />  is true the grid is locked.
///     • If <paramref name="showOverlay" />   is true a coloured outline / icon
///     is drawn on top of the curve preview to remind the user.
/// </summary>
[AttributeUsage(
	AttributeTargets.Field
)]
/// <summary>
/// Defines the Curve Bounds Attribute.
/// </summary>
public sealed class CurveBoundsAttribute : PropertyAttribute
{
	public readonly bool  LockViewRect;
	public readonly bool  ShowOverlay;
	public readonly float MaxTime;
	public readonly float MaxValue;
	public readonly float MinTime;
	public readonly float MinValue;

	public CurveBoundsAttribute(
		float minTime,
		float minValue,
		float maxTime,
		float maxValue,
		bool  lockViewRect = false,
		bool  showOverlay  = true)
	{
		MinTime      = minTime;
		MinValue     = minValue;
		MaxTime      = maxTime;
		MaxValue     = maxValue;
		LockViewRect = lockViewRect;
		ShowOverlay  = showOverlay;
	}

	public Rect ViewRect => new(
		MinTime,
		MinValue,
		MaxTime - MinTime,
		MaxValue - MinValue
	);
}
