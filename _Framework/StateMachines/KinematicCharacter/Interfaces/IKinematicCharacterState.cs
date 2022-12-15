using System;
using System.Collections.Generic;
using KinematicCharacterController;
using UnityEngine;

namespace Bezoro.StateMachines
{
	public interface IKinematicCharacterSuperState : IKinematicCharacterState
	{
		public IKinematicCharacterState LastState { get; }

		public IKinematicCharacterState CurrentState { get; }

		public void SwitchCurrentState(IKinematicCharacterState stateToSwitchTo);
	}

	public interface IKinematicCharacterState
	{
		public KinematicCharacterMotor Motor { get; }

		public List<Type> MonoBehaviourTypesToIgnoreCollisionsWith { get; }

		public void Init
		(
			KinematicCharacterStateMachine stateMachine,
			IKinematicCharacterSuperState parentState,
			KinematicCharacterMotor motor
		);

		public KinematicCharacterTransition TransitionsTo(IKinematicCharacterState state);

		public void Enter();

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

		public void SwitchParentCurrentState(IKinematicCharacterState toState);
	}
}