using System;
using System.Diagnostics;
using System.Threading;
using Bezoro.Core.Extensions;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Logger = Bezoro.Logging.Logger;

namespace Bezoro.Unity.Core.Common.Timing
{
	/// <summary>
	///     Drives a custom fixed-step loop using a per-frame deltaTime.
	///     Returns false when the processing budget is exceeded so callers can defer remaining ticks.
	/// </summary>
	public sealed class CustomFixedTick
	{
		private readonly Action                  _callback;
		private readonly CancellationTokenSource _cts       = new();
		private readonly Stopwatch               _stopWatch = new();

		private double _accumulator;

		/// <param name="customFixedDeltaTime">Fixed step size in seconds, must be > 0.</param>
		/// <param name="customFixedTickCallback">Invoked once per fixed step.</param>
		public CustomFixedTick(float customFixedDeltaTime, Action customFixedTickCallback)
		{
			customFixedDeltaTime.ThrowIf(v => v <= 0
			);

			CustomFixedDeltaTime = customFixedDeltaTime;
			_callback            = customFixedTickCallback;

			FixedTime     = 0.0;
			ReferenceTime = 0.0;
			_accumulator  = 0.0;
		}

		~CustomFixedTick()
		{
			_cts.Cancel();
			_cts.Dispose();
		}

		/// <summary>Fixed step size in seconds.</summary>
		public float CustomFixedDeltaTime { get; }

		/// <summary>True while RunAsync is driving this instance.</summary>
		public bool IsSelfDriven { get; private set; }

		/// <summary>Accumulated fixed time advanced in fixed steps.</summary>
		public double FixedTime { get; private set; }

		/// <summary>Accumulated reference time from variable deltaTime.</summary>
		public double ReferenceTime { get; private set; }

		/// <summary>
		///     Processing time budget in seconds; set to &lt;= 0 for unlimited.
		/// </summary>
		public float MaxAllowedTimestep { get; set; } = 0.3f;

		/// <summary>
		///     Optional hard cap on number of fixed ticks processed per call; set to 0 for unlimited.
		/// </summary>
		public int MaxTicksPerFrame { get; set; } = 0;

		/// <summary>
		///     Convenience factory that creates an instance and starts the self-driven loop.
		/// </summary>
		public static CustomFixedTick StartNew(
			float             customFixedDeltaTime,
			Action            customFixedTickCallback,
			Func<float>?      deltaTimeProvider = null,
			CancellationToken token             = default,
			PlayerLoopTiming  timing            = PlayerLoopTiming.Update)
		{
			var instance = new CustomFixedTick(
				customFixedDeltaTime,
				customFixedTickCallback
			);

			instance.RunAsync(
				deltaTimeProvider,
				token,
				timing
			).Forget();

			return instance;
		}


		/// <summary>
		///     Feed variable deltaTime. Processes as many fixed steps as allowed by the budget/cap.
		///     Returns false if not all pending steps were processed this call.
		///     No-ops if this instance is currently self-driven by RunAsync.
		/// </summary>
		public bool Tick(float deltaTime)
		{
			if (IsSelfDriven)
			{
				Logger.LogWarning(
					"Calling Tick() on a CustomFixedTick instance that is currently self-driven. Ignoring."
				);

				return true;
			}

			return ProcessTick(
				deltaTime
			);
		}

		/// <summary>
		///     Runs a self-driven loop that feeds Tick() with values from deltaTimeProvider
		///     until the token is canceled. Caller chooses PlayerLoopTiming and cancellation.
		///     If already self-driven, subsequent calls are ignored.
		/// </summary>
		public async UniTask RunAsync(
			Func<float>?      deltaTimeProvider = null,
			CancellationToken cancellationToken = default,
			PlayerLoopTiming  playerLoopTiming  = PlayerLoopTiming.Update)
		{
			if (IsSelfDriven)
			{
				Logger.LogWarning(
					"Calling RunAsync() on a CustomFixedTick instance that is already running. Ignoring."
				);

				return;
			}

			IsSelfDriven = true;
			var getDeltaTime = deltaTimeProvider ?? (() => Time.deltaTime);

			using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
				cancellationToken,
				_cts.Token
			);

			while (!linkedCts.Token.IsCancellationRequested)
			{
				float deltaTime = getDeltaTime();
				ProcessTick(
					deltaTime
				);

				await UniTask.Yield(
					playerLoopTiming,
					linkedCts.Token
				);
			}
		}

		/// <summary>Resets all time counters and pending accumulator to zero.</summary>
		public void Reset()
		{
			FixedTime     = 0.0;
			ReferenceTime = 0.0;
			_accumulator  = 0.0;
			_stopWatch.Reset();
		}

		/// <summary>
		///     Stops the self-driven loop if it is running.
		/// </summary>
		public void Stop()
		{
			if (!IsSelfDriven) return;

			_cts.Cancel();
			IsSelfDriven = false;
		}

		private bool ProcessTick(float deltaTime)
		{
			if (float.IsNaN(
					deltaTime
				) ||
				float.IsInfinity(
					deltaTime
				) ||
				deltaTime <= 0f) return true;

			_stopWatch.Restart();

			ReferenceTime += deltaTime;
			_accumulator  += deltaTime;

			const double epsilon = 1e-12;

			double fixedDt           = CustomFixedDeltaTime;
			bool   hasTickCap        = MaxTicksPerFrame > 0;
			int    maxTicksPerFrame  = MaxTicksPerFrame;
			bool   hasTimeBudget     = MaxAllowedTimestep > 0f;
			double maxAllowedSeconds = MaxAllowedTimestep;
			var    callback          = _callback;

			var ticksProcessed = 0;

			while (_accumulator + epsilon >= fixedDt)
			{
				_accumulator -= fixedDt;
				FixedTime    += fixedDt;

				callback?.Invoke();
				ticksProcessed++;

				if (hasTickCap && ticksProcessed >= maxTicksPerFrame) return false;

				if (hasTimeBudget && _stopWatch.Elapsed.TotalSeconds > maxAllowedSeconds) return false;
			}

			return true;
		}
	}
}
