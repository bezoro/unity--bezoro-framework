using Unity.Collections;

namespace Bezoro.Unity.Core.Common.Extensions;

/// <summary>
///     Contains extension methods for Native Array.
/// </summary>
public static class NativeArrayExtensions
{
	public static void Safe_Dispose<T>(this NativeArray<T> array)
		where T : struct
	{
		if (array.IsCreated) array.Dispose();
	}
}
