using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using Logger = Bezoro.Logging.Logger;

namespace Bezoro.Unity.Core.Common.Extensions;

/// <summary>
///     Contains extension methods for Transform.
/// </summary>
public static class TransformExtensions
{
	private const           float                                          _EPSILON              = 0.001f;
	private static readonly ConditionalWeakTable<Transform, TransformData> _TRANSFORM_DATA_TABLE = new();

	public static bool HasChanged(this Transform transform) =>
		_TRANSFORM_DATA_TABLE.GetOrCreateValue(
			transform
		).HasChanged(
			transform
		);

	public static bool HasChangedPosition(this Transform transform) =>
		_TRANSFORM_DATA_TABLE.GetOrCreateValue(
			transform
		).HasChangedPosition(
			transform
		);

	public static bool HasChangedRotation(this Transform transform) =>
		_TRANSFORM_DATA_TABLE.GetOrCreateValue(
			transform
		).HasChangedRotation(
			transform
		);

	public static bool HasChangedScale(this Transform transform) =>
		_TRANSFORM_DATA_TABLE.GetOrCreateValue(
			transform
		).HasChangedScale(
			transform
		);

	public static Bounds CalculateBoundsRecursive(this Transform rootParent)
	{
		var meshRenderers = rootParent.GetComponentsInChildren<Renderer>();

		if (meshRenderers.Length == 0)
		{
			Logger.LogWarning(
				"The parent doesn't have meshes to calculate bounds.",
				contextObject: rootParent
			);

			return new();
		}

		var totalBounds = new Bounds(
			rootParent.position,
			Vector3.zero
		);

		foreach (var renderer in meshRenderers)
		{
			totalBounds.Encapsulate(
				renderer.bounds
			);
		}

		return totalBounds;
	}

	public static Bounds TransformBounds(this Transform transform, Bounds localBounds) =>
		new()
		{
			center = transform.TransformPoint(
				localBounds.center
			),
			extents = TransformExtents(
				transform,
				localBounds.extents
			)
		};

	public static Vector3 CalculateAverageBoundsCenter(this Transform rootParent, Space space)
	{
		var worldPosition = rootParent.CalculateBoundsRecursive().center;

		return space == Space.Self
				   ? rootParent.InverseTransformPoint(
					   worldPosition
				   )
				   : worldPosition;
	}

	public static void ResetAll(this Transform transform)
	{
		transform.localPosition = Vector3.zero;
		transform.localRotation = Quaternion.identity;
		transform.localScale    = Vector3.one;
	}

	private static Vector3 TransformExtents(Transform transform, Vector3 extents)
	{
		var transformedAxes = new[]
		{
			transform.TransformVector(new Vector3(extents.x, 0f,       0f)),
			transform.TransformVector(new Vector3(0f,       extents.y, 0f)),
			transform.TransformVector(new Vector3(0f,       0f,       extents.z))
		};

		return new(
			transformedAxes.Sum(v => Mathf.Abs(
									v.x
								)
			),
			transformedAxes.Sum(v => Mathf.Abs(
									v.y
								)
			),
			transformedAxes.Sum(v => Mathf.Abs(
									v.z
								)
			)
		);
	}

	/// <summary>
	///     Represents the Transform Data.
	/// </summary>
	private class TransformData
	{
		private Quaternion _previousRotation;
		private Vector3    _previousPosition;
		private Vector3    _previousScale;

		public bool HasChanged(Transform transform) =>
			HasChangedPosition(
				transform
			) ||
			HasChangedRotation(
				transform
			) ||
			HasChangedScale(
				transform
			);

		public bool HasChangedPosition(Transform transform) =>
			CheckAndUpdateValue(
				transform.localPosition,
				ref _previousPosition,
				_EPSILON
			);

		public bool HasChangedRotation(Transform transform) =>
			CheckAndUpdateValue(
				transform.localRotation,
				ref _previousRotation,
				_EPSILON
			);

		public bool HasChangedScale(Transform transform) =>
			CheckAndUpdateValue(
				transform.localScale,
				ref _previousScale,
				_EPSILON
			);

		private bool CheckAndUpdateValue<T>(T currentValue, ref T previousValue, float threshold)
			where T : struct
		{
			var hasChanged = false;

			if (currentValue is Vector3 v3Current && previousValue is Vector3 v3Prev)
				hasChanged = (v3Current - v3Prev).sqrMagnitude > threshold;
			else if (currentValue is Quaternion qCurrent && previousValue is Quaternion qPrev)
				hasChanged = Quaternion.Angle(
								 qCurrent,
								 qPrev
							 ) >
							 threshold;

			if (hasChanged) previousValue = currentValue;

			return hasChanged;
		}
	}
}
