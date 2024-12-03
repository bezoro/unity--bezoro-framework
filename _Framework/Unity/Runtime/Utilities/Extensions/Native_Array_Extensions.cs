using Unity.Collections;

namespace Bezoro.Framework.Unity.Utilities
{
	public static class Native_Array_Extensions
	{
		public static void Safe_Dispose<T>( this NativeArray<T> array )
		where T : struct
		{
			if ( array.IsCreated ) array.Dispose();
		}
	}
}
