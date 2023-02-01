using System;
using UnityEngine;
using System.Collections.Generic;
using KinematicCharacterController;

namespace Bezoro.StateMachines
{
	public interface IKCCState
	{
		void Init
		(MonoBehaviour masterStateMachine,
		 IKCCStateMachine owningStateMachine,
		 KinematicCharacterMotor motor);

		List<Type> MonoBehaviourTypesToIgnoreCollisionsWith { get; }

		IKCCTransition[] Transitions { get; }

		#region KCC Methods

		public void UpdateRotation(ref Quaternion currentRotation, float deltaTime);

		public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime);

		public void BeforeCharacterUpdate(float deltaTime);

		public void PostGroundingUpdate(float deltaTime);

		public void AfterCharacterUpdate(float deltaTime);

		public void GroundHit
		(Collider hitCollider,
		 Vector3 hitNormal,
		 Vector3 hitPoint,
		 ref HitStabilityReport hitStabilityReport);

		public void MovementHit
		(Collider hitCollider,
		 Vector3 hitNormal,
		 Vector3 hitPoint,
		 ref HitStabilityReport hitStabilityReport);

		public void ProcessHitStabilityReport
		(Collider hitCollider,
		 Vector3 hitNormal,
		 Vector3 hitPoint,
		 Vector3 atCharacterPosition,
		 Quaternion atCharacterRotation,
		 ref HitStabilityReport hitStabilityReport);

		public void DiscreteCollisionDetected(Collider hitCollider);

		#endregion
	}

	public interface ITickableState
	{
		void Init(MonoBehaviour masterStateMachine, ITickableStateMachine owningStateMachine);

		void Enter();

		void EarlyTick();

		void Tick();

		void FixedTick();

		void LateTick();

		void OnTransitionTrue(ITickableState toState);

		void Exit();
	}

	public interface IEventState
	{
		void Init(MonoBehaviour masterStateMachine, IEventStateMachine owningStateMachine);

		void Enter();

		void OnTransitionRaised(IEventState toState);

		void Exit();
	}
}