using UnityEngine;

namespace Bezoro.Framework.Unity.Utilities
{
	public static class ObjectExtensions
	{
		public static bool IsNull( this object @object ) => @object is null;

		public static bool IsNotNull( this object @object ) => @object is not null;

		public static bool IsNull<T>( this T @object ) => @object is null;

		public static bool IsNotNull<T>( this T @object ) => @object is not null;

		public static bool EqualsNull( this object @object ) => @object == null;

		public static bool NotEqualsNull( this object @object ) => @object != null;

		public static bool EqualsNull<T>( this T @object ) => @object == null;

		public static bool NotEqualsNull<T>( this T @object ) => @object != null;

		public static bool IsUnityObject<T>( this T @object, out T outUnityObject )
		where T : class
		{
			if ( @object is Object unityObject )
			{
				outUnityObject = unityObject as T;

				return true;
			}

			outUnityObject = null;

			return false;
		}

		public static bool IsNotUnityObject<T>( this T @object )
		where T : class =>
			@object is not Object;
	}
}
