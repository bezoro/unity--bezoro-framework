using System.Collections;
using System.Text.RegularExpressions;
using System.Threading;
using Bezoro.Unity.Core.AsyncLoading;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Bezoro.Unity.Core.Tests.PlayMode
{
	public sealed class AsyncLoadingManagerPlayModeTests
	{
		[UnityTest]
		public IEnumerator RemovedLoad_CancellationPreservesNewRegistration()
		{
			using var owner = new TestGlobalProvider<AsyncLoadingManager>("Async loading");
			var manager = owner.Instance;
			var first = new PendingLoad("Withdrawn load");
			manager.Register(first);

			yield return WaitForAttempts(first, 1);

			manager.Remove(first);
			manager.Register(first);
			first.Completion.TrySetCanceled();

			yield return WaitForAttempts(first, 2);
			Assert.IsTrue(first.IsLoaded);
		}

		[UnityTest]
		public IEnumerator RegisteredLoad_CancellationReportsFailureAndQueueContinues()
		{
			using var owner = new TestGlobalProvider<AsyncLoadingManager>("Async loading");
			var manager = owner.Instance;
			var first = new PendingLoad("Registered cancellation");
			var next = new PendingLoad("Next load");
			manager.Register(first);

			yield return WaitForAttempts(first, 1);

			manager.Register(next);
			LogAssert.Expect(LogType.Error, new Regex("Failed to load item: Registered cancellation"));
			first.Completion.TrySetCanceled();
			next.Completion.TrySetResult();

			yield return WaitForAttempts(next, 1);
			Assert.IsFalse(first.IsLoaded);
			Assert.IsTrue(next.IsLoaded);
		}

		private static IEnumerator WaitForAttempts(PendingLoad item, int expected)
		{
			float deadline = Time.realtimeSinceStartup + 5f;
			while (item.Attempts < expected && Time.realtimeSinceStartup < deadline)
				yield return null;
			Assert.AreEqual(expected, item.Attempts);
		}

		private sealed class PendingLoad : IAsyncLoadable
		{
			public PendingLoad(string name) => LoadingName = name;

			public string LoadingName { get; }
			public int Priority => 0;
			public int Attempts { get; private set; }
			public bool IsLoaded { get; private set; }
			public UniTaskCompletionSource Completion { get; } = new();

			public async UniTask LoadAsync(CancellationToken ct)
			{
				Attempts++;
				if (Attempts == 1)
					await Completion.Task.AttachExternalCancellation(ct);
				IsLoaded = true;
			}
		}
	}
}
