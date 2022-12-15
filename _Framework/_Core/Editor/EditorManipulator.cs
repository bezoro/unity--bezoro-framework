using System.IO;
using UnityEditor;

namespace Bezoro.Core.Editor
{
	public static class EditorManipulator
	{
		public static void ReopenProject() =>
			EditorApplication.OpenProject(Directory.GetCurrentDirectory());
	}
}