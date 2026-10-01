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

		/// <summary>
		///     Event payload type used to verify event pooling.
		/// </summary>
		private sealed class TestEvent : BaseEvent<int> { }
	}
}
