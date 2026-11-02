using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bezoro.Unity.Core.Loading;
using NUnit.Framework;
using UnityEngine;

namespace Bezoro.Unity.Core.Tests.EditMode
{
	/// <summary>
	///     Verifies scene load handle behavior for no-op load/unload requests.
	/// </summary>
	[TestFixture]
	public sealed class SceneLoadHandleTests
	{
		[Test]
		public async Task Constructor_WhenOperationsAreNull_FailsHandle()
		{
			var handle = new SceneLoadHandle(null);

			Assert.IsTrue(handle.IsFailed);
			Assert.IsTrue(handle.IsDoneOrFailed);
			Assert.IsFalse(handle.IsDone);
			Assert.IsFalse(handle.IsDoneOrCancelled);
			Assert.AreEqual(-1f, handle.Progress);

			var exception = await CatchHandleExceptionAsync(handle);

			Assert.AreEqual("No async operations supplied.", exception.Message);
		}

		[Test]
		public async Task Load_WhenPathsAreEmpty_ReturnsFailedHandle()
		{
			var handle = SceneLoader.Load(new[] { string.Empty, null });

			await AssertFailedHandleAsync(handle);
		}

		[Test]
		public async Task Task_WhenOperationsAreEmpty_ThrowsInvalidOperationException()
		{
			var handle = new SceneLoadHandle(Array.Empty<AsyncOperation>());

			var exception = await CatchHandleExceptionAsync(handle);

			Assert.AreEqual("No async operations supplied.", exception.Message);
		}

		[Test]
		public async Task Unload_WhenPathsAreNull_ReturnsFailedHandle()
		{
			var handle = SceneLoader.Unload((IEnumerable<string>)null);

			await AssertFailedHandleAsync(handle);
		}

		private static async Task AssertFailedHandleAsync(SceneLoadHandle handle)
		{
			Assert.IsTrue(handle.IsFailed);
			Assert.AreEqual(-1f, handle.Progress);

			var exception = await CatchHandleExceptionAsync(handle);

			Assert.AreEqual("No async operations supplied.", exception.Message);
		}

		private static async Task<InvalidOperationException> CatchHandleExceptionAsync(SceneLoadHandle handle)
		{
			try
			{
				await handle.Task;
			}
			catch (InvalidOperationException exception)
			{
				return exception;
			}

			Assert.Fail("Expected failed scene load handle task.");
			return null;
		}
	}
}
