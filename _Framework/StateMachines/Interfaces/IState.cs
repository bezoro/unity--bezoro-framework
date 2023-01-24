using System;
using UnityEngine;
using System.Collections.Generic;
using KinematicCharacterController;

namespace Bezoro.StateMachines
{
	public interface IKCCState : ITickableState
	{
		public void Init
		(MonoBehaviour masterStateMachine,
		 IKCCStateMachine owningStateMachine,
		 KinematicCharacterMotor motor);

		public List<Type> MonoBehaviourTypesToIgnoreCollisionsWith { get; }

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

	public interface ITickableState : IState
	{
		public void Init
		(MonoBehaviour masterStateMachine,
		 ITickableStateMachine owningStateMachine);

		public void EarlyTick();

		public void Tick();

		public void FixedTick();

		public void LateTick();

		public void OnTransitionTrue(ITickableState toState);
	}

	public interface IEventState : IState
	{
		public void Init
		(MonoBehaviour masterStateMachine,
		 IEventStateMachine owningStateMachine);

		public void OnTransitionRaised(IEventState toState);
	}

	public interface IState
	{
		public void Init
		(MonoBehaviour masterStateMachine,
		 IStateMachine owningStateMachine);

		public void Enter();

		public void Exit();

		public void Disable();
	}
}