using Bezoro.Core;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using KinematicCharacterController;

namespace Bezoro.StateMachines
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(KinematicCharacterMotor))]
	public sealed class KCCStateMachineOwner : Tickable,
		ICharacterController
	{
		public BaseKCCStateMachine StateMachine => stateMachine;

		[SerializeField] [Required]
		private BaseKCCStateMachine stateMachine;

		[field: SerializeField] [Required]
		public KinematicCharacterMotor Motor { get; private set; }

		private void Awake()
		{
			Motor.CharacterController = this;
			var type = stateMachine.GetType();
			var newStateMachineInstance = ScriptableObject.CreateInstance(type);
			stateMachine = newStateMachineInstance as BaseKCCStateMachine;
			stateMachine.Init(this);
		}

		private void Start() =>
			stateMachine.Start();

		protected override void OnEnable()
		{
			base.OnEnable();
			stateMachine.Enable();
		}

		public override void EarlyTick() =>
			stateMachine.EarlyTick();

		public override void Tick() =>
			stateMachine.Tick();

		public override void FixedTick() =>
			stateMachine.FixedTick();

		public override void LateTick() =>
			stateMachine.LateTick();

		#region KCC Methods

		public void UpdateRotation(ref Quaternion currentRotation, float deltaTime) =>
			stateMachine.UpdateRotation(ref currentRotation, deltaTime);

		public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime) =>
			stateMachine.UpdateVelocity(ref currentVelocity, deltaTime);

		public void BeforeCharacterUpdate(float deltaTime) =>
			stateMachine.BeforeCharacterUpdate(deltaTime);

		public void PostGroundingUpdate(float deltaTime) =>
			stateMachine.PostGroundingUpdate(deltaTime);

		public void AfterCharacterUpdate(float deltaTime) =>
			stateMachine.AfterCharacterUpdate(deltaTime);

		public bool IsColliderValidForCollisions(Collider coll)
		{
			return !CheckForMonoBehaviourToIgnoreCollisionsWith();

			bool CheckForMonoBehaviourToIgnoreCollisionsWith()
			{
				var currentState = stateMachine.CurrentState;
				if (CheckInState(currentState))
					return true;

				if (!currentState.IsSubStateMachine(out var rootSubStateMachine))
					return false;

				currentState = rootSubStateMachine.CurrentState;
				while (true)
				{
					if (CheckInState(currentState))
						return true;

					if (!currentState.IsSubStateMachine(out var subStateMachine))
						return false;

					currentState = subStateMachine.CurrentState;
				}

				bool CheckInState(IKCCState state)
				{
					foreach (var monoBehaviour in
							 state.MonoBehaviourTypesToIgnoreCollisionsWith)
					{
						if (coll.TryGetComponent(monoBehaviour, out _))
							return true;
					}

					return false;
				}
			}
		}

		public void OnGroundHit
		(Collider hitCollider,
		 Vector3 hitNormal,
		 Vector3 hitPoint,
		 ref HitStabilityReport hitStabilityReport) =>
			stateMachine.OnGroundHit(hitCollider, hitNormal, hitPoint, ref hitStabilityReport);

		public void OnMovementHit
		(Collider hitCollider,
		 Vector3 hitNormal,
		 Vector3 hitPoint,
		 ref HitStabilityReport hitStabilityReport) =>
			stateMachine.OnMovementHit(hitCollider, hitNormal, hitPoint, ref hitStabilityReport);

		public void ProcessHitStabilityReport
		(Collider hitCollider,
		 Vector3 hitNormal,
		 Vector3 hitPoint,
		 Vector3 atCharacterPosition,
		 Quaternion atCharacterRotation,
		 ref HitStabilityReport hitStabilityReport) =>
			stateMachine.ProcessHitStabilityReport(hitCollider,
												   hitNormal,
												   hitPoint,
												   atCharacterPosition,
												   atCharacterRotation,
												   ref hitStabilityReport);

		public void OnDiscreteCollisionDetected(Collider hitCollider) =>
			stateMachine.OnDiscreteCollisionDetected(hitCollider);

		#endregion

		protected override void OnDisable()
		{
			base.OnDisable();
			stateMachine.Disable();
		}

		public bool TryFindStateInCurrentHierarchy<T>(out T outState)
		where T : class, IState
		{
			var boolean = stateMachine.TryFindStateInCurrentHierarchy(out T state);
			outState = state;
			return boolean;
		}

		public bool TryFindStateInLastHierarchy<T>(out T outState)
		where T : class, IState
		{
			var boolean = stateMachine.TryFindStateInLastHierarchy(out T state);
			outState = state;
			return boolean;
		}

		public List<IKCCState> GetCurrentHierarchy()
		{
			var currentState = stateMachine.CurrentState;
			var currentStateHierarchy = new List<IKCCState> { currentState };

			if (currentState is not IKCCStateMachine rootSuperState)
				return currentStateHierarchy;

			var parentState = rootSuperState;

			while (true)
			{
				currentState = parentState.CurrentState;

				currentStateHierarchy.Add(currentState);

				if (currentState is not IKCCStateMachine superState)
					return currentStateHierarchy;

				parentState = superState;
			}
		}

		public void SetStateMachine(BaseKCCStateMachine stateMachine)
		{
			this.stateMachine.Exit();
			this.stateMachine = stateMachine;
			this.stateMachine.Init(this);
			this.stateMachine.Start();
		}

		private void OnValidate()
		{
			if (Application.isPlaying)
				return;

			Motor = GetComponent<KinematicCharacterMotor>();
		}
	}
}