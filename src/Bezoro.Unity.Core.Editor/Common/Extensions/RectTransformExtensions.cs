using System.Collections.Generic;
using UnityEngine;

namespace Bezoro.Unity.Core.Editor.Common.Extensions
{
	/// <summary>
	///     Contains extension methods for Rect Transform.
	/// </summary>
	public static class RectTransformExtensions
	{
		public static bool ContainsRootCanvas(this ICollection<RectTransform> rts)
		{
			foreach (var rt in rts)
			{
				if (rt.IsRootCanvas())
					return true;
			}

			return false;
		}

		public static bool IsImmediateChildOfRoot(this RectTransform rt) =>
			rt.parent != null &&
			rt.parent.TryGetComponent(out Canvas canvas) &&
			canvas.isRootCanvas;

		public static bool IsRootCanvas(this RectTransform rt) => rt.parent == null && rt.GetComponent<Canvas>();
	}
}
