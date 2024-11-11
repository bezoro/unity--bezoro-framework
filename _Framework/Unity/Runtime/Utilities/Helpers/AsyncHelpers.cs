using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Bezoro.Framework.Unity.Utilities.Helpers
{
	public static class AsyncHelpers
	{
		public static async UniTask DelayedAction
		(
			Action action,
			float delayInSeconds,
			CancellationToken cancellationToken = default
		)
		{
			await UniTask.Delay(TimeSpan.FromSeconds(delayInSeconds), cancellationToken: cancellationToken);

			if (!cancellationToken.IsCancellationRequested)
			{
				action?.Invoke();
			}
		}

		public static async UniTask RepeatActionAsync
		(
			Func<UniTask> action,
			float intervalInSeconds,
			CancellationToken cancellationToken = default
		)
		{
			while (!cancellationToken.IsCancellationRequested)
			{
				await action();
				await UniTask.Delay(TimeSpan.FromSeconds(intervalInSeconds), cancellationToken: cancellationToken);
			}
		}
	}
}
