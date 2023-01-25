using System;
using UnityEngine;
using Bezoro.Core.Helpers;
using System.Collections.Generic;
using KinematicCharacterController;

namespace Bezoro.StateMachines
{
	public abstract class BaseKCCState : IKCCState
	{
		public void Init
		(MonoBehaviour masterStateMachine,
		 IKCCStateMachine owningStateMachine,
		 KinematicCharacterMotor motor)
		{
			_masterStateMachine = masterStateMachine;
			_owningStateMachine = owningStateMachine;
			_motor = motor;
			OnInit();
		}

		void ITickableState.Init
		(MonoBehaviour masterStateMachine,
		 ITickableStateMachine owningStateMachine) =>
			Init(masterStateMachine,
				 owningStateMachine as IKCCStateMachine,
				 null);

		void IState.Init
		(MonoBehaviour masterStateMachine,
		 IStateMachine owningStateMachine) =>
			Init(masterStateMachine,
				 owningStateMachine as IKCCStateMachine,
				 null);

		public MonoBehaviour MasterStateMachine => _masterStateMachine;

		public IKCCTransition[] Transitions => _transitions;

		public KinematicCharacterMotor Motor => _motor;

		public List<Type> MonoBehaviourTypesToIgnoreCollisionsWith { get; } = new(4);

		ITickTransition[] ITickableState.Transitions => _transitions;

		ITransition[] IState.Transitions => _transitions;

		public IKCCTransition TransitionsTo(IKCCState toState)
		{
			var newTransition = this.CreateTransition<KCCTransition>(toState);
			ArrayHelpers.Add(ref _transitions, newTransition);
			return newTransition;
		}


		public void Enter()
		{
			OnEnter();
			this.AllTransitionsBecomeRelevant();
		}

		public void EarlyTick() =>
			OnEarlyTick();

		public void Tick() =>
			OnTick();

		public void FixedTick() =>
			OnFixedTick();

		public void LateTick() =>
			OnLateTick();

		#region KCC Methods

		public void UpdateRotation(ref Quaternion currentRotation, float deltaTime) =>
			OnUpdateRotation(ref currentRotation, deltaTime);

		public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime) =>
			OnUpdateVelocity(ref currentVelocity, deltaTime);

		public void BeforeCharacterUpdate(float deltaTime) =>
			OnBeforeCharacterUpdate(deltaTime);

		public void PostGroundingUpdate(float deltaTime) =>
			OnPostGroundingUpdate(deltaTime);

		public void AfterCharacterUpdate(float deltaTime)
		{
			OnAfterCharacterUpdate(deltaTime);
			this.AllTransitionsTick();
		}

		public void GroundHit
		(Collider hitCollider,
		 Vector3 hitNormal,
		 Vector3 hitPoint,
		 ref HitStabilityReport hitStabilityReport) =>
			OnGroundHit(hitCollider,
					    hitNormal,
					    hitPoint,
					    ref hitStabilityReport);

		public void MovementHit
		(Collider hitCollider,
		 Vector3 hitNormal,
		 Vector3 hitPoint,
		 ref HitStabilityReport hitStabilityReport) =>
			OnMovementHit(hitCollider,
						  hitNormal,
						  hitPoint,
						  ref hitStabilityReport);

		public void ProcessHitStabilityReport
		(Collider hitCollider,
		 Vector3 hitNormal,
		 Vector3 hitPoint,
		 Vector3 atCharacterPosition,
		 Quaternion atCharacterRotation,
		 ref HitStabilityReport hitStabilityReport) =>
			OnProcessHitStabilityReport(hitCollider,
									    hitNormal,
									    hitPoint,
									    atCharacterPosition,
									    atCharacterRotation,
									    ref hitStabilityReport);

		public void DiscreteCollisionDetected(Collider hitCollider) =>
			OnDiscreteCollisionDetected(hitCollider);

		#endregion

		public void Disable() =>
			OnDisable();

		public void OnTransitionTrue(ITickableState toState) =>
			_owningStateMachine.DoTransition(toState);

