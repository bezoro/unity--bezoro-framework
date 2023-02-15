using System.IO;
using UnityEditor;

namespace Bezoro.Core.Editor
{
	public static class EditorHelpers
	{
		public static void ReopenProject() =>
			EditorApplication.OpenProject(Directory.GetCurrentDirectory());
	}
}