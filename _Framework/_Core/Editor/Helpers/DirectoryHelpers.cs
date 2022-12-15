using System.IO;
using Bezoro.Core.Helpers;
using UnityEditor;
using UnityEngine;

namespace Bezoro.Core.Editor.Helpers
{
	public class DirectoryHelpers
	{
		public static void CreateSymlink
		(
			string targetPath,
			string symlinkName,
			string sourcePath
		)
		{
			var fullTargetPath = Path.Combine(targetPath, symlinkName);

			if (Directory.Exists(fullTargetPath))
			{
				Debug.LogWarning
					($"Symlink <b>{symlinkName}</b> already exists at {targetPath}.");

				return;
			}

			var command = $"mklink /D \"{fullTargetPath}\" \"{sourcePath}\"";
			CmdHelpers.ExecuteCmdCommand(command, true);
			Debug.Log($"Symlink <b>{symlinkName}</b> created.");
			AssetDatabase.Refresh();
		}
	}
}