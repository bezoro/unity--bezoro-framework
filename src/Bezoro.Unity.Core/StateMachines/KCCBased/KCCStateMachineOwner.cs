using System.Collections.Generic;
using Bezoro.Unity.Core.StateMachines.Utilities;
using Bezoro.Unity.Core.ThirdParty.KinematicCharacterController.Runtime;
using Bezoro.Unity.Core.ThirdParty.KinematicCharacterController.Runtime.Abstractions;
using Bezoro.Unity.Core.Tickable;
using UnityEngine;

namespace Bezoro.Unity.Core.StateMachines.KCCBased
{
	[DisallowMultipleComponent]
	[RequireComponent(
		typeof(KinematicCharacterMotor)
	)]
	/// <summary>
	/// Runs a KCC state machine from a MonoBehaviour owner.
	/// </summary>
	public sealed class KCCStateMachineOwner : MonoBehaviour,
		IEarlyTickable,
		IFixedTickable,
		ILateTickable,
		IRegularTickable,
		ICharacterController
	{
		private bool _isTickRegistered;

		public BaseKCCStateMachine StateMachine => stateMachine;

		#region Unity Lifecycle

		private void Awake()
		{
			if (Motor == null)
				Motor = GetComponent<KinematicCharacterMotor>();

			if (Motor == null || stateMachine == null)
			{
				Debug.LogError($"{nameof(KCCStateMachineOwner)} requires a motor and state machine.", this);
				enabled = false;
				return;
			}

			Motor.CharacterController = this;
			stateMachine              = CreateRuntimeStateMachine(stateMachine);
			stateMachine.Init(
				this
			);
		}

		private void Start() =>
			stateMachine?.Start();

		private void OnEnable()
		{
			RegisterTickable();
			stateMachine?.Enable();
		}

		private void OnDisable()
		{
			UnregisterTickable();
			stateMachine?.Disable();
		}

		private void OnDestroy()
		{
			if (stateMachine == null) return;

			stateMachine.Destroy();
			Destroy(stateMachine);
		}

		private void OnValidate()
		{
			if (UnityEngine.Application.isPlaying) return;

			Motor = GetComponent<KinematicCharacterMotor>();
		}

		#endregion

		#region Tickable

		public void EarlyTick() =>
			stateMachine?.EarlyTick();

		public void FixedTick() =>
			stateMachine?.FixedTick();

		public void LateTick() =>
			stateMachine?.LateTick();

		public void RegularTick() =>
			stateMachine?.Tick();

		#endregion

		public bool TryFindStateInCurrentHierarchy<T>(out T outState)
			where T : class, IState
		{
			if (stateMachine == null)
			{
				outState = null!;
				return false;
			}

			bool boolean = stateMachine.TryFindStateInCurrentHierarchy(
				out T state
			);

			outState = state;

			return boolean;
		}

		public bool TryFindStateInLastHierarchy<T>(out T outState)
			where T : class, IState
		{
			if (stateMachine == null)
			{
				outState = null!;
				return false;
			}

			bool boolean = stateMachine.TryFindStateInLastHierarchy(
				out T state
			);

			outState = state;

			return boolean;
		}

		public List<IKCCState> GetCurrentHierarchy()
		{
			if (stateMachine == null)
				return new();

			var currentState          = stateMachine.CurrentState;
			var currentStateHierarchy = new List<IKCCState> { currentState };

			if (currentState is not IKCCStateMachine rootSuperState) return currentStateHierarchy;

			var parentState = rootSuperState;

			while (true)
			{
				currentState = parentState.CurrentState;

				currentStateHierarchy.Add(
					currentState
				);

				if (currentState is not IKCCStateMachine superState) return currentStateHierarchy;

				parentState = superState;
			}
		}

		public void SetStateMachine(BaseKCCStateMachine stateMachine)
		{
			if (stateMachine == null)
			{
				Debug.LogError($"{nameof(KCCStateMachineOwner)} cannot set a null state machine.", this);
				return;
			}

			if (this.stateMachine != null)
			{
				this.stateMachine.Exit();
				this.stateMachine.Destroy();
				Destroy(this.stateMachine);
			}

			this.stateMachine = CreateRuntimeStateMachine(stateMachine);
			this.stateMachine.Init(
				this
			);

			this.stateMachine.Start();
		}

		private static BaseKCCStateMachine CreateRuntimeStateMachine(BaseKCCStateMachine source)
			=> Instantiate(source);

		private void RegisterTickable()
		{
			if (_isTickRegistered)
				return;

			_isTickRegistered = TickableRegistration.Register(this);
		}

		private void UnregisterTickable()
		{
			if (!_isTickRegistered)
				return;

			TickableRegistration.Unregister(this);
			_isTickRegistered = false;
		}

		#region Serialized Fields

		[SerializeField]        private BaseKCCStateMachine     stateMachine = null!;
		[field: SerializeField] public  KinematicCharacterMotor Motor { get; private set; } = null!;

		#endregion

		#region KCC Methods

		public void UpdateRotation(ref Quaternion currentRotation, float deltaTime) =>
			stateMachine?.UpdateRotation(
				ref currentRotation,
				deltaTime
			);

		public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime) =>
			stateMachine?.UpdateVelocity(
				ref currentVelocity,
				deltaTime
			);

