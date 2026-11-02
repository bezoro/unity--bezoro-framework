using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bezoro.Unity.Core.Editor.Tools.CompilationSoundPlayer;
using Bezoro.Unity.Core.Editor.Utilities;
using UnityEditor;
using UnityEngine;

namespace Bezoro.Unity.Core.Editor.Tools.AudioNormalization
{
	public readonly struct AudioNormalizationStats
	{
		public AudioNormalizationStats(
			int   sampleFrameCount,
			int   channelCount,
			int   frequency,
			float peakAmplitude,
			float rmsAmplitude,
			float activeRmsAmplitude,
			float activeSamplePercent
		)
		{
			SampleFrameCount    = sampleFrameCount;
			ChannelCount        = channelCount;
			Frequency           = frequency;
			PeakAmplitude       = peakAmplitude;
			RmsAmplitude        = rmsAmplitude;
			ActiveRmsAmplitude  = activeRmsAmplitude;
			ActiveSamplePercent = activeSamplePercent;
		}

		public int   SampleFrameCount    { get; }
		public int   ChannelCount        { get; }
		public int   Frequency           { get; }
		public float PeakAmplitude       { get; }
		public float RmsAmplitude        { get; }
		public float ActiveRmsAmplitude  { get; }
		public float ActiveSamplePercent { get; }

		public float DurationSeconds =>
			Frequency <= 0
				? 0f
				: SampleFrameCount / (float)Frequency;

		public float PeakDb =>
			EditorAudioNormalizer.AmplitudeToDb(
				PeakAmplitude
			);

		public float RmsDb =>
			EditorAudioNormalizer.AmplitudeToDb(
				RmsAmplitude
			);

		public float ActiveRmsDb =>
			EditorAudioNormalizer.AmplitudeToDb(
				ActiveRmsAmplitude
			);
	}

	public readonly struct AudioNormalizationPlan
	{
		public AudioNormalizationPlan(
			float targetActiveRmsDb,
			float maxPeakDb,
			float gainDb,
			float gainLinear,
			float resultingPeakDb,
			float resultingActiveRmsDb,
			bool  isPeakLimited
		)
		{
			TargetActiveRmsDb    = targetActiveRmsDb;
			MaxPeakDb            = maxPeakDb;
			GainDb               = gainDb;
			GainLinear           = gainLinear;
			ResultingPeakDb      = resultingPeakDb;
			ResultingActiveRmsDb = resultingActiveRmsDb;
			IsPeakLimited        = isPeakLimited;
		}

		public float TargetActiveRmsDb    { get; }
		public float MaxPeakDb            { get; }
		public float GainDb               { get; }
		public float GainLinear           { get; }
		public float ResultingPeakDb      { get; }
		public float ResultingActiveRmsDb { get; }
		public bool  IsPeakLimited        { get; }
	}

	public static class EditorAudioNormalizer
	{
		public static AudioNormalizationStats AnalyzeSamples(
			float[] samples,
			int     channelCount,
			int     frequency,
			float   silenceThresholdDb
		)
		{
			if (samples == null)
				throw new ArgumentNullException(
					nameof(samples)
				);
			if (channelCount <= 0)
				throw new ArgumentOutOfRangeException(
					nameof(channelCount)
				);
			if (frequency <= 0)
				throw new ArgumentOutOfRangeException(
					nameof(frequency)
				);

			var silenceThresholdAmplitude = DbToAmplitude(
				silenceThresholdDb
			);
			var peakAmplitude      = 0d;
			var sumSquares         = 0d;
			var activeSumSquares   = 0d;
			var activeSampleCount  = 0;
			var sampleFrameCount   = samples.Length / channelCount;
			var activeSampleFactor = samples.Length == 0
				? 0f
				: 100f / samples.Length;

			foreach (var sample in samples)
			{
				var absoluteSample = Math.Abs(
					sample
				);

				peakAmplitude = Math.Max(
					peakAmplitude,
					absoluteSample
				);
				sumSquares += sample * sample;

				if (absoluteSample < silenceThresholdAmplitude) continue;

				activeSumSquares += sample * sample;
				activeSampleCount++;
			}

			var rmsAmplitude = samples.Length == 0
				? 0f
				: (float)Math.Sqrt(
					sumSquares / samples.Length
				);
			var activeRmsAmplitude = activeSampleCount == 0
				? 0f
				: (float)Math.Sqrt(
					activeSumSquares / activeSampleCount
				);

			return new AudioNormalizationStats(
				sampleFrameCount,
				channelCount,
				frequency,
				(float)peakAmplitude,
				rmsAmplitude,
				activeRmsAmplitude,
				activeSampleCount * activeSampleFactor
			);
		}

