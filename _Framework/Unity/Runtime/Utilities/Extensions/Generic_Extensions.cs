using Bezoro.Framework.Core;
using UnityEngine;

namespace Bezoro.Framework.Unity.Utilities
{
	public static class Generic_Extensions
	{
		public static bool Is_Null<T>(this T @object)
		where T : class
		{
			return @object is null;
		}

		public static bool Is_Not_Null<T>(this T @object)
		where T : class
		{
			Assert.IsNotNull(@object);

			return @object is not null;
		}

		public static bool Equals_Null<T>(this T @object)
		where T : class
		{
			return @object == null;
		}

		public static bool Not_Equals_Null<T>(this T @object)
		where T : class
		{
			Assert.IsNotNull(@object);

			return @object != null;
		}

		public static bool Is_Unity_Object<T>(this T @object, out T outUnityObject)
		where T : class
		{
			Assert.IsNotNull(@object);

			if (@object is Object unity_object)
			{
				outUnityObject = unity_object as T;
				return true;
			}

			outUnityObject = null;
			return false;
		}

		public static bool Is_Not_Unity_Object<T>(this T @object)
		where T : class
		{
			Assert.IsNotNull(@object);

			return @object is not Object;
		}
	}
}
