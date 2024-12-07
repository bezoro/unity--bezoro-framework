namespace Bezoro.Framework.Unity.Serializable2DArray
{
	public static class Serializable_2D_Array_Extensions
	{
		public static bool Is_Null_Or_Empty<T>( this Serializable_2D_Array<T> array )
		{
			if ( array == null ) return true;
			if ( array.Data == null ) return true;
			if ( array.Data.Length == 0 ) return true;

			return false;
		}
	}
}
