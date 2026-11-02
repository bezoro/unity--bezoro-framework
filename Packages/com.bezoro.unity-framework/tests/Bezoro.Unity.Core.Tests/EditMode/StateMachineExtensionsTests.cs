using Bezoro.Unity.Core.StateMachines;
using Bezoro.Unity.Core.StateMachines.Utilities;
using NUnit.Framework;

namespace Bezoro.Unity.Core.Tests.EditMode
{
	[TestFixture]
	public sealed class StateMachineExtensionsTests
	{
		[Test]
		public void TryFindStateInCurrentHierarchy_WhenNestedCurrentStateMatches_ReturnsState()
		{
			var target       = new NestedTestState();
			var childMachine = new TestStateMachine(target,       new());
			var rootMachine  = new TestStateMachine(childMachine, new());

			bool found = rootMachine.TryFindStateInCurrentHierarchy<TestState, NestedTestState>(out var state);

			Assert.IsTrue(found);
			Assert.AreSame(target, state);
		}

		[Test]
		public void TryFindStateInCurrentHierarchy_WhenNoCurrentStateMatches_ReturnsFalse()
		{
			var rootMachine = new TestStateMachine(new(), new());

			bool found = rootMachine.TryFindStateInCurrentHierarchy<TestState, NestedTestState>(out var state);

			Assert.IsFalse(found);
			Assert.IsNull(state);
		}

		[Test]
		public void TryFindStateInLastHierarchy_WhenNestedLastStateMatches_ReturnsState()
		{
			var target       = new NestedTestState();
			var childMachine = new TestStateMachine(new(), target);
			var rootMachine  = new TestStateMachine(new(), childMachine);

			bool found = rootMachine.TryFindStateInLastHierarchy<TestState, NestedTestState>(out var state);

			Assert.IsTrue(found);
			Assert.AreSame(target, state);
		}

		private sealed class NestedTestState : TestState { }

		private class TestState : IState
		{
			public void Destroy() { }
			public void Disable() { }
			public void Enable()  { }
			public void Enter()   { }
			public void Exit()    { }
		}

		private sealed class TestStateMachine : TestState, IStateMachine<TestState>
		{
			public TestStateMachine(TestState currentState, TestState lastState)
			{
				CurrentState = currentState;
				LastState    = lastState;
			}

			public TestState CurrentState { get; private set; }
			public TestState LastState    { get; private set; }

			public void DoTransition(TestState toState)
			{
				LastState    = CurrentState;
				CurrentState = toState;
			}
		}
	}
}
