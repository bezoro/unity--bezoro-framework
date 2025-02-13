using UnityEngine;

namespace Bezoro.Framework.Unity.Utilities
{
	public static class Utilities
	{
		public static bool Verify_Object_Exists(Object obj, string message = null, Object context = null)
		{
			return Verify_Object_Exists(obj as object, message, context);
		}

		public static bool Verify_Object_Exists(object obj, string message = null, Object context = null)
		{
			if (obj != null) return true;

			BDebug.Log_Warning(message, context);
			return false;
		}
	}
}
