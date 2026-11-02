using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace Bezoro.Unity.Core.Editor.Utilities
{
	internal static class EditorAudioWavUtility
	{
		private const int PCM_BITS_PER_SAMPLE = 16;
		private const int PCM_BYTES_PER_SAMPLE = PCM_BITS_PER_SAMPLE / 8;
		private const int WAV_HEADER_SIZE = 44;

		public static byte[] CreatePcm16WavData(
			float[] samples,
			int     channelCount,
			int     frequency,
			float   gain
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
			if (gain < 0f || float.IsNaN(
				    gain
			    ) || float.IsInfinity(
				    gain
			    ))
				throw new ArgumentOutOfRangeException(
					nameof(gain)
				);

			var dataByteCount = samples.Length * PCM_BYTES_PER_SAMPLE;
			var wavData       = new byte[WAV_HEADER_SIZE + dataByteCount];

			using var stream = new MemoryStream(
				wavData
			);
			using var writer = new BinaryWriter(
				stream,
				Encoding.ASCII
			);

			writer.Write(
				Encoding.ASCII.GetBytes(
					"RIFF"
				)
			);
			writer.Write(
				36 + dataByteCount
			);
			writer.Write(
				Encoding.ASCII.GetBytes(
					"WAVE"
				)
			);
			writer.Write(
				Encoding.ASCII.GetBytes(
					"fmt "
				)
			);
			writer.Write(
				16
			);
			writer.Write(
				(short)1
			);
			writer.Write(
				(short)channelCount
			);
			writer.Write(
				frequency
			);
			writer.Write(
				frequency * channelCount * PCM_BYTES_PER_SAMPLE
			);
			writer.Write(
				(short)(channelCount * PCM_BYTES_PER_SAMPLE)
			);
			writer.Write(
				(short)PCM_BITS_PER_SAMPLE
			);
			writer.Write(
				Encoding.ASCII.GetBytes(
					"data"
				)
			);
			writer.Write(
				dataByteCount
			);

			foreach (var sample in samples)
				writer.Write(
					ConvertToPcm16(
						sample,
						gain
					)
				);

			return wavData;
		}

		private static short ConvertToPcm16(float sample, float gain)
		{
			var scaledSample = Mathf.Clamp(
				sample * gain,
				-1f,
				1f
			);
			var sampleValue = Mathf.RoundToInt(
				scaledSample * short.MaxValue
			);

			return (short)Mathf.Clamp(
				sampleValue,
				short.MinValue,
				short.MaxValue
			);
		}
	}
}
