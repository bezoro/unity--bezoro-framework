using System.Collections;
using System.Diagnostics.CodeAnalysis;
using Bezoro.Unity.Core.Common.Enums;
using UnityEngine;
using UnityEngine.Audio;

namespace Bezoro.Unity.Core.MonoBehaviours
{
	/// <summary>
	///     Plays background music and sound effects through shared AudioSources.
	/// </summary>
	[RequireComponent(typeof(SingletonScope), typeof(GlobalReference))]
	[DefaultExecutionOrder(ExecutionOrder.Manager)]
	public class AudioManager : MonoBehaviour
	{
		private const string MASTER_MIXER_GROUP_NAME    = "Master";
		private const string MASTER_MIXER_RESOURCE_PATH = "MIX_Master";

		private readonly AudioManagerModel _model = new();

		private AudioClip? _currentBGM;
		private float      _bgmVolume = 1f;

		public static bool IsAvailable => TryGetInstance(out _);

		public static AudioManager Instance => GlobalReferenceAccess<AudioManager>.Instance;

		private AudioSource? BGMAudioSource { get; set; }
		private AudioSource? SFXAudioSource { get; set; }

		public static bool TryGetInstance([NotNullWhen(true)] out AudioManager? instance) =>
			GlobalReferenceAccess<AudioManager>.TryGetInstance(out instance);

		#region Unity Lifecycle

		private void Awake()
		{
			if (!GlobalReferenceAccess<AudioManager>.IsCurrent(this))
				return;

			SFXAudioSource             =  gameObject.AddComponent<AudioSource>();
			BGMAudioSource             =  gameObject.AddComponent<AudioSource>();
			BGMAudioSource.loop        =  _model.GetBGMLoop();
			BGMAudioSource.playOnAwake =  _model.GetBGMPlayOnAwake();
			BGMAudioSource.volume      =  _model.GetConfiguredBGMVolume(BGMAudioSource.volume, _bgmVolume);

			AssignMasterMixerGroup();
		}

		#endregion

		public void PlayBGM(AudioClip? bgmClip, float volume = 1f, float fadeInDuration = 0f)
		{
			if (!_model.ShouldPlayBGM(BGMAudioSource != null)) return;

			if (_model.ShouldSkipBGM(_currentBGM == bgmClip, BGMAudioSource!.isPlaying)) return;

			if (_model.ShouldFade(fadeInDuration))
			{
				StartCoroutine(
					FadeBGM(
						bgmClip,
						volume,
						fadeInDuration
					)
				);
			}
			else
			{
				BGMAudioSource.Stop();
				BGMAudioSource.clip   = bgmClip;
				BGMAudioSource.volume = volume;
				_bgmVolume            = volume;

				if (bgmClip != null) BGMAudioSource.Play();
			}

			_currentBGM = bgmClip;
		}

		public void PlaySFX(AudioClip? sfxClip, float volume = 1f)
		{
			if (!_model.ShouldPlaySFX(SFXAudioSource != null, sfxClip != null)) return;

			SFXAudioSource!.volume = volume;
			SFXAudioSource.PlayOneShot(
				sfxClip!
			);
		}

		public void SetBGMVolume(float volume)
		{
			_bgmVolume = _model.GetClampedVolume(volume);

			if (BGMAudioSource != null) BGMAudioSource.volume = _bgmVolume;
		}

		public void StopBGM(float fadeOutDuration = 0f)
		{
			var bgmAudioSource = BGMAudioSource;
			if (!_model.ShouldStopBGM(bgmAudioSource != null)) return;
			if (bgmAudioSource == null) return;

			if (_model.ShouldFade(fadeOutDuration))
			{
				StartCoroutine(
					FadeOutBGM(
						fadeOutDuration
					)
				);
			}
			else
			{
				bgmAudioSource.Stop();
				_currentBGM = null;
			}
		}

		private static AudioMixerGroup? GetMasterMixerGroup()
		{
			var model = new AudioManagerModel();
			var masterMixer = Resources.Load<AudioMixer>(MASTER_MIXER_RESOURCE_PATH);
			if (masterMixer == null)
				return null;

			AudioMixerGroup[] groups = masterMixer.FindMatchingGroups(MASTER_MIXER_GROUP_NAME);
			for (var i = 0; i < groups.Length; i++)
			{
				if (model.IsMasterMixerGroup(groups[i].name, MASTER_MIXER_GROUP_NAME))
					return groups[i];
			}

			return model.ShouldUseFallbackMixerGroup(groups.Length) ? groups[0] : null;
		}

