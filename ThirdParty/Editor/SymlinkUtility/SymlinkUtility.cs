using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace SymlinkUtility.Editor
{
	/**
	 * An editor utility for easily creating symlinks in your project.
	 * 
	 * Adds a Menu item under `Assets/Create/Folder (Symlink)`, and
	 * draws a small indicator in the Project view for folders that are
	 * symlinks.
	 */
	[InitializeOnLoad]
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
			EditorApplication.projectWindowItemOnGUI += OnProjectWindowItemGUI;
		}

		private static GUIStyle SymlinkMarkerStyle =>
			_Symlink_Marker_Style ??= new( EditorStyles.label )
			{
				normal =
				{
					textColor = new(
						.2f,
						.8f,
						.2f,
						.8f )
				},
				alignment = TextAnchor.MiddleRight
			};

		/**
		 * Draw a little indicator if folder is a symlink
		 */
		private static void OnProjectWindowItemGUI( string guid, Rect r )
		{
			try
			{
				var path = AssetDatabase.GUIDToAssetPath( guid );

				if ( string.IsNullOrEmpty( path ) ) return;

				var attributes = File.GetAttributes( path );

				if ( ( attributes & _FOLDER_SYMLINK_ATTRIBS ) == _FOLDER_SYMLINK_ATTRIBS )
				{
					GUI.Label(
						r,
						"<=>",
						SymlinkMarkerStyle );
				}
			} catch
			{
				// ignored
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
			20 )]
		private static void Junction() => Symlink( SymlinkType.Junction );
#endif

		// Create an absolute symbolic link
		[MenuItem(
			"Assets/Create/Folder (Absolute Symlink)",
			false,
			21 )]
		private static void SymlinkAbsolute()
		{
			Symlink( SymlinkType.Absolute );
		}

		// Create a relative symbolic link
		[MenuItem(
			"Assets/Create/Folder (Relative Symlink)",
			false,
			22 )]
		private static void SymlinkRelative()
		{
			Symlink( SymlinkType.Relative );
		}

		private static void Symlink( SymlinkType link_type )
		{
			var source_folder_path = EditorUtility.OpenFolderPanel(
				"Select Folder Source",
				"",
				"" );

			// Cancelled dialog
			if ( string.IsNullOrEmpty( source_folder_path ) ) return;

			if ( source_folder_path.Contains( Application.dataPath ) )
			{
				Debug.LogWarning( "Cannot create a symlink to folder in your project!" );
				return;
			}

			var source_folder_name = source_folder_path.Split(
				'/',
				'\\' ).LastOrDefault();

			if ( string.IsNullOrEmpty( source_folder_name ) )
			{
				Debug.LogWarning( "Couldn't deduce the folder name?" );
				return;
			}

			var uobject = Selection.activeObject;

			var target_path = uobject != null ? AssetDatabase.GetAssetPath( uobject ) : null;

			if ( string.IsNullOrEmpty( target_path ) ) target_path = "Assets";

			var attributes = File.GetAttributes( target_path );

			if ( ( attributes & FileAttributes.Directory ) != FileAttributes.Directory )
				target_path = Path.GetDirectoryName( target_path );

			// Get path to project.
			var path_to_project = Application.dataPath.Split(
				new[] { "/Assets" },
				StringSplitOptions.None )[ 0 ];

			target_path = $"{path_to_project}/{target_path}/{source_folder_name}";

			if ( Directory.Exists( target_path ) )
			{
				Debug.LogWarning
				(
					$"A folder already exists at this location, aborting link.\n{source_folder_path} -> {target_path}"
				);

				return;
			}

			// Use absolute path or relative path?
			var source_path = link_type == SymlinkType.Relative
				? GetRelativePath( source_folder_path, target_path )
				: source_folder_path;
#if UNITY_EDITOR_WIN
			var link_option = link_type == SymlinkType.Junction ? "/J" : "/D";
			var command     = $"mklink {link_option} \"{target_path}\" \"{source_path}\"";
			ExecuteCmdCommand(
				command,
				link_type != SymlinkType.Junction
			); // Symlinks require admin privilege on windows, junctions do not.
#elif UNITY_EDITOR_OSX
            // For some reason, OSX doesn't want to create a symlink with quotes around the paths, so escape the spaces instead.
            sourcePath = sourcePath.Replace(" ", "\\ ");
            targetPath = targetPath.Replace(" ", "\\ ");
            string command = string.Format("ln -s {0} {1}", sourcePath, targetPath);
            ExecuteBashCommand(command);
#elif UNITY_EDITOR_LINUX
            // Is Linux the same as OSX?
#endif

			AssetDatabase.Refresh( ImportAssetOptions.ForceUpdate );
		}

		private static string GetRelativePath( string source_path, string output_path )
		{
			if ( string.IsNullOrEmpty( output_path ) ) return source_path;

			source_path ??= string.Empty;

			var split_output = output_path.Split( new[] { '/' }, StringSplitOptions.RemoveEmptyEntries );
			var split_source = source_path.Split( new[] { '/' }, StringSplitOptions.RemoveEmptyEntries );

			var max = Mathf.Min( split_output.Length, split_source.Length );

			var i = 0;

			while ( i < max )
			{
				if ( split_output[ i ] != split_source[ i ] ) break;

				++i;
			}

			var hop_up_count     = split_output.Length - i - 1;
			var new_split_count  = hop_up_count + split_source.Length - i;
			var new_split_target = new string[new_split_count];
			var j                = 0;

			for ( ; j < hop_up_count ; ++j ) new_split_target[ j ] = "..";

			for ( max = new_split_target.Length ; j < max ; ++j, ++i ) new_split_target[ j ] = split_source[ i ];

			return string.Join(
				Path.DirectorySeparatorChar.ToString(),
				new_split_target );
		}

		private static void ExecuteCmdCommand( string command, bool as_admin )
		{
			var start_info = new ProcessStartInfo
			{
				FileName              = "CMD.exe",
				Arguments             = "/C " + command,
				UseShellExecute       = as_admin,
				RedirectStandardError = !as_admin,
				CreateNoWindow        = true
			};

			if ( as_admin ) start_info.Verb = "runas";

			var proc = new Process { StartInfo = start_info };

			using ( proc )
			{
				proc.Start();
				proc.WaitForExit();

				if ( !as_admin && !proc.StandardError.EndOfStream ) Debug.LogError( proc.StandardError.ReadToEnd() );
			}
		}

		private static void ExecuteBashCommand( string command )
		{
			command = command.Replace(
				"\"",
				"\"\"" );

			var proc = new Process
			{
				StartInfo = new()
				{
					FileName               = "/bin/bash",
					Arguments              = "-c \"" + command + "\"",
					UseShellExecute        = false,
					RedirectStandardOutput = true,
					RedirectStandardError  = true,
					CreateNoWindow         = true
				}
			};

			using ( proc )
			{
				proc.Start();
				proc.WaitForExit();

				if ( !proc.StandardError.EndOfStream ) Debug.LogError( proc.StandardError.ReadToEnd() );
			}
		}

		private enum SymlinkType
		{
			Junction,
			Absolute,
			Relative
		}
	}
}
