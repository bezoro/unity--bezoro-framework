using System;
using UnityEngine;
using System.Collections.Generic;
using KinematicCharacterController;

namespace Bezoro.StateMachines
{
	public interface IKCCState : IState, ITickable, IHasTransitions<IKCCState>
	{
		void Init
		(MonoBehaviour masterStateMachine,
		 IKCCStateMachine owningStateMachine,
		 KinematicCharacterMotor motor);

		List<Type> MonoBehaviourTypesToIgnoreCollisionsWith { get; }

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

		bool IsSubStateMachine(out IKCCStateMachine outStateMachine);
	}

	public interface ITickableState : IState, ITickable, IHasTransitions<ITickableState>
	{
		void Init(MonoBehaviour masterStateMachine, ITickableStateMachine owningStateMachine);
	}

	public interface ITickable
	{
		void EarlyTick();

		void Tick();

		void FixedTick();

		void LateTick();
	}

	public interface IEventState : IState, IHasTransitions<IEventState>
	{
		void Init(MonoBehaviour masterStateMachine, IEventStateMachine owningStateMachine);
	}

	public interface IHasTransitions<T>
	where T : IState
	{
		void OnTransitionRaised(T toState);
	}

	public interface IState
	{
		void Enter();

		void Exit();
	}
}