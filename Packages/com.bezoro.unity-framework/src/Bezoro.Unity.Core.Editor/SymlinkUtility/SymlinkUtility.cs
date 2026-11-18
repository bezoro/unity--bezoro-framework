using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Bezoro.Unity.Core.Tests.EditMode")]

namespace Bezoro.Unity.Core.Editor.SymlinkUtility
{
	/**
	 * An editor utility for easily creating symlinks in your project.
	 * 
	 * Adds a Menu item under `Assets/Create/Folder (Symlink)`, and
	 * draws a small indicator in the Project view for folders that are
	 * symlinks.
	 */
	[InitializeOnLoad]
	/// <summary>
	/// Creates and removes filesystem symlinks for Unity project assets.
	/// </summary>
	public static class SymlinkUtility
	{
		// FileAttributes that match a junction folder.
		private const FileAttributes _FOLDER_SYMLINK_ATTRIBS =
			FileAttributes.Directory | FileAttributes.ReparsePoint;

		// Style used to draw the symlink indicator in the project view.
		private static GUIStyle _Symlink_Marker_Style;

		/**
		 * Static constructor subscribes to projectWindowItemOnGUI delegate.
		 */
		static SymlinkUtility()
		{
			EditorApplication.projectWindowItemOnGUI -= OnProjectWindowItemGUI;
			EditorApplication.projectWindowItemOnGUI += OnProjectWindowItemGUI;
		}

		private static GUIStyle SymlinkMarkerStyle =>
			_Symlink_Marker_Style ??= new(
				EditorStyles.label
			)
			{
				normal =
				{
					textColor = new(
						.2f,
						.8f,
						.2f,
						.8f
					)
				},
				alignment = TextAnchor.MiddleRight
			};

		internal static string GetRelativePath(string source, string destination)
		{
			var parent = new Uri(Path.GetFullPath(Path.GetDirectoryName(destination)!) + Path.DirectorySeparatorChar);
			var relative = parent.MakeRelativeUri(new Uri(Path.GetFullPath(source)));
			if (relative.IsAbsoluteUri) throw new IOException("A relative link requires paths on the same volume.");
			return Uri.UnescapeDataString(relative.ToString()).Replace('/', Path.DirectorySeparatorChar);
		}

		[DllImport("libc", EntryPoint = "symlink", SetLastError = true)]
		private static extern int CreateUnixLink(string source, string destination);

		internal static void CreateUnixDirectoryLink(string source, string destination)
		{
			if (CreateUnixLink(source, destination) != 0)
				throw new IOException($"Cannot create link at {destination}: {new Win32Exception(Marshal.GetLastWin32Error()).Message}");
		}


		private static void ExecuteCmdCommand(string command, bool as_admin)
		{
			var start_info = new ProcessStartInfo
			{
				FileName              = "CMD.exe",
				Arguments             = "/C " + command,
				UseShellExecute       = as_admin,
				RedirectStandardError = !as_admin,
				CreateNoWindow        = true
			};

			if (as_admin) start_info.Verb = "runas";

			var proc = new Process { StartInfo = start_info };

			using (proc)
			{
				proc.Start();
				proc.WaitForExit();

				if (!as_admin && !proc.StandardError.EndOfStream)
					Debug.LogError(
						proc.StandardError.ReadToEnd()
					);
			}
		}

		/**
		 * Add a menu item in the Assets/Create category to add symlinks to directories.
		 */
#if UNITY_EDITOR_WIN

		// Create an absolute junction
		[MenuItem(
			"Assets/Create/Folder (Junction)",
			false,
			20
		)]
		private static void Junction() => Symlink(
			SymlinkType.Junction
		);
#endif

