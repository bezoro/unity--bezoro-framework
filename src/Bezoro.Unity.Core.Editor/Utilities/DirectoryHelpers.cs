using System.IO;
using UnityEditor;
using UnityEngine;

namespace Bezoro.Unity.Core.Editor.Utilities
{
	/// <summary>
	///     Creates, copies, and resolves Unity project directories.
	/// </summary>
	public static class DirectoryHelpers
	{
		public static void Create_Symlink(string targetPath, string symlinkName, string sourcePath)
		{
			string fullTargetPath = Path.Combine(
				targetPath,
				symlinkName
			);

			if (Directory.Exists(
					fullTargetPath
				))
			{
				Debug.LogWarning(
					$"Symlink <b>{symlinkName}</b> already exists at {targetPath}."
				);

				return;
			}

			var command = $"mklink /D \"{fullTargetPath}\" \"{sourcePath}\"";
			CmdHelpers.Execute_Command(
				command,
				true
			);

			Debug.Log(
				$"Symlink <b>{symlinkName}</b> created."
			);

			AssetDatabase.Refresh();
		}
	}
}
