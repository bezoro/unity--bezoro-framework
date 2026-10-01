using System;
using System.Collections.Generic;
using Bezoro.Unity.Core.StateMachines;
using Bezoro.Unity.Core.StateMachines.KCCBased;
using Bezoro.Unity.Core.StateMachines.Utilities;
using Bezoro.Unity.Core.ThirdParty.KinematicCharacterController.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace Bezoro.Unity.Core.Tests.EditMode
{
	[TestFixture]
	public sealed class StateMachineExtensionsTests
	{
		[Test]
		public void KccStateExtensions_AddMonoBehaviourToIgnoreCollisionsWith_AddsType()
		{
			var state = new TestKccState();

			state.AddMonoBehaviourToIgnoreCollisionsWith<TestBehaviour>();

			CollectionAssert.Contains(state.MonoBehaviourTypesToIgnoreCollisionsWith, typeof(TestBehaviour));
		}

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

		private sealed class TestBehaviour : MonoBehaviour { }

		private sealed class TestKccState : IKCCState
		{
			public void Init(
				KCCStateMachineOwner    owner,
				IKCCStateMachine        owningStateMachine,
				KinematicCharacterMotor motor) { }

			public KCCTransition[] Transitions                              { get; } = Array.Empty<KCCTransition>();
			public List<Type>      MonoBehaviourTypesToIgnoreCollisionsWith { get; } = new();

			public bool IsSubStateMachine(out IKCCStateMachine outStateMachine)
			{
				outStateMachine = null;
				return false;
			}

			public void AfterCharacterUpdate(float  deltaTime) { }
			public void BeforeCharacterUpdate(float deltaTime) { }

			public void Destroy()                                       { }
			public void Disable()                                       { }
			public void DiscreteCollisionDetected(Collider hitCollider) { }
			public void EarlyTick()                                     { }
			public void Enable()                                        { }
			public void Enter()                                         { }
			public void Exit()                                          { }
			public void FixedTick()                                     { }

			public void GroundHit(
				Collider               hitCollider,
				Vector3                hitNormal,
				Vector3                hitPoint,
				ref HitStabilityReport hitStabilityReport) { }

			public void LateTick() { }

			public void MovementHit(
				Collider               hitCollider,
				Vector3                hitNormal,
				Vector3                hitPoint,
				ref HitStabilityReport hitStabilityReport) { }

			public void OnTransitionRaised(IKCCState toState)   { }
			public void PostGroundingUpdate(float    deltaTime) { }

			public void ProcessHitStabilityReport(
				Collider               hitCollider,
				Vector3                hitNormal,
				Vector3                hitPoint,
				Vector3                atCharacterPosition,
				Quaternion             atCharacterRotation,
				ref HitStabilityReport hitStabilityReport) { }

			public void Tick()                                                          { }
			public void UpdateRotation(ref Quaternion currentRotation, float deltaTime) { }
			public void UpdateVelocity(ref Vector3    currentVelocity, float deltaTime) { }
		}

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
