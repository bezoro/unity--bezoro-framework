using System.Collections.Generic;
using Bezoro.Unity.Core.Observables;
using NUnit.Framework;

namespace Bezoro.Unity.Core.Tests.EditMode
{
	/// <summary>
	///     Verifies observable property change notification semantics.
	/// </summary>
	[TestFixture]
	public sealed class ObservablePropertyTests
	{
		[Test]
		public void Unsubscribe_RemovesHandler()
		{
			var property       = new TestReferenceProperty();
			var receivedValues = new List<string>();
			void Handler(string value) => receivedValues.Add(value);

			property.Subscribe(Handler);
			property.Unsubscribe(Handler);
			property.Value = "changed";

			CollectionAssert.IsEmpty(receivedValues);
		}

		[Test]
		public void Value_WhenChanged_NotifiesSubscribers()
		{
			var property       = new TestValueProperty();
			var receivedValues = new List<int>();
			property.Subscribe(receivedValues.Add);

			property.Value = 3;
			property.Value = 5;

			CollectionAssert.AreEqual(new[] { 3, 5 }, receivedValues);
		}

		[Test]
		public void Value_WhenSetToSameValue_DoesNotNotifySubscribers()
		{
			var property       = new TestValueProperty();
			var receivedValues = new List<int>();
			property.Value = 3;
			property.Subscribe(receivedValues.Add);

			property.Value = 3;

			CollectionAssert.IsEmpty(receivedValues);
		}

		[Test]
		public void Subscriber_CanReadValueFromAnotherThreadBeforeReturning()
		{
			var property = new TestValueProperty();
			bool completed = false;
			property.Subscribe(_ =>
			{
				var read = System.Threading.Tasks.Task.Run(() => property.Value);
				completed = read.Wait(System.TimeSpan.FromSeconds(3)) && read.Result == 42;
			});
			property.Value = 42;
			Assert.IsTrue(completed);
		}

		private sealed class TestReferenceProperty : ObservableReferenceProperty<string> { }

		private sealed class TestValueProperty : ObservableValueProperty<int> { }
	}
}
