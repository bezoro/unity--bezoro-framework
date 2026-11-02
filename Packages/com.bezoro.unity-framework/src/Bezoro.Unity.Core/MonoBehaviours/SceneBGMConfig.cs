using UnityEngine;

namespace Bezoro.Unity.Core.MonoBehaviours
{
	/// <summary>
	///     Stores Scene BGM Config values.
	/// </summary>
	public class SceneBGMConfig : MonoBehaviour
	{
		private readonly SceneBGMConfigModel _model = new();

		[Header(
			"BGM Configuration"
		)]

		[SerializeField] private AudioClip? bgmClip;
		[SerializeField] [Range(
			0f,
			1f
		)] private float volume = 1f;
		[SerializeField] private float fadeInDuration = 1f;
		[SerializeField] private bool  playOnStart    = true;

		public AudioClip? BGMClip        => bgmClip;
		public float      FadeInDuration => fadeInDuration;
		public float      Volume         => volume;

		#region Unity Lifecycle

		private void Start()
		{
			if (_model.ShouldPlayOnStart(playOnStart) &&
				AudioManager.TryGetInstance(out var instance))
				instance.PlayBGM(
					bgmClip,
					volume,
					fadeInDuration
				);
		}

		#endregion

		[ContextMenu("Play BGM")]
		public void PlayBGMManually()
		{
			if (!_model.ShouldRunManualPlay(UnityEngine.Application.isPlaying)) return;

			if (AudioManager.TryGetInstance(
					out var instance
				))
				instance.PlayBGM(
					bgmClip,
					volume,
					fadeInDuration
				);
		}

		[ContextMenu("Stop BGM")]
		public void Stop()
		{
			if (!_model.ShouldRunManualStop(UnityEngine.Application.isPlaying)) return;

			if (AudioManager.TryGetInstance(
					out var instance
				))
				instance.StopBGM(
					fadeInDuration
				);
		}
	}

	public sealed class SceneBGMConfigModel
	{
		public bool ShouldPlayOnStart(bool playOnStart) =>
			playOnStart;

		public bool ShouldRunManualPlay(bool isApplicationPlaying) =>
			isApplicationPlaying;

		public bool ShouldRunManualStop(bool isApplicationPlaying) =>
			isApplicationPlaying;
	}
}
