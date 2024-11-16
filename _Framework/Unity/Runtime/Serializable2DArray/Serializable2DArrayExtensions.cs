namespace Bezoro.Framework.Unity.Serializable2DArray
{
	public static class Serializable2DArrayExtensions
	{
		public static bool IsNullOrEmpty<T>
			( this Serializable2DArray<T> array ) =>
			array == null || array.Data == null || array.Data.Length == 0;
	}
}
