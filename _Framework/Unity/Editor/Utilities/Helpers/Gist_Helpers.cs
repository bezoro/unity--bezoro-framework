namespace Bezoro.Framework.Unity.Editor.Utilities
{
	public static class Gist_Helpers
	{
		public static string Get_URL(string id, string user = "bezoro")
		{
			return $"https://gist.github.com/{user}/{id}/raw";
		}
	}
}
