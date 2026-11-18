using System.Collections;
using System.Text.RegularExpressions;
using System.Threading;
using Bezoro.Unity.Core.AsyncLoading;
using Bezoro.Unity.Core.Loading;
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
			using var progress = new TestGlobalProvider<LoadingProgressService>("Async loading progress");
			progress.Instance.BeginOperation("Withdrawn load test");
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
			using var progress = new TestGlobalProvider<LoadingProgressService>("Async loading progress");
			progress.Instance.BeginOperation("Registered load test");
			using var owner = new TestGlobalProvider<AsyncLoadingManager>("Async loading");
			var manager = owner.Instance;
			var withdrawn = new EqualPendingLoad("Earlier equal load", priority: 0);
			var first = new EqualPendingLoad("Registered cancellation", priority: 1);
			var next = new PendingLoad("Next load");
			manager.Register(withdrawn);
			manager.Register(first);

			yield return WaitForAttempts(first, 1);

			manager.Remove(first);
			manager.Register(next);
			LogAssert.Expect(LogType.Error, new Regex("Failed to load item: Registered cancellation"));
			first.Completion.TrySetCanceled();
			next.Completion.TrySetResult();

			yield return WaitForAttempts(next, 1);
			Assert.AreEqual(0, withdrawn.Attempts);
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

		private class PendingLoad : IAsyncLoadable
		{
			public PendingLoad(string name, int priority = 0)
			{
				LoadingName = name;
				Priority = priority;
			}

			public string LoadingName { get; }
			public int Priority { get; }
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

		private sealed class EqualPendingLoad : PendingLoad
		{
			public EqualPendingLoad(string name, int priority) : base(name, priority) { }

			public override bool Equals(object? other) => other is EqualPendingLoad;
			public override int GetHashCode() => typeof(EqualPendingLoad).GetHashCode();
		}
	}
}
