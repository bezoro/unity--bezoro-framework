using System;
using System.Threading;
using System.Threading.Tasks;
using Bezoro.Unity.Core.Common.Timing;
using NUnit.Framework;

namespace Bezoro.Unity.Core.Tests.EditMode
{
	/// <summary>
	///     Verifies main-thread helper contracts that can be exercised deterministically in EditMode.
	/// </summary>
	[TestFixture]
	public sealed class PlayerLoopHelpersTests
	{
		[Test]
		public void GetCurrentManagedThreadId_ReturnsCurrentThreadId()
		{
			int threadId = PlayerLoopHelpers.Get_Current_Managed_Thread_Id();

			Assert.AreEqual(Thread.CurrentThread.ManagedThreadId, threadId);
		}

		[Test]
		public void ReturnToMainThread_WhenActionIsNull_ThrowsArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(() => PlayerLoopHelpers.ReturnToMainThread(null));
		}

		[Test]
		public async Task RunOnMainThread_ExecutesAction()
		{
			var invoked = false;

			await PlayerLoopHelpers.Run_On_Main_Thread(() => invoked = true);

			Assert.IsTrue(invoked);
		}

		[Test]
		public async Task RunOnMainThread_ReturnsFuncResult()
		{
			int result = await PlayerLoopHelpers.Run_On_Main_Thread(() => 42);

			Assert.AreEqual(42, result);
		}

		[Test]
		public void RunOnMainThread_WhenActionIsNull_ThrowsArgumentNullException()
		{
			Assert.ThrowsAsync<ArgumentNullException>(async () => await PlayerLoopHelpers.Run_On_Main_Thread(null));
		}

		[Test]
		public void RunOnMainThread_WhenFuncIsNull_ThrowsArgumentNullException()
		{
			Assert.ThrowsAsync<ArgumentNullException>(async () => await PlayerLoopHelpers.Run_On_Main_Thread<int>(null)
			);
		}
	}
}
