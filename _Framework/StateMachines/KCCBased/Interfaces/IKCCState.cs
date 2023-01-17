using System;
using UnityEngine;
using System.Collections.Generic;
using KinematicCharacterController;

namespace Bezoro.StateMachines
{
	public interface IKCCState
	{
		public KinematicCharacterMotor Motor { get; }

		public List<Type> MonoBehaviourTypesToIgnoreCollisionsWith { get; }

		public void Init
		(
			BaseKCCStateMachine stateMachine,
			IKCCSuperState parentState,
			KinematicCharacterMotor motor
		);

		public KCCTransition TransitionsTo(IKCCState state);

		public void Enter();

		public void EarlyTick();

		public void Tick();

		public void FixedTick();

		public void LateTick();

		#region KCC Methods

		public void UpdateRotation(ref Quaternion currentRotation, float deltaTime);

		public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime);

		public void BeforeCharacterUpdate(float deltaTime);

		public void PostGroundingUpdate(float deltaTime);

		public void AfterCharacterUpdate(float deltaTime);

		public void GroundHit
		(
			Collider hitCollider,
			Vector3 hitNormal,
			Vector3 hitPoint,
			ref HitStabilityReport hitStabilityReport
		);

		public void MovementHit
		(
			Collider hitCollider,
			Vector3 hitNormal,
			Vector3 hitPoint,
			ref HitStabilityReport hitStabilityReport
		);

		public void ProcessHitStabilityReport
		(
			Collider hitCollider,
			Vector3 hitNormal,
			Vector3 hitPoint,
			Vector3 atCharacterPosition,
			Quaternion atCharacterRotation,
			ref HitStabilityReport hitStabilityReport
		);

		public void DiscreteCollisionDetected(Collider hitCollider);

		#endregion

		public void Exit();

		public void OnTransitionTrue(IKCCState toState);
	}
}