using System;
using UnityEditor;
using UnityEngine;
using System.Reflection;
using UnityEditor.Compilation;

namespace Bezoro.Editor
{
	/// <summary>
	/// Plays a sound effect when script compiling starts and ends. 
	/// </summary>
	[InitializeOnLoad]
	internal static class CompileIndicator
	{
		private const string AudioClipType = "t:audioclip ";
		private const string StartCompilationSFXName = "SFX_StartCompilation";
		private const string EndCompilationSFXName = "SFX_EndCompilation";
		private const string Path = "Assets/_Shared-Code/_Framework/CompilationSoundPlayer";

		static CompileIndicator()
		{
			AssetHelpers.TryFindAsset(AudioClipType + StartCompilationSFXName,
									  new[] { Path },
									  out AudioClip startClip);

			AssetHelpers.TryFindAsset(AudioClipType + EndCompilationSFXName,
									  new[] { Path },
									  out AudioClip endClip);

			CompilationPipeline.compilationStarted += _ => PlayClip(startClip);
			CompilationPipeline.compilationFinished += _ => PlayClip(endClip);
			AssemblyReloadEvents.afterAssemblyReload += () => PlayClip(endClip);
		}

		private static void PlayClip(AudioClip clip, int startSample = 0, bool loop = false)
		{
			try
			{
				var unityEditorAssembly = typeof(AudioImporter).Assembly;
				var audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");
				var method = audioUtilClass.GetMethod("PlayPreviewClip",
													  BindingFlags.Static | BindingFlags.Public,
													  null,
													  new[]
													  {
														  typeof(AudioClip), typeof(int),
														  typeof(bool)
													  },
													  null);

				method.Invoke(null, new object[] { clip, startSample, loop });
			}
			catch (Exception e)
			{
				Debug.LogError("Error playing clip: " + e.Message);
			}
		}
	}
}