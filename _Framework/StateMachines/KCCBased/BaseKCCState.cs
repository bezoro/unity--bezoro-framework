using System;
using UnityEngine;
using Bezoro.Core.Helpers;
using System.Collections.Generic;
using KinematicCharacterController;

namespace Bezoro.StateMachines
{
	public abstract class BaseKCCState :
		IKCCState
	{
		public void Init
		(KCCStateMachineOwner owner,
		 IKCCStateMachine owningStateMachine,
		 KinematicCharacterMotor motor)
		{
			_owner = owner;
			_owningStateMachine = owningStateMachine;
			_motor = motor;
			OnInit();
		}

		public MonoBehaviour Owner => _owner;

		public KCCTransition[] Transitions => _transitions;

		public KinematicCharacterMotor Motor => _motor;

		public List<Type> MonoBehaviourTypesToIgnoreCollisionsWith { get; } = new(4);

		public void Enter()
		{
			OnEnter();
			AllTransitionsBecomeRelevant();

			void AllTransitionsBecomeRelevant()
			{
				for (var i = 0; i < _transitions.Length; i++)
				{
					if (_transitions[i] == null)
						continue;

					_transitions[i].BecomeRelevant();
				}
			}
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
			AllTransitionsTick();

			void AllTransitionsTick()
			{
				for (var i = 0; i < _transitions.Length; i++)
				{
					if (_transitions[i] == null)
						continue;

					_transitions[i].Tick();
				}
			}
		}

		public void GroundHit
		(Collider hitCollider,
		 Vector3 hitNormal,
		 Vector3 hitPoint,
		 ref HitStabilityReport hitStabilityReport) =>
			OnGroundHit(hitCollider, hitNormal, hitPoint, ref hitStabilityReport);

		public void MovementHit
		(Collider hitCollider,
		 Vector3 hitNormal,
		 Vector3 hitPoint,
		 ref HitStabilityReport hitStabilityReport) =>
			OnMovementHit(hitCollider, hitNormal, hitPoint, ref hitStabilityReport);

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

		public void OnTransitionRaised(IKCCState toState) =>
			_owningStateMachine.DoTransition(toState);

		public void Exit()
		{
			AllTransitionsCeaseRelevant();
			OnExit();

			void AllTransitionsCeaseRelevant()
			{
				for (var i = 0; i < _transitions.Length; i++)
				{
					if (_transitions[i] == null)
						continue;

					_transitions[i].CeaseRelevant();
				}
			}
		}

		public KCCTransition TransitionsTo(IKCCState toState)
		{
			var newTransition = new KCCTransition();
			newTransition.Init(this, toState, _owner);
			ArrayHelpers.Add(ref _transitions, newTransition);
			return newTransition;
		}

		public bool IsSubStateMachine(out IKCCStateMachine outStateMachine)
		{
			outStateMachine = null;
			return false;
		}

		public void AddMonoBehaviourToIgnoreCollisionsWith<T>()
		where T : MonoBehaviour =>
			KCCStateExtensions.AddMonoBehaviourToIgnoreCollisionsWith<T>(this);

		#region Abstract Methods

		/// <summary>
		/// Called once in the lifetime of the object, when it is created.
		/// Use it like <b>Awake</b> in a <b>MonoBehaviour</b>.
		/// </summary>
		protected abstract void OnInit();

		/// <summary>
		/// Called when the <b>SubStateMachine</b> is <i>Entered</i>.
		/// </summary>
		protected abstract void OnEnter();

		/// <summary>
		/// Called every frame, before <b>OnTick</b>.
		/// </summary>
		protected abstract void OnEarlyTick();

		/// <summary>
		/// Called every frame.
		/// </summary>
		protected abstract void OnTick();

		/// <summary>
		/// Called at the same rate as <b>FixedUpdate</b>.
		/// </summary>
		protected abstract void OnFixedTick();

		/// <summary>
		/// Called every frame after <b>OnTick</b>.
		/// </summary>
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

		/// <summary>
		/// Called when the <b>SubStateMachine</b> is <i>Exited</i>.
		/// </summary>
		protected abstract void OnExit();

		#endregion

		private KCCStateMachineOwner _owner;
		private IKCCStateMachine _owningStateMachine;
		private KinematicCharacterMotor _motor;
		private KCCTransition[] _transitions = new KCCTransition[4];
	}
}