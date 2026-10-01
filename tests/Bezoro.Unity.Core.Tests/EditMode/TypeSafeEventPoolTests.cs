using Bezoro.Unity.Core.EventSystem;
using NUnit.Framework;

namespace Bezoro.Unity.Core.Tests.EditMode
{
	/// <summary>
	///     Verifies type-safe event pool reuse and interface return behavior.
	/// </summary>
	[TestFixture]
	public sealed class TypeSafeEventPoolTests
	{
		[Test]
		public void Get_WhenPoolIsEmpty_UsesFactory()
		{
			var createdCount = 0;
			var pool = new TypeSafeEventPool<TestEvent>(
				2, () =>
				{
					createdCount++;
					return new();
				}
			);

			var evt = pool.Get();

			Assert.IsNotNull(evt);
			Assert.AreEqual(1, createdCount);
		}

		[Test]
		public void InterfaceReturn_WhenEventTypeDoesNotMatch_IgnoresItem()
		{
			var createdCount = 0;
			ITypeSafeEventPool pool = new TypeSafeEventPool<TestEvent>(
				2, () =>
				{
					createdCount++;
					return new();
				}
			);

			pool.Return(new OtherEvent());
			var evt = ((TypeSafeEventPool<TestEvent>)pool).Get();

			Assert.IsNotNull(evt);
			Assert.AreEqual(1, createdCount);
		}

		[Test]
		public void Return_ThenGet_ReusesReturnedInstance()
		{
			var pool = new TypeSafeEventPool<TestEvent>(2, () => new());
			var evt  = pool.Get();

			pool.Return(evt);
			var reused = pool.Get();

			Assert.AreSame(evt, reused);
		}

		private sealed class OtherEvent : BaseEvent { }

		private sealed class TestEvent : BaseEvent { }
	}
}
