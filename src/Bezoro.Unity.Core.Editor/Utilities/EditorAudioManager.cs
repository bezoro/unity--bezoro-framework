using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using UnityEditor;
using UnityEngine;

namespace Bezoro.Unity.Core.Editor.Utilities
{
	/// <summary>
	///     Plays editor feedback audio clips.
	/// </summary>
	public static class EditorAudioManager
	{
		private const string MCI_ALIAS = "BezoroEditorAudioFeedback";

		private static readonly object MciLock = new();

		private static Timer? _mciCloseTimer;
		private static string? _currentTemporaryPlaybackPath;

		static EditorAudioManager()
		{
			EditorApplication.quitting += StopNativePlayback;
		}

		public static void PlayClip(AudioClip clip) =>
			PlayClip(
				clip,
				1f
			);

		public static void PlayClip(AudioClip clip, float volume)
		{
			if (clip == null)
				throw new ArgumentNullException(
					nameof(clip)
				);

			volume = Mathf.Clamp01(
				volume
			);

			try
			{
				if (volume <= 0f)
				{
					StopNativePlayback();
					return;
				}

				if (TryPlayNative(
					    clip,
					    volume
				    ))
					return;

				Debug.LogError(
					"Error playing compilation sound: attenuated editor audio playback is only supported for audio clips with project file paths on Windows."
				);
			}
			catch (Exception ex)
			{
				Debug.LogError(
					$"Error playing compilation sound: {ex.Message}"
				);
			}
		}

		private static bool TryPlayNative(AudioClip clip, float volume)
		{
			if (UnityEngine.Application.platform != RuntimePlatform.WindowsEditor) return false;

			if (volume < 1f)
			{
				return TryCreateAttenuatedWavFile(
					       clip,
					       volume,
					       out var attenuatedWavPath
				       )
					&& TryPlayMci(
						attenuatedWavPath,
						clip.length,
						true
					);
			}

			var assetPath = AssetDatabase.GetAssetPath(
				clip
			);
			if (string.IsNullOrWhiteSpace(
				    assetPath
			    ))
				return false;

			var projectRoot = Directory.GetParent(
					UnityEngine.Application.dataPath
				)
				?.FullName;
			if (string.IsNullOrWhiteSpace(
				    projectRoot
			    ))
				return false;

			var fullPath = Path.GetFullPath(
				Path.Combine(
					projectRoot,
					assetPath
				)
			);
			if (!File.Exists(
				    fullPath
			    ))
				return false;

			return TryPlayMci(
				fullPath,
				clip.length,
				false
			);
		}

		private static bool TryCreateAttenuatedWavFile(AudioClip clip, float volume, out string wavPath)
		{
			wavPath = string.Empty;

			if (!TryLoadClipSamples(
				    clip,
				    out var samples
			    ))
				return false;

			var directory = Path.Combine(
				Path.GetTempPath(),
				nameof(Bezoro),
				nameof(EditorAudioManager)
			);
			Directory.CreateDirectory(
				directory
			);

			wavPath = Path.Combine(
				directory,
				$"{Guid.NewGuid():N}.wav"
			);

			File.WriteAllBytes(
				wavPath,
				CreatePcm16WavData(
					samples,
					clip.channels,
					clip.frequency,
					volume
				)
			);

			return true;
		}

		private static bool TryLoadClipSamples(AudioClip clip, out float[] samples)
		{
			samples = Array.Empty<float>();

			if (clip.loadState == AudioDataLoadState.Unloaded
			    && !clip.LoadAudioData())
				return false;

			if (clip.loadState != AudioDataLoadState.Loaded) return false;
			if (clip.samples <= 0 || clip.channels <= 0) return false;

			samples = new float[clip.samples * clip.channels];
			return clip.GetData(
				samples,
				0
			);
		}

		internal static byte[] CreatePcm16WavData(
			float[] samples,
			int     channelCount,
			int     frequency,
			float   volume
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

			volume = Mathf.Clamp01(
				volume
			);

			return EditorAudioWavUtility.CreatePcm16WavData(
				samples,
				channelCount,
				frequency,
				volume
			);
		}

