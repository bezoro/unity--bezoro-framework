using System.IO;
using UnityEditor;

namespace Bezoro.Framework.Core.Editor.Helpers
{
	public static class EditorHelpers
	{
		public static void ReopenProject() =>
			EditorApplication.OpenProject(Directory.GetCurrentDirectory());
	}
}