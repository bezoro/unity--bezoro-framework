namespace Bezoro.Framework.Unity.Editor.Utilities.Helpers
{
	public static class GistHelpers
	{
		public static string GetUrl( string id, string user = "bezoro" )
		{
			return $"https://gist.github.com/{user}/{id}/raw";
		}
	}
}
