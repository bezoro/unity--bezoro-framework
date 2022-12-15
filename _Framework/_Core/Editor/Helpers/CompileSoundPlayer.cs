using Cysharp.Threading.Tasks;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Bezoro.Core.Editor.Helpers
{
	public static class CompileSoundPlayer
	{
		[DidReloadScripts]
		private static void PlaySound()
		{
			var go = new GameObject("CompileSFX")
					 {
						 hideFlags = HideFlags.HideAndDontSave
					 };

			_audioSource = go.AddComponent<AudioSource>();
			_audioSource.clip = Resources.Load<AudioClip>("SFX_Compiled");
			_audioSource.volume = 0.5f;
			_audioSource.Play();

			Async_DestroyAudioWhenFinishedPlaying();
		}

		private static async UniTask Async_DestroyAudioWhenFinishedPlaying()
		{
			await UniTask.WaitUntil(() => _audioSource.isPlaying == false);

			GameObject.DestroyImmediate(_audioSource.gameObject);
		}

		private static AudioSource _audioSource;
	}
}