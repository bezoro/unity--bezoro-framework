using System;
using Bezoro.Unity.Core.Common.Timing;
using NUnit.Framework;
using Cysharp.Threading.Tasks;

namespace Bezoro.Unity.Core.Tests.EditMode
{
	/// <summary>
	///     Verifies manual custom fixed-step ticking.
	/// </summary>
	[TestFixture]
	public sealed class CustomFixedTickTests
	{
		[Test]
		public async System.Threading.Tasks.Task RunAsync_ExternalCancellationAndStopAllowRestart()
		{
			var count = 0;
			var ticker = new CustomFixedTick(1f, () => count++);
			using var external = new System.Threading.CancellationTokenSource();
			var first = ticker.RunAsync(() => 1f, external.Token);
			external.Cancel();
			await first.SuppressCancellationThrow();
			Assert.IsFalse(ticker.IsSelfDriven);
			ticker.Tick(1f);
			var second = ticker.RunAsync(() => 1f);
			ticker.Stop();
			var third = ticker.RunAsync(() => 1f);
			await second.SuppressCancellationThrow();
			Assert.IsTrue(ticker.IsSelfDriven);
			ticker.Stop();
			await third.SuppressCancellationThrow();
			Assert.IsFalse(ticker.IsSelfDriven);
			Assert.GreaterOrEqual(count, 4);
		}

		[Test]
		public void Constructor_WhenFixedDeltaTimeIsNotPositive_Throws()
		{
			Assert.Throws<ArgumentException>(() => new CustomFixedTick(0f,    () => { }));
			Assert.Throws<ArgumentException>(() => new CustomFixedTick(-0.1f, () => { }));
		}

		[Test]
		public void Reset_ClearsTimeCountersAndPendingAccumulator()
		{
			var callbackCount = 0;
			var ticker        = new CustomFixedTick(0.5f, () => callbackCount++);
			ticker.Tick(0.25f);

			ticker.Reset();
			ticker.Tick(0.25f);

			Assert.AreEqual(0, callbackCount);
			Assert.That(ticker.ReferenceTime, Is.EqualTo(0.25d).Within(0.000001d));
			Assert.That(ticker.FixedTime,     Is.EqualTo(0d).Within(0.000001d));
		}

		[Test]
		public void Tick_WhenAccumulatedDeltaReachesFixedStep_InvokesCallbackAndAdvancesFixedTime()
		{
			var callbackCount = 0;
			var ticker        = new CustomFixedTick(0.5f, () => callbackCount++);

			ticker.Tick(0.25f);
			bool completed = ticker.Tick(0.25f);

			Assert.IsTrue(completed);
			Assert.AreEqual(1, callbackCount);
			Assert.That(ticker.ReferenceTime, Is.EqualTo(0.5d).Within(0.000001d));
			Assert.That(ticker.FixedTime,     Is.EqualTo(0.5d).Within(0.000001d));
		}

		[Test]
		public void Tick_WhenDeltaIsBelowFixedStep_AccumulatesReferenceTimeWithoutCallback()
		{
			var callbackCount = 0;
			var ticker        = new CustomFixedTick(0.5f, () => callbackCount++);

			bool completed = ticker.Tick(0.25f);

			Assert.IsTrue(completed);
			Assert.AreEqual(0, callbackCount);
			Assert.That(ticker.ReferenceTime, Is.EqualTo(0.25d).Within(0.000001d));
			Assert.That(ticker.FixedTime,     Is.EqualTo(0d).Within(0.000001d));
		}

		[Test]
		public void Tick_WhenMaxTicksPerFrameIsReached_ReturnsFalseAndLeavesRemainderPending()
		{
			var callbackCount = 0;
			var ticker = new CustomFixedTick(0.1f, () => callbackCount++)
			{
				MaxTicksPerFrame   = 2,
				MaxAllowedTimestep = 0f
			};

			bool completed = ticker.Tick(0.35f);

			Assert.IsFalse(completed);
			Assert.AreEqual(2, callbackCount);
			Assert.That(ticker.FixedTime, Is.EqualTo(0.2d).Within(0.000001d));
		}
	}
}
