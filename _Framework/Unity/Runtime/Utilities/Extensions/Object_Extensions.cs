namespace Bezoro.Framework.Unity.Utilities
{
	public static class Object_Extensions
	{
		public static bool Is_Null( this object @object )
		{
			return @object is null;
		}

		public static bool Is_Not_Null( this object @object )
		{
			return @object is not null;
		}

		public static bool Equals_Null( this object @object )
		{
			return @object == null;
		}

		public static bool Not_Equals_Null( this object @object )
		{
			return @object != null;
		}
	}
}
