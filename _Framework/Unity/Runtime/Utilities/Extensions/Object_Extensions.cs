using Bezoro.Framework.Core;
using UnityEngine;

namespace Bezoro.Framework.Unity.Utilities
{
	public static class Object_Extensions
	{
		public static bool Is_Null(this object @object)
		{
			return @object is null;
		}

		public static bool Is_Not_Null(this object @object)
		{
			return !Is_Null(@object);
		}

		public static bool Equals_Null(this object @object)
		{
			return @object == null;
		}

		public static bool Not_Equals_Null(this object @object)
		{
			return !Equals_Null(@object);
		}

		public static bool Is_Unity_Object(this object @object, out Object outUnityObject)
		{
			outUnityObject = null;

			if (@object.Equals_Null())
			{
				Logger.LogWarning("Object is null.");
				return false;
			}

			outUnityObject = @object as Object;

			if (outUnityObject.Equals_Null())
			{
				Logger.LogWarning("Object is not a Unity Object.");
				return false;
			}

			return true;
		}

		public static bool Is_Not_Unity_Object(this object @object)
		{
			return !Is_Unity_Object(@object, out _);
		}

		public static bool Is_Of_Type(this object @object, System.Type type)
		{
			return @object.GetType() == type;
		}

		public static bool Is_Not_Of_Type(this object @object, System.Type type)
		{
			return !Is_Of_Type(@object, type);
		}

		public static bool Is_Assignable_From(this object @object, System.Type type)
		{
			return type.IsAssignableFrom(@object.GetType());
		}

		public static bool Is_Not_Assignable_From(this object @object, System.Type type)
		{
			return !Is_Assignable_From(@object, type);
		}
	}
}
