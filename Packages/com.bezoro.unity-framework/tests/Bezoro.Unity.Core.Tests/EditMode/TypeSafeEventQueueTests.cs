using System;
using Bezoro.Unity.Core.EventSystem;
using NUnit.Framework;

namespace Bezoro.Unity.Core.Tests.EditMode
{
	/// <summary>
	///     Verifies type-safe event queue enqueue, dequeue, and clear behavior.
	/// </summary>
	[TestFixture]
	public sealed class TypeSafeEventQueueTests
	{
		[Test]
		public void Clear_RemovesQueuedEvents()
		{
			var queue = new TypeSafeEventQueue();
			queue.Enqueue(new TestEvent());

			queue.Clear();

			Assert.IsFalse(queue.TryDequeueAny(out _, out _));
		}

		[Test]
		public void Enqueue_WhenEventIsNull_ThrowsArgumentNullException()
		{
			var queue = new TypeSafeEventQueue();

			Assert.Throws<ArgumentNullException>(() => queue.Enqueue<TestEvent>(null));
		}

		[Test]
		public void TryDequeueAny_WhenEventWasEnqueued_ReturnsEventAndType()
		{
			var queue = new TypeSafeEventQueue();
			var evt   = new TestEvent();

			queue.Enqueue(evt);

			bool dequeued = queue.TryDequeueAny(out var eventType, out var eventData);

			Assert.IsTrue(dequeued);
			Assert.AreEqual(typeof(TestEvent), eventType);
			Assert.AreSame(evt, eventData);
		}

		private sealed class TestEvent : BaseEvent { }
	}
}
