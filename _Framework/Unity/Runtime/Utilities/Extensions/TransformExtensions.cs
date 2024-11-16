using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Bezoro.Framework.Unity.Utilities
{
	public static class TransformExtensions
	{
		private const float EPSILON = 0.001f;
		private static readonly ConditionalWeakTable<Transform, TransformData> TransformDataTable = new();

		public static Bounds CalculateBoundsRecursive( this Transform rootParent )
		{
			var meshRenderers = rootParent.GetComponentsInChildren<Renderer>();

			if ( meshRenderers.Length == 0 )
			{
				BDebug.LogWarning( "The parent doesn't have meshes to calculate bounds.", rootParent );

				return new();
			}

			var totalBounds = new Bounds( rootParent.position, Vector3.zero );
			foreach ( var renderer in meshRenderers )
			{
				totalBounds.Encapsulate( renderer.bounds );
			}

			return totalBounds;
		}

		public static Vector3 CalculateAverageBoundsCenter( this Transform rootParent, Space space )
		{
			var worldPosition = CalculateBoundsRecursive( rootParent ).center;

			return space == Space.Self ? rootParent.InverseTransformPoint( worldPosition ) : worldPosition;
		}

		public static bool HasChanged
			( this Transform transform ) =>
			TransformDataTable.GetOrCreateValue( transform ).HasChanged( transform );

		public static bool HasChangedPosition
			( this Transform transform ) =>
			TransformDataTable.GetOrCreateValue( transform ).HasChangedPosition( transform );

		public static bool HasChangedRotation
			( this Transform transform ) =>
			TransformDataTable.GetOrCreateValue( transform ).HasChangedRotation( transform );

		public static bool HasChangedScale
			( this Transform transform ) =>
			TransformDataTable.GetOrCreateValue( transform ).HasChangedScale( transform );

		public static Bounds TransformBounds( this Transform transform, Bounds localBounds ) =>
			new()
			{
				center  = transform.TransformPoint( localBounds.center ),
				extents = TransformExtents( transform, localBounds.extents )
			};

		private static Vector3 TransformExtents( Transform transform, Vector3 extents )
		{
			var transformedAxes = new[]
			{
				transform.right   * extents.x,
				transform.up      * extents.y,
				transform.forward * extents.z
			};

			return new(
				transformedAxes.Sum( v => Mathf.Abs( v.x ) ),
				transformedAxes.Sum( v => Mathf.Abs( v.y ) ),
				transformedAxes.Sum( v => Mathf.Abs( v.z ) )
			);
		}

		public static void ResetAll( this Transform transform )
		{
			transform.localPosition = Vector3.zero;
			transform.localRotation = Quaternion.identity;
			transform.localScale    = Vector3.one;
		}

		private class TransformData
		{
			private Vector3 _previousPosition;
			private Quaternion _previousRotation;
			private Vector3 _previousScale;

			public bool HasChanged
				( Transform transform ) =>
				HasChangedPosition( transform ) || HasChangedRotation( transform ) || HasChangedScale( transform );

			private bool CheckAndUpdateValue<T>( T currentValue, ref T previousValue, float threshold ) where T : struct
			{
				var hasChanged = false;
				if ( currentValue is Vector3 v3Current && previousValue is Vector3 v3Prev )
				{
					hasChanged = ( v3Current - v3Prev ).sqrMagnitude > threshold;
				}
				else if ( currentValue is Quaternion qCurrent && previousValue is Quaternion qPrev )
				{
					hasChanged = Quaternion.Angle( qCurrent, qPrev ) > threshold;
				}

				if ( hasChanged )
				{
					previousValue = currentValue;
				}

				return hasChanged;
			}

			public bool HasChangedPosition
				( Transform transform ) =>
				CheckAndUpdateValue( transform.localPosition, ref _previousPosition, EPSILON );

			public bool HasChangedRotation
				( Transform transform ) =>
				CheckAndUpdateValue( transform.localRotation, ref _previousRotation, EPSILON );

			public bool HasChangedScale
				( Transform transform ) =>
				CheckAndUpdateValue( transform.localScale, ref _previousScale, EPSILON );
		}
	}
}
