using UnityEngine;

namespace Bezoro.Framework.Unity.Utilities.Helpers
{
	public class VectorHelpers
	{
		public static Vector3 CalculateLocalPosition(int x, int y, Vector2 cellSize, Bounds localBounds)
		{
			return localBounds.min + new Vector3(( x + 0.5f ) * cellSize.x, localBounds.size.y, ( y + 0.5f ) * cellSize.y);
		}
	}
}
