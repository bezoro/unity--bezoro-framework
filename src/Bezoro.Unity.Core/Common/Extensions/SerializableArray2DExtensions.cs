using Bezoro.Unity.Core.Collections;

namespace Bezoro.Unity.Core.Common.Extensions;

/// <summary>
///     Contains extension methods for Serializable Array2D.
/// </summary>
public static class SerializableArray2DExtensions
{
	public static bool IsNullOrEmpty<T>(this SerializableArray2D<T> array)
	{
		if (array == null) return true;

		if (array.Elements == null) return true;

		if (array.Elements.Length == 0) return true;

		return false;
	}
}
