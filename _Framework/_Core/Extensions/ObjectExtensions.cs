namespace Bezoro.Framework.Core.Extensions
{
	public static class ObjectExtensions
	{
		public static bool IsNull(this object @object) =>
			@object is null;

		public static bool IsNull<T>(this T @object) =>
			@object is null;

		public static bool EqualsNull(this object @object) =>
			@object == null;

		public static bool EqualsNull<T>(this T @object) =>
			@object == null;
	}
}