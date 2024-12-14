using System.IO;
using UnityEditor;
using UnityEngine;

namespace Bezoro.Framework.Unity.Editor.Utilities
{
	public static class Directory_Helpers
	{
		public static void Create_Symlink(string target_path, string symlink_name, string source_path)
		{
			var full_target_path = Path.Combine(target_path, symlink_name);

			if (Directory.Exists(full_target_path))
			{
				Debug.LogWarning($"Symlink <b>{symlink_name}</b> already exists at {target_path}.");
				return;
			}

			var command = $"mklink /D \"{full_target_path}\" \"{source_path}\"";
			CMD_Helpers.Execute_Command(command, true);
			Debug.Log($"Symlink <b>{symlink_name}</b> created.");
			AssetDatabase.Refresh();
		}
	}
}
