using System;
using UnityEditor;
using UnityEngine;
using System.Reflection;
using UnityEditor.Compilation;

namespace Bezoro.Editor
{
	[InitializeOnLoad]
	internal static class CompileIndicator
	{
		static CompileIndicator()
		{
			if (Initialized)
				return;

			AssetHelpers.TryFindAsset(AudioClipType + StartCompilationSFXName,
									  new[] { Path },
									  out StartClip);

			AssetHelpers.TryFindAsset(AudioClipType + EndCompilationSFXName,
									  new[] { Path },
									  out EndClip);

			CompilationPipeline.compilationStarted += _ => PlayClip(StartClip);
			CompilationPipeline.compilationFinished += _ => PlayClip(EndClip);
			AssemblyReloadEvents.afterAssemblyReload += () => PlayClip(EndClip);
			Initialized = true;
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

		private static readonly AudioClip StartClip;
		private static readonly AudioClip EndClip;

		private const string AudioClipType = "t:audioclip ";
		private const string StartCompilationSFXName = "SFX_StartCompilation";
		private const string EndCompilationSFXName = "SFX_EndCompilation";
		private const string Path = "Assets/_SharedCode/_Framework/Editor/CompilationSoundPlayer";

		private static readonly bool Initialized;
	}
}