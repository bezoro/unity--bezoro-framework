using System;
using System.Reflection;
using Bezoro.Unity.Core.Editor.Utilities;
using NUnit.Framework;

namespace Bezoro.Unity.Core.Tests.EditMode
{
	[TestFixture]
	public sealed class EditorAudioManagerTests
	{
		[Test]
		public void EditorAudioManager_WhenCreatingAttenuatedWavData_ScalesPcmSamples()
		{
			var wavData = CreatePcm16WavData(
				new[]
				{
					1f,
					-1f,
					0.5f,
					-0.5f
				},
				1,
				44100,
				0.5f
			);

			Assert.That(
				ReadPcmSample(
					wavData,
					0
				),
				Is.EqualTo(16384)
			);
			Assert.That(
				ReadPcmSample(
					wavData,
					1
				),
				Is.EqualTo(-16384)
			);
			Assert.That(
				ReadPcmSample(
					wavData,
					2
				),
				Is.EqualTo(8192)
			);
			Assert.That(
				ReadPcmSample(
					wavData,
					3
				),
				Is.EqualTo(-8192)
			);
		}

		private static byte[] CreatePcm16WavData(
			float[] samples,
			int     channelCount,
			int     frequency,
			float   volume
		)
		{
			var method = typeof(EditorAudioManager).GetMethod(
				"CreatePcm16WavData",
				BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic
			);

			Assert.That(
				method,
				Is.Not.Null,
				$"{nameof(EditorAudioManager)} should create attenuated WAV data before native playback."
			);

			return (byte[])method!.Invoke(
				null,
				new object[]
				{
					samples,
					channelCount,
					frequency,
					volume
				}
			)!;
		}

		private static short ReadPcmSample(byte[] wavData, int sampleIndex) =>
			BitConverter.ToInt16(
				wavData,
				44 + sampleIndex * sizeof(short)
			);
	}
}