		private IEnumerator FadeBGM(AudioClip? newClip, float targetVolume, float duration)
		{
			if (!_model.ShouldPlayBGM(BGMAudioSource != null)) yield break;

			// Fade out current BGM
			float startVolume = BGMAudioSource!.volume;
			var   elapsed     = 0f;

			float halfDuration = _model.GetHalfFadeDuration(duration);
			while (_model.ShouldContinueFade(elapsed, halfDuration))
			{
				elapsed += Time.deltaTime;
				BGMAudioSource.volume = _model.GetFadeVolume(
					startVolume,
					0f,
					elapsed,
					halfDuration
				);

				yield return null;
			}

			// Switch to new BGM
			BGMAudioSource.Stop();
			BGMAudioSource.clip   = newClip;
			BGMAudioSource.volume = 0f;

			if (newClip != null) BGMAudioSource.Play();

			// Fade in new BGM
			elapsed = 0f;
			while (_model.ShouldContinueFade(elapsed, halfDuration))
			{
				elapsed += Time.deltaTime;
				BGMAudioSource.volume = _model.GetFadeVolume(
					0f,
					targetVolume,
					elapsed,
					halfDuration
				);

				yield return null;
			}

			BGMAudioSource.volume = targetVolume;
			_bgmVolume            = targetVolume;
		}

		private IEnumerator FadeOutBGM(float duration)
		{
			if (!_model.ShouldStopBGM(BGMAudioSource != null)) yield break;

			float startVolume = BGMAudioSource!.volume;
			var   elapsed     = 0f;

			while (_model.ShouldContinueFade(elapsed, duration))
			{
				elapsed += Time.deltaTime;
				BGMAudioSource.volume = _model.GetFadeVolume(
					startVolume,
					0f,
					elapsed,
					duration
				);

				yield return null;
			}

			BGMAudioSource.Stop();
			BGMAudioSource.volume = _bgmVolume; // Restore volume for next BGM
			_currentBGM           = null;
		}

		private void AssignMasterMixerGroup()
		{
			var masterGroup = GetMasterMixerGroup();
			if (!_model.ShouldAssignMixerGroup(masterGroup != null))
				return;

			if (BGMAudioSource != null)
				BGMAudioSource.outputAudioMixerGroup = masterGroup;

			if (SFXAudioSource != null)
				SFXAudioSource.outputAudioMixerGroup = masterGroup;
		}
	}

	public sealed class AudioManagerModel
	{
		public bool GetBGMLoop()
		{
			return true;
		}

		public bool GetBGMPlayOnAwake()
		{
			return false;
		}

		public float GetConfiguredBGMVolume(float sourceVolume, float configuredVolume)
		{
			return sourceVolume * configuredVolume;
		}

		public bool ShouldPlayBGM(bool hasAudioSource)
		{
			return hasAudioSource;
		}

		public bool ShouldSkipBGM(bool isSameClip, bool isPlaying)
		{
			return isSameClip && isPlaying;
		}

		public bool ShouldPlaySFX(bool hasAudioSource, bool hasClip)
		{
			return hasAudioSource && hasClip;
		}

		public bool ShouldStopBGM(bool hasAudioSource)
		{
			return hasAudioSource;
		}

		public bool ShouldFade(float duration)
		{
			return duration > 0f;
		}

		public bool ShouldContinueFade(float elapsed, float duration)
		{
			return elapsed < duration;
		}

		public float GetHalfFadeDuration(float duration)
		{
			return duration * 0.5f;
		}

		public float GetFadeVolume(float startVolume, float targetVolume, float elapsed, float duration)
		{
			return Mathf.Lerp(startVolume, targetVolume, elapsed / duration);
		}

		public float GetClampedVolume(float volume)
		{
			return Mathf.Clamp01(volume);
		}

		public bool IsMasterMixerGroup(string groupName, string masterGroupName)
		{
			return groupName == masterGroupName;
		}

		public bool ShouldUseFallbackMixerGroup(int groupCount)
		{
			return groupCount > 0;
		}

		public bool ShouldAssignMixerGroup(bool hasMixerGroup)
		{
			return hasMixerGroup;
		}
	}
}