		public void Exit()
		{
			this.AllTransitionsCeaseRelevant();
			OnExit();
		}

		#region Abstract Methods

		protected abstract void OnInit();

		protected abstract void OnEnter();

		protected abstract void OnEarlyTick();

		protected abstract void OnTick();

		protected abstract void OnFixedTick();

		protected abstract void OnLateTick();

		protected abstract void OnUpdateRotation(ref Quaternion currentRotation, float deltaTime);

		protected abstract void OnUpdateVelocity(ref Vector3 currentVelocity, float deltaTime);

		protected abstract void OnBeforeCharacterUpdate(float deltaTime);

		protected abstract void OnPostGroundingUpdate(float deltaTime);

		protected abstract void OnAfterCharacterUpdate(float deltaTime);

		protected abstract void OnGroundHit
		(Collider hitCollider,
		 Vector3 hitNormal,
		 Vector3 hitPoint,
		 ref HitStabilityReport hitStabilityReport);

		protected abstract void OnMovementHit
		(Collider hitCollider,
		 Vector3 hitNormal,
		 Vector3 hitPoint,
		 ref HitStabilityReport hitStabilityReport);

		protected abstract void OnProcessHitStabilityReport
		(Collider hitCollider,
		 Vector3 hitNormal,
		 Vector3 hitPoint,
		 Vector3 atCharacterPosition,
		 Quaternion atCharacterRotation,
		 ref HitStabilityReport hitStabilityReport);

		protected abstract void OnDiscreteCollisionDetected(Collider hitCollider);

		protected abstract void OnDisable();

		protected abstract void OnExit();

		#endregion

		private IKCCStateMachine _owningStateMachine;
		private MonoBehaviour _masterStateMachine;
		private IKCCTransition[] _transitions = new IKCCTransition[4];
		private KinematicCharacterMotor _motor;
	}

	public sealed class NullKCCState : IKCCState
	{
		public void Init
		(MonoBehaviour masterStateMachine,
		 IKCCStateMachine owningStateMachine,
		 KinematicCharacterMotor motor) { }

		void ITickableState.Init
		(MonoBehaviour masterStateMachine,
		 ITickableStateMachine owningStateMachine) { }

		void IState.Init
		(MonoBehaviour masterStateMachine,
		 IStateMachine owningStateMachine) { }

		public MonoBehaviour MasterStateMachine { get; }

		public IKCCTransition[] Transitions { get; }

		public List<Type> MonoBehaviourTypesToIgnoreCollisionsWith { get; }

		ITickTransition[] ITickableState.Transitions { get; }

		ITransition[] IState.Transitions { get; }

		public void Enter() { }

		public void EarlyTick() { }

		public void Tick() { }

		public void FixedTick() { }

		public void LateTick() { }

		public void OnTransitionTrue(ITickableState toState) { }

		#region KCC Methods

		public void UpdateRotation(ref Quaternion currentRotation, float deltaTime) { }

		public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime) { }

		public void BeforeCharacterUpdate(float deltaTime) { }

		public void PostGroundingUpdate(float deltaTime) { }

		public void AfterCharacterUpdate(float deltaTime) { }

		public void GroundHit
		(Collider hitCollider,
		 Vector3 hitNormal,
		 Vector3 hitPoint,
		 ref HitStabilityReport hitStabilityReport) { }

		public void MovementHit
		(Collider hitCollider,
		 Vector3 hitNormal,
		 Vector3 hitPoint,
		 ref HitStabilityReport hitStabilityReport) { }

		public void ProcessHitStabilityReport
		(Collider hitCollider,
		 Vector3 hitNormal,
		 Vector3 hitPoint,
		 Vector3 atCharacterPosition,
		 Quaternion atCharacterRotation,
		 ref HitStabilityReport hitStabilityReport) { }

		public void DiscreteCollisionDetected(Collider hitCollider) { }

		#endregion

		public void Disable() { }

		public void Exit() { }
	}
}