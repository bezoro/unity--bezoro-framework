using System.Threading;
using Bezoro.Unity.Core.Attributes;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Bezoro.Unity.Core.MonoBehaviours
{
	/// <summary>
	///     Fades an AudioSource in or out using a configurable curve and duration.
	/// </summary>
	public class AudioFader : MonoBehaviour
	{
		[EditorGetComponent(typeof(AudioSource))]
		[SerializeField] private AudioSource? audioSource;

		[SerializeField] private bool           fadeIn;
		[SerializeField] private bool           fadeOut;
		[SerializeField] private float          fadeDuration = 1f;
		[SerializeField] private AnimationCurve fadeCurve    = AnimationCurve.EaseInOut(0, 0, 1, 1);

		[Range(0f, 1f)]
		[SerializeField] private float soundStartPosition;

		private bool                     _isFadingOut;
		private CancellationTokenSource? _cts;
		private float                    _originalVolume;
		private readonly AudioFaderModel _model = new();

		#region Unity Lifecycle

		private void Awake()
		{
			_cts = new();

			if (audioSource != null)
			{
				_originalVolume = audioSource.volume;
				SetAudioStartPosition();
			}

			if (_model.ShouldStartFadeIn(fadeIn, audioSource != null))
				DoFadeInAsync(
					_cts.Token
				).Forget();

			if (_model.ShouldTrackFadeOut(fadeOut, audioSource != null, audioSource != null && audioSource.clip != null))
				TrackAudioProgressAsync(
					_cts.Token
				).Forget();
		}

		private void OnDestroy()
		{
			// cancel all running fades
			if (_cts == null)
				return;

			_cts.Cancel();
			_cts.Dispose();
		}

		#endregion

		public void TriggerFadeOut()
		{
			if (!_model.ShouldTriggerFadeOut(_cts != null, _cts != null && _cts.IsCancellationRequested, _isFadingOut))
				return;

			DoFadeOutAsync(
				_cts!.Token
			).Forget();
		}

		private async UniTaskVoid DoFadeInAsync(CancellationToken ct)
		{
			if (audioSource == null) return;

			audioSource.volume = 0f;
			var currentTime = 0f;

			while (currentTime < fadeDuration)
			{
				if (ct.IsCancellationRequested) return;

				currentTime += Time.deltaTime;
				audioSource.volume = _model.GetFadeInVolume(currentTime, fadeDuration, _originalVolume);

				await UniTask.Yield(ct);
			}

			audioSource.volume = _originalVolume;
		}

		private async UniTaskVoid DoFadeOutAsync(CancellationToken ct)
		{
			if (audioSource == null || _isFadingOut) return;

			_isFadingOut = true;

			var   currentTime = 0f;
			float startVolume = audioSource.volume;

			while (currentTime < fadeDuration && audioSource.isPlaying)
			{
				if (ct.IsCancellationRequested) return;

				currentTime += Time.deltaTime;
				float normalized = _model.GetNormalizedFadeProgress(currentTime, fadeDuration);
				float curveVal = fadeCurve.Evaluate(
					normalized
				);

				audioSource.volume = _model.GetFadeOutVolume(startVolume, curveVal);

				await UniTask.Yield(ct);
			}

			if (audioSource != null)
				audioSource.volume = 0f;
		}

		private async UniTaskVoid TrackAudioProgressAsync(CancellationToken ct)
		{
			if (audioSource == null || audioSource.clip == null) return;

			await UniTask.WaitUntil(() => audioSource.isPlaying, cancellationToken: ct);

			float clipLen   = audioSource.clip.length;
			float fadeStart = _model.GetFadeOutStartTime(clipLen, fadeDuration);

			while (audioSource.isPlaying && audioSource.time < fadeStart && !_isFadingOut)
			{
				if (ct.IsCancellationRequested) return;

				await UniTask.Yield(ct);
			}

			if (audioSource.isPlaying && !_isFadingOut)
				DoFadeOutAsync(ct).Forget();
		}

		private void SetAudioStartPosition()
		{
			if (audioSource != null && audioSource.clip != null)
			{
				float clipLength = audioSource.clip.length;
				audioSource.time = _model.GetAudioStartTime(soundStartPosition, clipLength);
			}
		}
	}

	public sealed class AudioFaderModel
	{
		public bool ShouldStartFadeIn(bool fadeIn, bool hasAudioSource)
		{
			return fadeIn && hasAudioSource;
		}

		public bool ShouldTrackFadeOut(bool fadeOut, bool hasAudioSource, bool hasClip)
		{
			return fadeOut && hasAudioSource && hasClip;
		}

		public bool ShouldTriggerFadeOut(bool hasCancellationSource, bool isCancellationRequested, bool isFadingOut)
		{
			return hasCancellationSource && !isCancellationRequested && !isFadingOut;
		}

		public float GetAudioStartTime(float soundStartPosition, float clipLength)
		{
			return soundStartPosition * clipLength;
		}

		public float GetFadeOutStartTime(float clipLength, float fadeDuration)
		{
			return clipLength - fadeDuration;
		}

		public float GetNormalizedFadeProgress(float currentTime, float fadeDuration)
		{
			return currentTime / fadeDuration;
		}

		public float GetFadeInVolume(float currentTime, float fadeDuration, float originalVolume)
		{
			return Mathf.Lerp(0f, originalVolume, GetNormalizedFadeProgress(currentTime, fadeDuration));
		}

		public float GetFadeOutVolume(float startVolume, float curveValue)
		{
			return Mathf.Lerp(startVolume, 0f, curveValue);
		}
	}
}
