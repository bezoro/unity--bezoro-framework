using System.Net.Http;
using System.Threading.Tasks;

namespace Bezoro.Framework.Unity.Utilities
{
	public static class UrlHelpers
	{
		public static async Task<string> GetContents( string url )
		{
			using var client = new HttpClient();
			var response = await client.GetAsync( url );
			var content = await response.Content.ReadAsStringAsync();

			return content;
		}
	}
}
