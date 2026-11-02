using System;
using System.Reflection;
using Bezoro.Unity.Core.Editor.Utilities;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Bezoro.Unity.Core.Tests.EditMode
{
	[TestFixture]
	public sealed class EditorAudioManagerTests
	{
		[Test]
		public void EditorAudioManager_WhenStoppingWithoutPlayback_IsSafeAndIdempotent()
		{
			var stop = typeof(EditorAudioManager).GetMethod(
				"StopNativePlayback",
				BindingFlags.Static | BindingFlags.NonPublic
			);
			Assert.That(stop, Is.Not.Null);

			// Exercise the same cleanup callback used when the Editor quits.
			Assert.DoesNotThrow(() => stop!.Invoke(null, null));
			Assert.DoesNotThrow(() => stop!.Invoke(null, null));
			LogAssert.NoUnexpectedReceived();
		}

		[Test]
		public void EditorAudioManager_WhenMuted_StopsWithoutNativeLibraryErrors()
		{
			var clip = AudioClip.Create("Muted editor feedback regression", 1, 1, 44100, false);
			try
			{
				Assert.DoesNotThrow(() => EditorAudioManager.PlayClip(clip, 0f));
				LogAssert.NoUnexpectedReceived();
			}
			finally { UnityEngine.Object.DestroyImmediate(clip); }
		}

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
