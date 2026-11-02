using System.Net.Http;
using System.Threading.Tasks;

namespace Bezoro.Unity.Core.Common.Helpers;

/// <summary>
///     Downloads text content from URLs.
/// </summary>
public static class UrlHelpers
{
	public static async Task<string> GetContents(string url)
	{
		using var client = new HttpClient();
		var response = await client.GetAsync(
						   url
					   );

		string content = await response.Content.ReadAsStringAsync();

		return content;
	}
}
