using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Bezoro.Unity.Core.Common.Helpers;

/// <summary>
///     Bridges async operations with Unity-friendly task helpers.
/// </summary>
public static class AsyncHelper
{
	public static async UniTask ExecuteAfterDelay(Action action, float delayS, CancellationToken ct = default)
	{
		if (action == null)
			throw new ArgumentNullException(nameof(action));

		await UniTask.Delay(TimeSpan.FromSeconds(delayS), cancellationToken: ct);

		if (!ct.IsCancellationRequested) action.Invoke();
	}

	public static async UniTask ExecuteAtIntervals(
		Func<UniTask>     action,
		float             intervalS,
		CancellationToken ct = default)
	{
		if (action == null)
			throw new ArgumentNullException(nameof(action));

		while (true)
		{
			if (ct.IsCancellationRequested) break;

			await action();
			await UniTask.Delay(TimeSpan.FromSeconds(intervalS), cancellationToken: ct);
		}
	}
}
