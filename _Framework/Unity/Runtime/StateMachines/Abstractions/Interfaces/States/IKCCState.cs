using System;
using System.Collections.Generic;
using Bezoro.Framework.Unity.StateMachines.KCCBased;
using KCC;
using UnityEngine;

namespace Bezoro.Framework.Unity.StateMachines
{
	public interface IKCCState : IState,
		ITickable,
		IHasTransitions<KCCTransition, IKCCState>
	{
		public List<Type> MonoBehaviourTypesToIgnoreCollisionsWith { get; }

		public void Init(
			KCCStateMachineOwner owner,
			IKCCStateMachine owningStateMachine,
			KinematicCharacterMotor motor
		);

		public bool IsSubStateMachine( out IKCCStateMachine outStateMachine );

	#region KCC Methods

		public void UpdateRotation( ref Quaternion currentRotation, float deltaTime );

		public void UpdateVelocity( ref Vector3 currentVelocity, float deltaTime );

		public void BeforeCharacterUpdate( float deltaTime );

		public void PostGroundingUpdate( float deltaTime );

		public void AfterCharacterUpdate( float deltaTime );

		public void GroundHit(
			Collider hitCollider,
			Vector3 hitNormal,
			Vector3 hitPoint,
			ref HitStabilityReport hitStabilityReport
		);

		public void MovementHit(
			Collider hitCollider,
			Vector3 hitNormal,
			Vector3 hitPoint,
			ref HitStabilityReport hitStabilityReport
		);

		public void ProcessHitStabilityReport(
			Collider hitCollider,
			Vector3 hitNormal,
			Vector3 hitPoint,
			Vector3 atCharacterPosition,
			Quaternion atCharacterRotation,
			ref HitStabilityReport hitStabilityReport
		);

		public void DiscreteCollisionDetected( Collider hitCollider );

	#endregion
	}
}