		public static AudioNormalizationPlan CreateNormalizationPlan(
			AudioNormalizationStats stats,
			float                   targetActiveRmsDb,
			float                   maxPeakDb
		)
		{
			ValidateDbValue(
				targetActiveRmsDb,
				nameof(targetActiveRmsDb)
			);
			ValidateDbValue(
				maxPeakDb,
				nameof(maxPeakDb)
			);

			if (stats.ActiveRmsAmplitude <= 0f || stats.PeakAmplitude <= 0f)
				return new AudioNormalizationPlan(
					targetActiveRmsDb,
					maxPeakDb,
					0f,
					1f,
					stats.PeakDb,
					stats.ActiveRmsDb,
					false
				);

			var desiredGainDb = targetActiveRmsDb - stats.ActiveRmsDb;
			var peakLimitDb  = maxPeakDb - stats.PeakDb;
			var gainDb = Math.Min(
				desiredGainDb,
				peakLimitDb
			);

			return new AudioNormalizationPlan(
				targetActiveRmsDb,
				maxPeakDb,
				gainDb,
				DbToAmplitude(
					gainDb
				),
				stats.PeakDb + gainDb,
				stats.ActiveRmsDb + gainDb,
				gainDb < desiredGainDb - 0.0001f
			);
		}

		public static float[] NormalizeSamples(float[] samples, float gainLinear)
		{
			if (samples == null)
				throw new ArgumentNullException(
					nameof(samples)
				);
			if (gainLinear < 0f || float.IsNaN(
				    gainLinear
			    ) || float.IsInfinity(
				    gainLinear
			    ))
				throw new ArgumentOutOfRangeException(
					nameof(gainLinear)
				);

			var normalizedSamples = new float[samples.Length];
			for (var i = 0; i < samples.Length; i++)
				normalizedSamples[i] = Mathf.Clamp(
					samples[i] * gainLinear,
					-1f,
					1f
				);

			return normalizedSamples;
		}

		public static string GetReplacementAssetPath(string sourceAssetPath)
		{
			if (string.IsNullOrWhiteSpace(
				    sourceAssetPath
			    ))
				throw new ArgumentException(
					"Source asset path cannot be empty.",
					nameof(sourceAssetPath)
				);

			var normalizedPath = NormalizeAssetPath(
				sourceAssetPath
			);

			return string.Equals(
				       Path.GetExtension(
					       normalizedPath
				       ),
				       ".wav",
				       StringComparison.OrdinalIgnoreCase
			       )
				? normalizedPath
				: NormalizeAssetPath(
					Path.ChangeExtension(
						normalizedPath,
						".wav"
					)
				);
		}

		public static string GetNormalizedCopiesFolderPath(string sourceFolderAssetPath)
		{
			if (string.IsNullOrWhiteSpace(
				    sourceFolderAssetPath
			    ))
				throw new ArgumentException(
					"Source folder path cannot be empty.",
					nameof(sourceFolderAssetPath)
				);

			var normalizedPath = NormalizeAssetPath(
				sourceFolderAssetPath
			);
			var parentPath = NormalizeAssetPath(
				Path.GetDirectoryName(
					normalizedPath
				) ?? "Assets"
			);
			var folderName = Path.GetFileName(
				normalizedPath
			);

			return NormalizeAssetPath(
				$"{parentPath}/{folderName}_Normalized"
			);
		}

		public static IReadOnlyList<string> FindAudioClipAssetPaths(
			string folderAssetPath,
			bool   includeSubfolders
		)
		{
			if (!AssetDatabase.IsValidFolder(
				    folderAssetPath
			    ))
				throw new DirectoryNotFoundException(
					$"Folder does not exist: {folderAssetPath}"
				);

			var normalizedFolderPath = NormalizeAssetPath(
				folderAssetPath
			);

			return AssetDatabase.FindAssets(
					"t:AudioClip",
					new[]
					{
						normalizedFolderPath
					}
				)
				.Select(AssetDatabase.GUIDToAssetPath)
				.Select(NormalizeAssetPath)
				.Where(path => includeSubfolders || IsDirectChildOfFolder(
					path,
					normalizedFolderPath
				))
				.OrderBy(
					path => path,
					StringComparer.OrdinalIgnoreCase
				)
				.ToArray();
		}

		public static bool TryAnalyzeClip(
			AudioClip                       clip,
			float                           silenceThresholdDb,
			out AudioNormalizationStats     stats,
			out string                      error
		)
		{
			stats = default;
			error = string.Empty;

			if (!TryLoadClipSamples(
				    clip,
				    out var samples,
				    out error
			    ))
				return false;

			stats = AnalyzeSamples(
				samples,
				clip.channels,
				clip.frequency,
				silenceThresholdDb
			);
			return true;
		}

