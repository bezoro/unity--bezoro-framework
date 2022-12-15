namespace Bezoro.Core.Helpers
{
	public static class GistHelpers
	{
		public static string GetUrl(string id, string user = "bezoro") =>
			$"https://gist.github.com/{user}/{id}/raw";
	}
}