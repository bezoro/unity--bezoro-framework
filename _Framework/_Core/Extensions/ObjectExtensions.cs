namespace Bezoro.Framework.Core.Extensions
{
	public static class ObjectExtensions
	{
		public static bool IsNull(this object @object) =>
			@object is null;

		public static bool EqualsNull(this object @object) =>
			@object == null;
	}
}