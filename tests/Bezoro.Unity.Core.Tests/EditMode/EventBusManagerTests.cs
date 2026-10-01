using System;
using Bezoro.Unity.Core.Common.Patterns;
using Bezoro.Unity.Core.EventSystem;
using NUnit.Framework;

namespace Bezoro.Unity.Core.Tests.EditMode
{
	/// <summary>
	///     Verifies named event bus registry behavior.
	/// </summary>
	[TestFixture]
	public sealed class EventBusManagerTests
	{
		[TearDown]
		public void TearDown()
		{
			Singleton<EventBusManager>.Reset();
		}

		[Test]
		public void ClearAll_RemovesEveryRegisteredContext()
		{
			EventBusManager.Instance.GetOrCreateEventBus("gameplay");
			EventBusManager.Instance.GetOrCreateEventBus("ui");

			EventBusManager.Instance.ClearAll();

			Assert.IsFalse(EventBusManager.Instance.TryGetEventBus("gameplay", out _));
			Assert.IsFalse(EventBusManager.Instance.TryGetEventBus("ui",       out _));
		}

		[Test]
		public void GetOrCreateEventBus_WhenContextExists_ReturnsSameInstance()
		{
			var firstBus  = EventBusManager.Instance.GetOrCreateEventBus("gameplay");
			var secondBus = EventBusManager.Instance.GetOrCreateEventBus("gameplay");

			Assert.AreSame(firstBus, secondBus);
		}

		[Test]
		public void GetOrCreateEventBus_WhenContextIsEmpty_ThrowsArgumentException()
		{
			Assert.Throws<ArgumentException>(() => EventBusManager.Instance.GetOrCreateEventBus(string.Empty));
		}

		[Test]
		public void GetOrCreateEventBus_WhenContextsDiffer_ReturnsIsolatedInstances()
		{
			var gameplayBus = EventBusManager.Instance.GetOrCreateEventBus("gameplay");
			var uiBus       = EventBusManager.Instance.GetOrCreateEventBus("ui");

			Assert.AreNotSame(gameplayBus, uiBus);
		}

		[Test]
		public void RemoveEventBus_WhenContextExists_RemovesOnlyThatContext()
		{
			var gameplayBus = EventBusManager.Instance.GetOrCreateEventBus("gameplay");
			var uiBus       = EventBusManager.Instance.GetOrCreateEventBus("ui");

			bool removed = EventBusManager.Instance.RemoveEventBus("gameplay");

			Assert.IsTrue(removed);
			Assert.IsFalse(EventBusManager.Instance.TryGetEventBus("gameplay", out _));
			Assert.IsTrue(EventBusManager.Instance.TryGetEventBus("ui",        out var remainingBus));
			Assert.AreSame(uiBus, remainingBus);
			Assert.AreNotSame(gameplayBus, remainingBus);
		}

		[Test]
		public void TryGetEventBus_WhenContextExists_ReturnsRegisteredBus()
		{
			var registeredBus = EventBusManager.Instance.GetOrCreateEventBus("gameplay");

			bool found = EventBusManager.Instance.TryGetEventBus("gameplay", out var eventBus);

			Assert.IsTrue(found);
			Assert.AreSame(registeredBus, eventBus);
		}

		[Test]
		public void TryGetEventBus_WhenContextIsMissing_ReturnsFalseAndNullBus()
		{
			bool found = EventBusManager.Instance.TryGetEventBus("missing", out var eventBus);

			Assert.IsFalse(found);
			Assert.IsNull(eventBus);
		}
	}
}