		/**
		 * Draw a little indicator if folder is a symlink
		 */
		private static void OnProjectWindowItemGUI(string guid, Rect r)
		{
			try
			{
				string path = AssetDatabase.GUIDToAssetPath(
					guid
				);

				if (string.IsNullOrEmpty(
						path
					)) return;

				var attributes = File.GetAttributes(
					path
				);

				if ((attributes & _FOLDER_SYMLINK_ATTRIBS) == _FOLDER_SYMLINK_ATTRIBS)
					GUI.Label(
						r,
						"<=>",
						SymlinkMarkerStyle
					);
			}
			catch
			{
				// ignored
			}
		}

		private static void Symlink(SymlinkType link_type)
		{
			string source_folder_path = EditorUtility.OpenFolderPanel(
				"Select Folder Source",
				"",
				""
			);

			// Cancelled dialog
			if (string.IsNullOrEmpty(
					source_folder_path
				)) return;

			var assets = Path.GetFullPath(Application.dataPath).TrimEnd(Path.DirectorySeparatorChar);
			var source = Path.GetFullPath(source_folder_path).TrimEnd(Path.DirectorySeparatorChar);
			var comparison = Application.platform == RuntimePlatform.WindowsEditor
				? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
			if (source.Equals(assets, comparison) || source.StartsWith(assets + Path.DirectorySeparatorChar, comparison) ||
			    assets.StartsWith(source + Path.DirectorySeparatorChar, comparison))
			{
				Debug.LogWarning("Choose a folder outside the project Assets tree that does not contain it.");
				return;
			}

			string source_folder_name = source_folder_path.Split(
				'/',
				'\\'
			).LastOrDefault();

			if (string.IsNullOrEmpty(
					source_folder_name
				))
			{
				Debug.LogWarning(
					"Couldn't deduce the folder name?"
				);

				return;
			}

			var uobject = Selection.activeObject;

			string target_path = uobject != null
									 ? AssetDatabase.GetAssetPath(
										 uobject
									 )
									 : null;

			if (string.IsNullOrEmpty(
					target_path
				)) target_path = "Assets";

			var attributes = File.GetAttributes(
				target_path
			);

			if ((attributes & FileAttributes.Directory) != FileAttributes.Directory)
				target_path = Path.GetDirectoryName(
					target_path
				);

			// Get path to project.
			string path_to_project = Directory.GetParent(Application.dataPath)!.FullName;

			target_path = $"{path_to_project}/{target_path}/{source_folder_name}";

			if (Directory.Exists(
					target_path
				))
			{
				Debug.LogWarning
				(
					$"A folder already exists at this location, aborting link.\n{source_folder_path} -> {target_path}"
				);

				return;
			}

			// Use absolute path or relative path?
			string source_path = link_type == SymlinkType.Relative
									 ? GetRelativePath(
										 source_folder_path,
										 target_path
									 )
									 : source_folder_path;
#if UNITY_EDITOR_WIN
			string link_option = link_type == SymlinkType.Junction ? "/J" : "/D";
			var    command     = $"mklink {link_option} \"{target_path}\" \"{source_path}\"";

			ExecuteCmdCommand(
				command,
				link_type != SymlinkType.Junction
			); // Symlinks require admin privilege on windows, junctions do not.
#elif UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX
			try { CreateUnixDirectoryLink(source_path, target_path); }
			catch (IOException error)
			{
				Debug.LogError(error.Message);
				return;
			}
#endif

			AssetDatabase.Refresh(
				ImportAssetOptions.ForceUpdate
			);
		}

		// Create an absolute symbolic link
		[MenuItem(
			"Assets/Create/Folder (Absolute Symlink)",
			false,
			21
		)]
		private static void SymlinkAbsolute() =>
			Symlink(
				SymlinkType.Absolute
			);

		// Create a relative symbolic link
		[MenuItem(
			"Assets/Create/Folder (Relative Symlink)",
			false,
			22
		)]
		private static void SymlinkRelative() =>
			Symlink(
				SymlinkType.Relative
			);

		/// <summary>
		///     Lists the supported Symlink Type values.
		/// </summary>
		private enum SymlinkType
		{
			Junction,
			Absolute,
			Relative
		}
	}
}
