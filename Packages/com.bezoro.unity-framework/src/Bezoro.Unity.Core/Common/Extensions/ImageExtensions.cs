using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Bezoro.Unity.Core.Common.Extensions
{
	/// <summary>
	///     Contains extension methods for Image.
	/// </summary>
	public static class ImageExtensions
	{
		public static async UniTask<AsyncProgressHandle> DoFadeToAlpha(
			this Image        image,
			float             endAlpha,
			float             durationS,
			float             startDelayS = 0f,
			AnimationCurve?   fadeCurve   = null,
			CancellationToken ct          = default)
		{
			var handle = new AsyncProgressHandle();

			if (image == null || image.gameObject == null)
				return handle;

			var endColor = new Color(
				image.color.r,
				image.color.g,
				image.color.b,
				endAlpha
			);

			return await image.DoFadeToColor(
					   endColor,
					   durationS,
					   startDelayS,
					   fadeCurve,
					   ct
				   );
		}

		public static async UniTask<AsyncProgressHandle> DoFadeToColor(
			this Image        image,
			Color             endColor,
			float             durationS,
			float             startDelayS = 0f,
			AnimationCurve?   fadeCurve   = null,
			CancellationToken ct          = default)
		{
			var handle = new AsyncProgressHandle();

			if (image == null || image.gameObject == null)
				return handle;

			await UniTask.Delay(
				TimeSpan.FromSeconds(
					startDelayS
				),
				cancellationToken: ct
			);

			var startColor  = image.color;
			var elapsedTime = 0f;

			while (elapsedTime < durationS)
			{
				if (ct.IsCancellationRequested) return handle;
				if (!UnityEngine.Application.isPlaying) return handle;
				if (image == null || image.gameObject == null) return handle;

				elapsedTime += Time.deltaTime;
				float t = elapsedTime / durationS;
				float curveValue = fadeCurve != null
									   ? Mathf.Clamp01(
										   fadeCurve.Evaluate(
											   t
										   )
									   )
									   : t;

				var c = Color.Lerp(
					startColor,
					endColor,
					curveValue
				);

				image.color = c;

				handle.Progress = t;

				await UniTask.Yield(
					PlayerLoopTiming.Update,
					ct
				);
			}

			image.color = endColor;

			handle.Progress = 1f;
			return handle;
		}

		public static void SetColor(this Image image, Color color)
		{
			if (image == null) return;

			image.color = color;
		}
	}

	/// <summary>
	///     Represents immutable struct data.
	/// </summary>
	public record struct AsyncProgressHandle
	{
		public float Progress { get; internal set; }
	}
}