		private static bool TryPlayMci(
			string fullPath,
			float  clipLength,
			bool   deleteFileOnClose
		)
		{
			lock (MciLock)
			{
				CancelNativeCloseTimer();
				CloseNativePlayback();
				DeleteCurrentTemporaryPlaybackFile();

				var openErrorCode = SendMciCommand(
					GetOpenCommand(
						fullPath
					),
					false
				);
				if (openErrorCode != 0)
				{
					LogMciError(
						"opening",
						openErrorCode
					);
					DeleteTemporaryFile(
						fullPath,
						deleteFileOnClose
					);
					return false;
				}

				var playErrorCode = SendMciCommand(
					$"play {MCI_ALIAS} from 0",
					false
				);
				if (playErrorCode != 0)
				{
					LogMciError(
						"playing",
						playErrorCode
					);
					SendMciCommand(
						$"close {MCI_ALIAS}",
						false
					);
					DeleteTemporaryFile(
						fullPath,
						deleteFileOnClose
					);
					return false;
				}

				_currentTemporaryPlaybackPath = deleteFileOnClose
					? fullPath
					: null;

				ScheduleNativeClose(
					clipLength
				);
				return true;
			}
		}

		private static string GetOpenCommand(string fullPath)
		{
			var extension = Path.GetExtension(
					fullPath
				)
				.ToLowerInvariant();
			var deviceType = extension switch
			{
				".mp3" => "mpegvideo",
				".wav" => "waveaudio",
				_      => null
			};

			var quotedPath = QuoteMciValue(
				fullPath
			);

			return string.IsNullOrEmpty(
				       deviceType
			       )
				? $"open {quotedPath} alias {MCI_ALIAS}"
				: $"open {quotedPath} type {deviceType} alias {MCI_ALIAS}";
		}

		private static void ScheduleNativeClose(float clipLength)
		{
			var closeDelay = TimeSpan.FromSeconds(
				Math.Max(
					1d,
					clipLength + 1d
				)
			);

			_mciCloseTimer = new Timer(
				_ => StopNativePlayback(),
				null,
				closeDelay,
				Timeout.InfiniteTimeSpan
			);
		}

		private static void StopNativePlayback()
		{
			lock (MciLock)
			{
				CancelNativeCloseTimer();
				CloseNativePlayback();
				DeleteCurrentTemporaryPlaybackFile();
			}
		}

		private static void CloseNativePlayback() =>
			SendMciCommand(
				$"close {MCI_ALIAS}",
				false
			);

		private static void CancelNativeCloseTimer()
		{
			_mciCloseTimer?.Dispose();
			_mciCloseTimer = null;
		}

		private static void DeleteCurrentTemporaryPlaybackFile()
		{
			var temporaryPlaybackPath = _currentTemporaryPlaybackPath;
			_currentTemporaryPlaybackPath = null;

			DeleteTemporaryFile(
				temporaryPlaybackPath,
				true
			);
		}

		private static void DeleteTemporaryFile(string? path, bool shouldDelete)
		{
			if (!shouldDelete) return;
			if (string.IsNullOrWhiteSpace(
				    path
			    ))
				return;

			try
			{
				if (File.Exists(
					    path
				    ))
					File.Delete(
						path
					);
			}
			catch (IOException)
			{
				/* The file can remain until the next playback if the OS still holds it. */
			}
			catch (UnauthorizedAccessException)
			{
				/* The file can remain until the next playback if the OS still holds it. */
			}
		}

		private static string QuoteMciValue(string value) =>
			$"\"{value.Replace("\"", "\\\"")}\"";

		private static void LogMciError(string operation, int errorCode)
		{
			Debug.LogError(
				$"Error {operation} compilation sound through native audio playback: {GetMciErrorMessage(errorCode)}"
			);
		}

		private static string GetMciErrorMessage(int errorCode)
		{
			var errorText = new StringBuilder(
				256
			);

			return mciGetErrorString(
				errorCode,
				errorText,
				errorText.Capacity
			)
				? errorText.ToString()
				: $"MCI error {errorCode}";
		}

		private static int SendMciCommand(string command, bool returnValue)
		{
			var resultBuffer = returnValue
				? new StringBuilder(
					256
				)
				: null;

			return mciSendString(
				command,
				resultBuffer,
				resultBuffer?.Capacity ?? 0,
				IntPtr.Zero
			);
		}

		[DllImport("winmm.dll", CharSet = CharSet.Unicode)]
		private static extern int mciSendString(
			string command,
			StringBuilder? returnValue,
			int returnLength,
			IntPtr callback
		);

		[DllImport("winmm.dll", CharSet = CharSet.Unicode)]
		private static extern bool mciGetErrorString(
			int errorCode,
			StringBuilder errorText,
			int errorTextLength
		);
	}
}
