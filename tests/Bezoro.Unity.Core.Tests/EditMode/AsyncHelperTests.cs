using System;
using System.Threading;
using System.Threading.Tasks;
using Bezoro.Unity.Core.Common.Helpers;
using Cysharp.Threading.Tasks;
using NUnit.Framework;

namespace Bezoro.Unity.Core.Tests.EditMode
{
	[TestFixture]
	public sealed class AsyncHelperTests
	{
		[Test]
		public async Task ExecuteAfterDelay_InvokesActionAfterDelay()
		{
			var invoked = false;

			await AsyncHelper.ExecuteAfterDelay(() => invoked = true, 0f);

			Assert.IsTrue(invoked);
		}

		[Test]
		public void ExecuteAfterDelay_WhenActionIsNull_ThrowsArgumentNullException()
		{
			Assert.ThrowsAsync<ArgumentNullException>(async () => await AsyncHelper.ExecuteAfterDelay(null, 0f));
		}

		[Test]
		public void ExecuteAtIntervals_WhenActionIsNull_ThrowsArgumentNullException()
		{
			Assert.ThrowsAsync<ArgumentNullException>(async () => await AsyncHelper.ExecuteAtIntervals(null, 0f)
			);
		}

		[Test]
		public async Task ExecuteAtIntervals_WhenTokenIsAlreadyCancelled_DoesNotInvokeAction()
		{
			using var cancellationTokenSource = new CancellationTokenSource();
			cancellationTokenSource.Cancel();

			var invokeCount = 0;

			await AsyncHelper.ExecuteAtIntervals(
				() =>
				{
					invokeCount++;
					return UniTask.CompletedTask;
				},
				0f,
				cancellationTokenSource.Token
			);

			Assert.AreEqual(0, invokeCount);
		}
	}
}
