using System;
using System.Threading;
using System.Threading.Tasks;
using Bezoro.Unity.Core.AsyncLoading;
using NUnit.Framework;

namespace Bezoro.Unity.Core.Tests.EditMode
{
	[TestFixture]
	public sealed class AsyncLoadingFrameBudgetTests
	{
		[Test]
		public async Task WaitIfNeededAsync_WhenCancellationTokenIsActive_Completes()
		{
			var budget = new AsyncLoadingFrameBudget();

			await budget.WaitIfNeededAsync(CancellationToken.None);

			Assert.Pass();
		}

		[Test]
		public void WaitIfNeededAsync_WhenCancellationTokenIsCanceled_ThrowsOperationCanceledException()
		{
			var       budget = new AsyncLoadingFrameBudget();
			using var cts    = new CancellationTokenSource();
			cts.Cancel();

			Assert.CatchAsync<OperationCanceledException>(async () => await budget.WaitIfNeededAsync(cts.Token)
			);
		}
	}
}
