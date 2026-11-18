using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bezoro.Unity.Core.EventSystem;
using Cysharp.Threading.Tasks;
using NUnit.Framework;

namespace Bezoro.Unity.Core.Tests.EditMode
{
	[TestFixture]
	/// <summary>
	/// Verifies event bus pooling does not return the same published event twice.
	/// </summary>
	public sealed class EventBusTests
	{
		[Test]
		public void Publish_WhenEventDataIsNull_ThrowsArgumentNullException()
		{
			var bus = new EventBus();

			Assert.ThrowsAsync<ArgumentNullException>(async () => await bus.Publish<TestEvent>(
																	  null!
																  )
			);
		}

		[Test]
		public async Task Publish_WhenNestedEventIsQueued_DoesNotReuseOuterEventBeforeDispatch()
		{
			var bus               = new EventBus();
			var receivedPayloads  = new List<int>();
			var queuedNestedEvent = false;

			bus.SubscribeUnique<TestEvent>(evt =>
				{
					receivedPayloads.Add(
						evt.Payload
					);

					if (queuedNestedEvent) return;

					queuedNestedEvent = true;
					bus.Publish(
						bus.CreateEvent<TestEvent, int>(
							2
						)
					).Forget();
				}
			);

			await bus.PublishAsync(
				bus.CreateEvent<TestEvent, int>(
					1
				)
			);

			CollectionAssert.AreEqual(
				new[] { 1, 2 },
				receivedPayloads
			);
		}

		[Test]
		public async Task PublishAsync_AfterCancelEventProcessing_UsesFreshCancellationToken()
		{
			var bus              = new EventBus();
			var receivedPayloads = new List<int>();

			bus.SubscribeUnique<TestEvent>(evt => receivedPayloads.Add(
											   evt.Payload
										   )
			);

			bus.CancelEventProcessing();

			await bus.PublishAsync(
				bus.CreateEvent<TestEvent, int>(
					7
				)
			);

			CollectionAssert.AreEqual(
				new[] { 7 },
				receivedPayloads
			);
		}

		[Test]
		public async Task PublishAsync_WhenEventIsPooled_ReturnsEventOnlyOnce()
		{
			var bus = new EventBus();
			bus.SubscribeUnique<TestEvent>(_ => { });

			var publishedEvent = bus.CreateEvent<TestEvent, int>(1);

			await bus.PublishAsync(publishedEvent);

			var firstPooledEvent  = bus.CreateEvent<TestEvent, int>(2);
			var secondPooledEvent = bus.CreateEvent<TestEvent, int>(3);

			Assert.AreSame(publishedEvent, firstPooledEvent);
			Assert.AreNotSame(firstPooledEvent, secondPooledEvent);
			Assert.AreEqual(2, firstPooledEvent.Payload);
			Assert.AreEqual(3, secondPooledEvent.Payload);
		}

		[Test]
		public async Task PublishAsync_WhenEventIsQueued_InvokesSubscriber()
		{
			var bus              = new EventBus();
			var receivedPayloads = new List<int>();

			bus.SubscribeUnique<TestEvent>(evt => receivedPayloads.Add(evt.Payload));

			await bus.PublishAsync(
				bus.CreateEvent<TestEvent, int>(
					1
				)
			);

			CollectionAssert.AreEqual(
				new[] { 1 },
				receivedPayloads
			);
		}

		[Test]
		public async Task NestedPublications_CompleteAfterTheirHandlersInStablePriorityOrder()
		{
			using var bus = new EventBus();
			var received = new List<int>();
			var nested = new List<UniTask>();
			bool wasPending = false;
			bus.SubscribeUnique<TestEvent>(evt =>
			{
				received.Add(evt.Payload);
				if (evt.Payload != 0) return;
				foreach (int value in new[] { 1, 2, 3 })
				{
					var next = new TestEvent();
					next.Initialize(value, value == 1 ? int.MinValue : int.MaxValue);
					nested.Add(bus.Publish(next));
				}
				wasPending = nested.TrueForAll(task => task.Status == UniTaskStatus.Pending);
			});
			await bus.Publish(bus.CreateEvent<TestEvent, int>(0));
			foreach (var publication in nested) await publication;
			Assert.IsTrue(wasPending);
			CollectionAssert.AreEqual(new[] { 0, 2, 3, 1 }, received);
		}

		[Test]
		public async Task ConcurrentPublisher_WaitsForItsOwnDispatch()
		{
			using var bus = new EventBus();
			var received = new List<int>();
			UniTask second = default;
			bool pendingWhileFirstHandlerRan = false;
			bus.SubscribeUnique<TestEvent>(evt =>
			{
				received.Add(evt.Payload);
				if (evt.Payload != 1) return;
				// Join only the enqueue operation. Waiting for the nested dispatch here would block the drainer.
				var enqueue = Task.Run(() => { second = bus.Publish(bus.CreateEvent<TestEvent, int>(2)); });
				pendingWhileFirstHandlerRan = enqueue.Wait(TimeSpan.FromSeconds(3)) && second.Status == UniTaskStatus.Pending;
			});
			await bus.Publish(bus.CreateEvent<TestEvent, int>(1));
			await second;
			Assert.IsTrue(pendingWhileFirstHandlerRan);
			CollectionAssert.AreEqual(new[] { 1, 2 }, received);
		}

		[Test]
		public void Cancel_FromHandler_RetainsActiveLeaseUntilHandlerReturns()
		{
			using var bus = new EventBus();
			var active = bus.CreateEvent<TestEvent, int>(1);
			TestEvent? rentedDuringDispatch = null;
			int observedAfterCancellation = 0;
			bus.SubscribeUnique<TestEvent>(evt =>
			{
				bus.CancelEventProcessing();
				rentedDuringDispatch = bus.CreateEvent<TestEvent, int>(2);
				observedAfterCancellation = evt.Payload;
			});
			var result = bus.Publish(active);
			Assert.Catch<OperationCanceledException>(() => result.GetAwaiter().GetResult());
			Assert.AreNotSame(active, rentedDuringDispatch);
			Assert.AreEqual(1, observedAfterCancellation);
			Assert.AreSame(active, bus.CreateEvent<TestEvent, int>(3));
		}

		[Test]
		public void Dispose_FromHandler_CancelsActivePublication()
		{
			using var bus = new EventBus();
			bus.SubscribeUnique<TestEvent>(_ => bus.Dispose());
			var result = bus.Publish(new TestEvent());
			Assert.AreEqual(UniTaskStatus.Canceled, result.Status);
			Assert.Catch<OperationCanceledException>(() => result.GetAwaiter().GetResult());
		}

		[Test]
		public async Task CreateEvent_ExternalPublicationAndPriorLeaseCannotChangeConstructorDefaults()
		{
			using var bus = new EventBus();
			var leased = bus.CreateEvent<TestEvent, int>(9);
			leased.Initialize(8, 7, true);
			await bus.Publish(leased);
			var external = new TestEvent();
			await bus.Publish(external);
			await bus.Publish(external);
			var fresh = bus.CreateEvent<TestEvent>();
			Assert.AreEqual(0, fresh.Payload);
			Assert.AreEqual(1, fresh.Priority);
			Assert.IsFalse(fresh.RunOnThreadPool);
			var first = bus.CreateEvent<TestEvent, int>(1);
			var second = bus.CreateEvent<TestEvent, int>(2);
			Assert.AreSame(leased, first);
			Assert.AreNotSame(first, second);
			Assert.AreNotSame(external, second);
		}

		/// <summary>
		///     Event payload type used to verify event pooling.
		/// </summary>
		private sealed class TestEvent : BaseEvent<int> { }
	}
}
