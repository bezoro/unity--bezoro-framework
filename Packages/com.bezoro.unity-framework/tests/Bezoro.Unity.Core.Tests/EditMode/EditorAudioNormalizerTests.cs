using Bezoro.Unity.Core.Editor.Tools.AudioNormalization;
using NUnit.Framework;

namespace Bezoro.Unity.Core.Tests.EditMode
{
	[TestFixture]
	public sealed class EditorAudioNormalizerTests
	{
		[Test]
		public void EditorAudioNormalizer_WhenAnalyzingSamples_ReportsPeakRmsAndActiveRms()
		{
			var stats = EditorAudioNormalizer.AnalyzeSamples(
				new[]
				{
					0f,
					0.5f,
					-0.5f,
					0.25f
				},
				1,
				44100,
				-40f
			);

			Assert.That(
				stats.PeakDb,
				Is.EqualTo(-6.0206f).Within(0.0001f)
			);
			Assert.That(
				stats.RmsDb,
				Is.EqualTo(-8.5194f).Within(0.0001f)
			);
			Assert.That(
				stats.ActiveRmsDb,
				Is.EqualTo(-7.27f).Within(0.001f)
			);
			Assert.That(
				stats.ActiveSamplePercent,
				Is.EqualTo(75f).Within(0.0001f)
			);
		}

		[Test]
		public void EditorAudioNormalizer_WhenTargetWouldClip_LimitsGainByPeakCeiling()
		{
			var stats = EditorAudioNormalizer.AnalyzeSamples(
				new[]
				{
					0.5f,
					0.25f
				},
				1,
				44100,
				-60f
			);

			var plan = EditorAudioNormalizer.CreateNormalizationPlan(
				stats,
				-2f,
				-1f
			);

			Assert.That(
				plan.IsPeakLimited,
				Is.True
			);
			Assert.That(
				plan.ResultingPeakDb,
				Is.EqualTo(-1f).Within(0.0001f)
			);
		}

		[Test]
		public void EditorAudioNormalizer_WhenNormalizingSamples_AppliesLinearGainAndClampsOutput()
		{
			var normalizedSamples = EditorAudioNormalizer.NormalizeSamples(
				new[]
				{
					0.75f,
					-0.75f,
					0.25f
				},
				2f
			);

			Assert.That(
				normalizedSamples,
				Is.EqualTo(
					new[]
					{
						1f,
						-1f,
						0.5f
					}
				).Within(0.0001f)
			);
		}

		[Test]
		public void EditorAudioNormalizer_WhenReplacingMp3Original_UsesWavPathForReplacement()
		{
			var replacementPath = EditorAudioNormalizer.GetReplacementAssetPath(
				"Assets/Audio/VO_F_BuildFinished_01.mp3"
			);

			Assert.That(
				replacementPath,
				Is.EqualTo("Assets/Audio/VO_F_BuildFinished_01.wav")
			);
		}
	}
}
