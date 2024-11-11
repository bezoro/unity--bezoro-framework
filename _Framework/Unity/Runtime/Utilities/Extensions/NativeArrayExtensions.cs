using Unity.Collections;

namespace Bezoro.Framework.Unity.Utilities.Extensions
{
	public static class NativeArrayExtensions
	{
		public static void SafeDispose<T>(this NativeArray<T> array)
		where T : struct
		{
			if (array.IsCreated)
			{
				array.Dispose();
			}
		}
	}
}
