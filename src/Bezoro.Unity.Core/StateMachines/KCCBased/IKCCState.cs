using System;
using System.Collections.Generic;
using Bezoro.Unity.Core.ThirdParty.KinematicCharacterController.Runtime;
using UnityEngine;

namespace Bezoro.Unity.Core.StateMachines.KCCBased;

/// <summary>
///     Defines a KCC state that receives character controller callbacks.
/// </summary>
public interface IKCCState : IState,
	ITickable,
	IHasTransitions<KCCTransition, IKCCState>
{
	public List<Type> MonoBehaviourTypesToIgnoreCollisionsWith { get; }

	public bool IsSubStateMachine(out IKCCStateMachine outStateMachine);

	public void Init(
		KCCStateMachineOwner    owner,
		IKCCStateMachine        owningStateMachine,
		KinematicCharacterMotor motor
	);

	#region KCC Methods

	public void UpdateRotation(ref Quaternion currentRotation, float deltaTime);

	public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime);

	public void BeforeCharacterUpdate(float deltaTime);

	public void PostGroundingUpdate(float deltaTime);

	public void AfterCharacterUpdate(float deltaTime);

	public void GroundHit(
		Collider               hitCollider,
		Vector3                hitNormal,
		Vector3                hitPoint,
		ref HitStabilityReport hitStabilityReport
	);

	public void MovementHit(
		Collider               hitCollider,
		Vector3                hitNormal,
		Vector3                hitPoint,
		ref HitStabilityReport hitStabilityReport
	);

	public void ProcessHitStabilityReport(
		Collider               hitCollider,
		Vector3                hitNormal,
		Vector3                hitPoint,
		Vector3                atCharacterPosition,
		Quaternion             atCharacterRotation,
		ref HitStabilityReport hitStabilityReport
	);

	public void DiscreteCollisionDetected(Collider hitCollider);

	#endregion
}
