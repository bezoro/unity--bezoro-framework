using UnityEngine;
using Debug = Sisus.Debugging.Debug;

namespace Bezoro.Core.Extensions
{
	public static class TransformExtensions
	{
		public static void ResetPosition(this Transform self) =>
			self.localPosition = Vector3.zero;

		public static void ResetRotation(this Transform self) =>
			self.localRotation = Quaternion.identity;

		public static void ResetScale(this Transform self) =>
			self.localScale = Vector3.one;

		public static Bounds CalculateBoundsRecursive(this Transform rootParent)
		{
			var meshRenderers = rootParent.GetComponentsInChildren<Renderer>();
			if (meshRenderers.Length == 0)
			{
				Debug.LogWarning($"the parent doesn't have meshes to calculate the bounds of. Returning an empty bounds.",
								 rootParent);

				return new Bounds();
			}

			var totalBounds = new Bounds(rootParent.position, Vector3.zero);
			for (var i = 0; i < meshRenderers.Length; i++)
			{
				var meshFilter = meshRenderers[i];
				totalBounds.Encapsulate(meshFilter.bounds);
			}

			return totalBounds;
		}

		public static Vector3 CalculateAverageBoundsCenter(this Transform rootParent, Space space)
		{
			var totalBounds = CalculateBoundsRecursive(rootParent);
			var worldPosition = totalBounds.center;
			return space == Space.Self ?
					   rootParent.InverseTransformPoint(worldPosition) :
					   worldPosition;
		}
	}
}