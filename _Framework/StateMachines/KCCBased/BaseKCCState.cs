using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using KinematicCharacterController;

namespace Bezoro.StateMachines
{
	public abstract class BaseKCCState : IKCCState
	{
		public KinematicCharacterMotor Motor => _motor;

		public List<Type> MonoBehaviourTypesToIgnoreCollisionsWith { get; } = new(12);

		public virtual void Init
		(
			BaseKCCStateMachine masterStateMachine,
			IKCCStateMachine owningStateMachine,
			KinematicCharacterMotor motor
		)
		{
			_masterStateMachine = masterStateMachine;
			_owningStateMachine = owningStateMachine;
			_motor = motor;
			OnInit();
		}

		public KCCTransition TransitionsTo(IKCCState state)
		{
			var transition = new KCCTransition(this, state, _masterStateMachine);
			_transitions.Add(transition);
			return transition;
		}

		public void Enter()
		{
			OnEnter();

			for (var i = 0; i < _transitions.Count; i++)
				_transitions[i].BecomeRelevant();
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
			TickTransitions();
		}

		public void GroundHit
		(
			Collider hitCollider,
			Vector3 hitNormal,
			Vector3 hitPoint,
			ref HitStabilityReport hitStabilityReport
		) =>
			OnGroundHit
				(
				 hitCollider,
				 hitNormal,
				 hitPoint,
				 ref hitStabilityReport
				);

		public void MovementHit
		(
			Collider hitCollider,
			Vector3 hitNormal,
			Vector3 hitPoint,
			ref HitStabilityReport hitStabilityReport
		) =>
			OnMovementHit
				(
				 hitCollider,
				 hitNormal,
				 hitPoint,
				 ref hitStabilityReport
				);

		public void ProcessHitStabilityReport
		(
			Collider hitCollider,
			Vector3 hitNormal,
			Vector3 hitPoint,
			Vector3 atCharacterPosition,
			Quaternion atCharacterRotation,
			ref HitStabilityReport hitStabilityReport
		) =>
			OnProcessHitStabilityReport
				(
				 hitCollider,
				 hitNormal,
				 hitPoint,
				 atCharacterPosition,
				 atCharacterRotation,
				 ref hitStabilityReport
				);

		public void DiscreteCollisionDetected(Collider hitCollider) =>
			OnDiscreteCollisionDetected(hitCollider);

		#endregion

		public void Disable() =>
			OnDisable();

		public void OnTransitionTrue(IKCCState toState) =>
			_owningStateMachine.DoTransition(toState);

		public void Exit()
		{
			for (var i = 0; i < _transitions.Count; i++)
				_transitions[i].CeaseRelevant();

			OnExit();
		}

		protected void AddMonoBehaviourTypeToIgnoreCollisionsWith<TMonoBehaviour>()
		where TMonoBehaviour : MonoBehaviour
		{
			var type = typeof(TMonoBehaviour);
			MonoBehaviourTypesToIgnoreCollisionsWith.Add(type);
		}

		protected Coroutine StartCoroutine(IEnumerator routine) =>
			_masterStateMachine.StartCoroutine(routine);

		protected bool TryGetComponent<T>(out T component)
		where T : Component
		{
			if (_masterStateMachine.TryGetComponent<T>(out var c))
			{
				component = c;
				return true;
			}

			component = null;
			return false;
		}

		#region Protected Abstract Methods

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
		(
			Collider hitCollider,
			Vector3 hitNormal,
			Vector3 hitPoint,
			ref HitStabilityReport hitStabilityReport
		);

		protected abstract void OnMovementHit
		(
			Collider hitCollider,
			Vector3 hitNormal,
			Vector3 hitPoint,
			ref HitStabilityReport hitStabilityReport
		);

		protected abstract void OnProcessHitStabilityReport
		(
			Collider hitCollider,
			Vector3 hitNormal,
			Vector3 hitPoint,
			Vector3 atCharacterPosition,
			Quaternion atCharacterRotation,
			ref HitStabilityReport hitStabilityReport
		);

		protected abstract void OnDiscreteCollisionDetected(Collider hitCollider);

		protected abstract void OnDisable();

		protected abstract void OnExit();

		#endregion

		private void TickTransitions()
		{
			for (var i = 0; i < _transitions.Count; i++)
				_transitions[i].Tick();
		}

		private IKCCStateMachine _owningStateMachine;
		private BaseKCCStateMachine _masterStateMachine;
		private KinematicCharacterMotor _motor;

		private readonly List<KCCTransition> _transitions = new(4);
	}

	public sealed class NullKCCState : IKCCState
	{
		public void Init
		(
			BaseKCCStateMachine masterStateMachine,
			IKCCStateMachine owningStateMachine,
			KinematicCharacterMotor motor
		) { }

		public List<Type> MonoBehaviourTypesToIgnoreCollisionsWith { get; } = new();

		public KinematicCharacterMotor Motor { get; } = new();

		public void Enter() { }

		public void EarlyTick() { }

		public void Tick() { }

		public void FixedTick() { }

		public void LateTick() { }

		#region KCC Methods

		public void UpdateRotation(ref Quaternion currentRotation, float deltaTime) { }

		public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime) { }

		public void BeforeCharacterUpdate(float deltaTime) { }

		public void PostGroundingUpdate(float deltaTime) { }

		public void AfterCharacterUpdate(float deltaTime) { }

		public void GroundHit
		(
			Collider hitCollider,
			Vector3 hitNormal,
			Vector3 hitPoint,
			ref HitStabilityReport hitStabilityReport
		) { }

		public void MovementHit
		(
			Collider hitCollider,
			Vector3 hitNormal,
			Vector3 hitPoint,
			ref HitStabilityReport hitStabilityReport
		) { }

		public void ProcessHitStabilityReport
		(
			Collider hitCollider,
			Vector3 hitNormal,
			Vector3 hitPoint,
			Vector3 atCharacterPosition,
			Quaternion atCharacterRotation,
			ref HitStabilityReport hitStabilityReport
		) { }

		public void DiscreteCollisionDetected(Collider hitCollider) { }

		#endregion

		public void Disable() { }

		public void Exit() { }

		public void OnTransitionTrue(IKCCState toState) { }
	}
}