		public void BeforeCharacterUpdate(float deltaTime) =>
			stateMachine?.BeforeCharacterUpdate(
				deltaTime
			);

		public void PostGroundingUpdate(float deltaTime) =>
			stateMachine?.PostGroundingUpdate(
				deltaTime
			);

		public void AfterCharacterUpdate(float deltaTime) =>
			stateMachine?.AfterCharacterUpdate(
				deltaTime
			);

		public bool IsColliderValidForCollisions(Collider coll)
		{
			if (stateMachine == null)
				return true;

			return !CheckForMonoBehaviourToIgnoreCollisionsWith();

			bool CheckForMonoBehaviourToIgnoreCollisionsWith()
			{
				var currentState = stateMachine.CurrentState;

				if (CheckInState(
						currentState
					)) return true;

				if (!currentState.IsSubStateMachine(
						out var rootSubStateMachine
					)) return false;

				currentState = rootSubStateMachine.CurrentState;

				while (true)
				{
					if (CheckInState(
							currentState
						)) return true;

					if (!currentState.IsSubStateMachine(
							out var subStateMachine
						)) return false;

					currentState = subStateMachine.CurrentState;
				}

				bool CheckInState(IKCCState state)
				{
					foreach (var monoBehaviour in
							 state.MonoBehaviourTypesToIgnoreCollisionsWith)
					{
						if (coll.TryGetComponent(
								monoBehaviour,
								out _
							))
							return true;
					}

					return false;
				}
			}
		}

		public void OnGroundHit(
			Collider               hitCollider,
			Vector3                hitNormal,
			Vector3                hitPoint,
			ref HitStabilityReport hitStabilityReport
		) =>
			stateMachine?.OnGroundHit(
				hitCollider,
				hitNormal,
				hitPoint,
				ref hitStabilityReport
			);

		public void OnMovementHit(
			Collider               hitCollider,
			Vector3                hitNormal,
			Vector3                hitPoint,
			ref HitStabilityReport hitStabilityReport
		) =>
			stateMachine?.OnMovementHit(
				hitCollider,
				hitNormal,
				hitPoint,
				ref hitStabilityReport
			);

		public void ProcessHitStabilityReport(
			Collider               hitCollider,
			Vector3                hitNormal,
			Vector3                hitPoint,
			Vector3                atCharacterPosition,
			Quaternion             atCharacterRotation,
			ref HitStabilityReport hitStabilityReport
		) =>
			stateMachine?.ProcessHitStabilityReport(
				hitCollider,
				hitNormal,
				hitPoint,
				atCharacterPosition,
				atCharacterRotation,
				ref hitStabilityReport
			);

		public void OnDiscreteCollisionDetected(Collider hitCollider) =>
			stateMachine?.OnDiscreteCollisionDetected(
				hitCollider
			);

		#endregion
	}
}
