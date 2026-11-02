namespace Bezoro.Unity.Core.Editor.Utilities
{
	/// <summary>
	///     Builds raw GitHub Gist URLs for editor download workflows.
	/// </summary>
	public static class GistHelpers
	{
		public static string Get_URL(string id, string user = "bezoro") =>
			$"https://gist.github.com/{user}/{id}/raw";
	}
}
