using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Bezoro.Unity.Core.AsyncLoading;

/// <summary>
///     Keeps async activation work inside a small frame-time budget.
/// </summary>
public sealed class AsyncLoadingFrameBudget
{
	private const double INITIAL_AVERAGE_MS    = 1000d / 60d;
	private const double MAX_BUDGET_FRACTION   = .35d;
	private const double MAX_BUDGET_MS         = 4d;
	private const double MIN_BUDGET_MS         = .25d;
	private const double PRESSURED_FRACTION    = .1d;
	private const double SEVERE_PRESSURE_RATIO = 1.35d;
	private const double SMOOTHING             = .1d;
	private       double _averageFrameMs       = INITIAL_AVERAGE_MS;
	private       double _frameStartTime;

	private int _frame;

	public AsyncLoadingFrameBudget()
	{
		ResetFrameBudget();
	}

	public async UniTask WaitIfNeededAsync(
		CancellationToken ct)
	{
		ct.ThrowIfCancellationRequested();

		if (Time.frameCount != _frame)
			ResetFrameBudget();

		double elapsedMs = (Time.realtimeSinceStartupAsDouble - _frameStartTime) * 1000d;
		if (elapsedMs < GetBudgetMs())
			return;

		await UniTask.Yield(ct);
		ResetFrameBudget();
	}

	private double GetBudgetMs()
	{
		float lastFrameMs = Time.unscaledDeltaTime * 1000f;
		double fraction = lastFrameMs > _averageFrameMs * SEVERE_PRESSURE_RATIO
							  ? PRESSURED_FRACTION
							  : MAX_BUDGET_FRACTION;

		return Math.Clamp(_averageFrameMs * fraction, MIN_BUDGET_MS, MAX_BUDGET_MS);
	}

	private void ResetFrameBudget()
	{
		double lastFrameMs = Time.unscaledDeltaTime * 1000d;
		if (lastFrameMs > 0d)
			_averageFrameMs = Math.Clamp(
				_averageFrameMs + (lastFrameMs - _averageFrameMs) * SMOOTHING,
				MIN_BUDGET_MS,
				1000d
			);

		_frame          = Time.frameCount;
		_frameStartTime = Time.realtimeSinceStartupAsDouble;
	}
}
