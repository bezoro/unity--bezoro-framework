using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Bezoro.Framework.Unity.Editor.Utilities
{
	public static class EditorAudioManager
	{
		public static void PlayClip( AudioClip clip )
		{
			if ( clip == null )
			{
				throw new ArgumentNullException( nameof(clip) );
			}

			try
			{
				StopAllPreviewClips();
				InvokePlayPreviewClip( clip );
			}
			catch ( Exception ex )
			{
				Debug.LogError( $"Error playing compilation sound: {ex.Message}" );
			}
		}

		private static void StopAllPreviewClips()
		{
			var stopMethod = GetAudioUtilMethod( "StopAllPreviewClips" );
			stopMethod?.Invoke( null, null );
		}

		private static void InvokePlayPreviewClip( AudioClip clip )
		{
			var method = GetAudioUtilMethod( "PlayPreviewClip" );
			method?.Invoke( null, new object[] { clip, 0, false } );
		}

		private static MethodInfo GetAudioUtilMethod( string methodName )
		{
			const string audioUtilClassName = "UnityEditor.AudioUtil";
			var unityEditorAssembly = typeof(AudioImporter).Assembly;

			try
			{
				var audioUtilClass = unityEditorAssembly.GetType( audioUtilClassName );
				const BindingFlags flags = BindingFlags.Static | BindingFlags.Public;
				return audioUtilClass?.GetMethod( methodName, flags );
			}
			catch ( Exception ex )
			{
				Debug.LogError( $"Error retrieving method {methodName} from {audioUtilClassName}: {ex.Message}" );
				return null;
			}
		}
	}
}
