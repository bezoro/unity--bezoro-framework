using System.IO;

namespace Bezoro.Unity.Core.Editor.Utilities
{
	/// <summary>
	///     Copies, writes, and resolves files used by editor tooling.
	/// </summary>
	public class FileHelpers
	{
		/// <summary>
		///     Checks if a file exists at the specified path.
		/// </summary>
		/// <param name="path">The path to check for file existence.</param>
		/// <returns>True if the file exists, false otherwise.</returns>
		public static bool FileExists(string path)
		{
			if (string.IsNullOrEmpty(
					path
				)) return false;

			return File.Exists(
				path
			);
		}
	}
}