		public static bool TryCreateNormalizedWavData(
			AudioClip                   clip,
			float                       targetActiveRmsDb,
			float                       maxPeakDb,
			float                       silenceThresholdDb,
			out byte[]                  wavData,
			out AudioNormalizationStats stats,
			out AudioNormalizationPlan  plan,
			out string                  error
		)
		{
			wavData = Array.Empty<byte>();
			stats   = default;
			plan    = default;
			error   = string.Empty;

			if (!TryLoadClipSamples(
				    clip,
				    out var samples,
				    out error
			    ))
				return false;

			stats = AnalyzeSamples(
				samples,
				clip.channels,
				clip.frequency,
				silenceThresholdDb
			);
			plan = CreateNormalizationPlan(
				stats,
				targetActiveRmsDb,
				maxPeakDb
			);

			wavData = EditorAudioWavUtility.CreatePcm16WavData(
				samples,
				clip.channels,
				clip.frequency,
				plan.GainLinear
			);
			return true;
		}

		public static int ApplyClipReplacementMapToEditorAudioFeedbackSettings(
			IReadOnlyDictionary<string, string> replacementAssetPaths
		)
		{
			if (replacementAssetPaths == null)
				throw new ArgumentNullException(
					nameof(replacementAssetPaths)
				);

			var settings         = EditorAudioFeedbackSettings.instance;
			var serializedObject = new SerializedObject(
				settings
			);
			var changedClipCount = 0;

			serializedObject.Update();

			foreach (var propertyName in EditorAudioFeedbackClipPropertyNames)
			{
				var property = serializedObject.FindProperty(
					propertyName
				);
				var clip = property?.objectReferenceValue as AudioClip;
				if (clip == null) continue;

				var sourceAssetPath = NormalizeAssetPath(
					AssetDatabase.GetAssetPath(
						clip
					)
				);
				if (!replacementAssetPaths.TryGetValue(
					    sourceAssetPath,
					    out var replacementAssetPath
				    ))
					continue;

				var replacementClip = AssetDatabase.LoadAssetAtPath<AudioClip>(
					replacementAssetPath
				);
				if (replacementClip == null) continue;

				property!.objectReferenceValue = replacementClip;
				changedClipCount++;
			}

			if (changedClipCount == 0) return 0;

			serializedObject.ApplyModifiedProperties();
			EditorUtility.SetDirty(
				settings
			);
			settings.SaveSettings();
			return changedClipCount;
		}

		public static string AssetPathToFullPath(string assetPath)
		{
			var projectRoot = Directory.GetParent(
					UnityEngine.Application.dataPath
				)
				?.FullName;
			if (string.IsNullOrWhiteSpace(
				    projectRoot
			    ))
				throw new DirectoryNotFoundException(
					"Could not resolve the Unity project root."
				);

			return Path.GetFullPath(
				Path.Combine(
					projectRoot,
					NormalizeAssetPath(
						assetPath
					)
				)
			);
		}

		public static string NormalizeAssetPath(string assetPath) =>
			assetPath.Replace(
				'\\',
				'/'
			);

		public static float DbToAmplitude(float db)
		{
			ValidateDbValue(
				db,
				nameof(db)
			);

			return Mathf.Pow(
				10f,
				db / 20f
			);
		}

		public static float AmplitudeToDb(float amplitude)
		{
			if (amplitude <= 0f) return float.NegativeInfinity;

			return 20f * Mathf.Log10(
				amplitude
			);
		}

		private static readonly string[] EditorAudioFeedbackClipPropertyNames =
		{
			"compilationStartClip",
			"compilationEndClip",
			"unityFullyInitializedClip",
			"buildStartedClip",
			"buildCompletedSuccessfullyClip",
			"buildFailedClip",
			"testStartClip",
			"testEndClip"
		};

		private static bool TryLoadClipSamples(
			AudioClip   clip,
			out float[] samples,
			out string  error
		)
		{
			samples = Array.Empty<float>();
			error   = string.Empty;

			if (clip == null)
			{
				error = "Audio clip is missing.";
				return false;
			}

			if (clip.loadState == AudioDataLoadState.Unloaded
			    && !clip.LoadAudioData())
			{
				error = $"Could not load audio data for {clip.name}.";
				return false;
			}

			if (clip.loadState != AudioDataLoadState.Loaded)
			{
				error = $"{clip.name} audio data is not loaded.";
				return false;
			}

			if (clip.samples <= 0 || clip.channels <= 0)
			{
				error = $"{clip.name} does not contain readable samples.";
				return false;
			}

			samples = new float[clip.samples * clip.channels];
			if (clip.GetData(
				    samples,
				    0
			    ))
				return true;

			error = $"Could not read samples from {clip.name}.";
			return false;
		}

		private static bool IsDirectChildOfFolder(string assetPath, string folderAssetPath) =>
			string.Equals(
				NormalizeAssetPath(
					Path.GetDirectoryName(
						assetPath
					) ?? string.Empty
				),
				folderAssetPath,
				StringComparison.OrdinalIgnoreCase
			);

		private static void ValidateDbValue(float db, string parameterName)
		{
			if (float.IsNaN(
				    db
			    ) || float.IsInfinity(
				    db
			    ))
				throw new ArgumentOutOfRangeException(
					parameterName
				);
		}
	}
}
