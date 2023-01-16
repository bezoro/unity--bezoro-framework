namespace Bezoro.Core.Extensions
{
	public static class ObjectExtensions
	{
		public static bool IsNull(this object @object) =>
			@object == null;
	}
}