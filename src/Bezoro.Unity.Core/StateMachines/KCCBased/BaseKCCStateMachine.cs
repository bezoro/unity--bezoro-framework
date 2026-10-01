using Bezoro.Unity.Core.ThirdParty.KinematicCharacterController.Runtime;
using UnityEngine;

namespace Bezoro.Unity.Core.StateMachines.KCCBased
{
	/// <summary>
	///     Manages KCC states, transitions, and nested sub-state machines.
	/// </summary>
	public abstract class BaseKCCStateMachine : ScriptableObject,
		IKCCStateMachine
	{
		#region Serialized Fields

		[field: SerializeField] public DisableMethod DisableMethod { get; private set; }

		#endregion

		private bool _hasStarted;
		private bool _wasDisabled;

		private KCCStateMachineOwner _owner = null!;
		public void Init(KCCStateMachineOwner owner)
		{
			_owner = owner;
			RegisterStates();
			SetUpTransitions();
			OnInit();
		}

		public IKCCState CurrentState { get; private set; } = null!;
		public IKCCState LastState { get; private set; } = null!;
		public void Destroy()
		{
			OnDestroyed();
			if (!_hasStarted) return;

			CurrentState.Destroy();
		}

		public void Disable()
		{
			OnDisabled();
			if (!_hasStarted) return;

			CurrentState.Disable();
			_wasDisabled = true;
		}

		#region Interface Implementations

		public void DoTransition(IKCCState toState)
		{
			CurrentState.Exit();
			LastState    = CurrentState;
			CurrentState = toState;
			CurrentState.Enter();
		}

		#endregion

		public void EarlyTick()
		{
			if (!_hasStarted) return;

			CurrentState.EarlyTick();
		}

		public void Enable()
		{
			if (!_wasDisabled) return;
			if (!_hasStarted) return;

			OnEnabled();

			if (_wasDisabled && DisableMethod is DisableMethod.Restart)
			{
				CurrentState.Exit();
				LastState    = CurrentState;
				CurrentState = ResolveStartState();
				CurrentState.Enter();
			}
			else
			{
				CurrentState.Enable();
			}

			_wasDisabled = false;
		}

		public void Exit()
		{
			OnExit();
			if (!_hasStarted) return;

			CurrentState.Exit();
		}

		public void FixedTick()
		{
			if (!_hasStarted) return;

			CurrentState.FixedTick();
		}

		public void LateTick()
		{
			if (!_hasStarted) return;

			CurrentState.LateTick();
		}

		public void Start()
		{
			if (_hasStarted) return;

			OnStart();
			CurrentState = ResolveStartState();
			CurrentState.Enter();
			_hasStarted = true;
		}

		public void Tick()
		{
			if (!_hasStarted) return;

			CurrentState.Tick();
		}

		protected T RegisterState<T>()
			where T : class, IKCCState, new()
		{
			var newState = new T();

			newState.Init(
				_owner,
				this,
				_owner.Motor
			);

			return newState;
		}

		#region Abstract Methods

		/// <summary>
		///     Called when the <b>Owner</b> is <i>Destroyed</i>, while this object is still being executed.
		/// </summary>
		protected abstract void OnDestroyed();

		/// <summary>
		///     Called when the <b>Owner</b> gets <i>Disabled</i>, while this object is still being executed.
		/// </summary>
		protected abstract void OnDisabled();

		/// <summary>
		///     Called when the <b>Owner</b> gets <i>Enabled</i> and was previously <i>Disabled</i>,
		///     while this object is still being executed.
		/// </summary>
		protected abstract void OnEnabled();

		/// <summary>
		///     Called when the <b>Owner</b> switches <b>State Machines</b>.
		/// </summary>
		protected abstract void OnExit();

		/// <summary>
		///     Called once in the lifetime of the object, when it is created.
		///     Use it like <b>Awake</b> in a <b>MonoBehaviour</b>.
		/// </summary>
		protected abstract void OnInit();

		/// <summary>
		///     Called once in the lifetime of the object when it is <i>Started</i>.
		/// </summary>
		protected abstract void OnStart();

		/// <summary>
		///     Called once in the lifetime of the object, in <b>Awake</b>.
		///     Use it to register all <b>States</b> needed for the <b>StateMachine</b> to work.
		/// </summary>
		protected abstract void RegisterStates();

		/// <summary>
		///     If <b>DisableMethod</b> is <b>Continue</b>:
		///     <para>Called once in the lifetime of the object, in <b>Start</b>.</para>
		///     If <b>DisableMethod</b> is <b>Restart</b>:
		///     <para>Called once in <b>Start</b> and on <b>Enable</b>.</para>
		///     <para></para>
		///     Use it to decide which should be the <b>Start</b> <b>State</b>, then return it.
		/// </summary>
		protected abstract IKCCState ResolveStartState();

		/// <summary>
		///     Called once in the lifetime of the object, in <b>Awake</b>.
		///     Use it to set up all of the <b>States</b> <b>Transitions</b>.
		/// </summary>
		protected abstract void SetUpTransitions();

		#endregion

		#region KCC Methods

		public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
		{
			if (!_hasStarted) return;

			CurrentState.UpdateRotation(
				ref currentRotation,
				deltaTime
			);
		}

		public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
		{
			if (!_hasStarted) return;

			CurrentState.UpdateVelocity(
				ref currentVelocity,
				deltaTime
			);
		}

		public void BeforeCharacterUpdate(float deltaTime)
		{
			if (!_hasStarted) return;

			CurrentState.BeforeCharacterUpdate(
				deltaTime
			);
		}

		public void PostGroundingUpdate(float deltaTime)
		{
			if (!_hasStarted) return;

			CurrentState.PostGroundingUpdate(
				deltaTime
			);
		}

		public void AfterCharacterUpdate(float deltaTime)
		{
			if (!_hasStarted) return;

			CurrentState.AfterCharacterUpdate(
				deltaTime
			);
		}

		public bool IsColliderValidForCollisions(Collider coll)
		{
			if (!_hasStarted) return true;

			return !CheckForMonoBehaviourToIgnoreCollisionsWith();

			bool CheckForMonoBehaviourToIgnoreCollisionsWith()
			{
				if (CheckInState(
						CurrentState
					)) return true;

				if (!CurrentState.IsSubStateMachine(
						out var rootSubStateMachine
					)) return false;

				var currentState = rootSubStateMachine.CurrentState;

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
		)
		{
			if (!_hasStarted) return;

			CurrentState.GroundHit(
				hitCollider,
				hitNormal,
				hitPoint,
				ref hitStabilityReport
			);
		}

		public void OnMovementHit(
			Collider               hitCollider,
			Vector3                hitNormal,
			Vector3                hitPoint,
			ref HitStabilityReport hitStabilityReport
		)
		{
			if (!_hasStarted) return;

			CurrentState.MovementHit(
				hitCollider,
				hitNormal,
				hitPoint,
				ref hitStabilityReport
			);
		}

		public void ProcessHitStabilityReport(
			Collider               hitCollider,
			Vector3                hitNormal,
			Vector3                hitPoint,
			Vector3                atCharacterPosition,
			Quaternion             atCharacterRotation,
			ref HitStabilityReport hitStabilityReport
		)
		{
			if (!_hasStarted) return;

			CurrentState.ProcessHitStabilityReport(
				hitCollider,
				hitNormal,
				hitPoint,
				atCharacterPosition,
				atCharacterRotation,
				ref hitStabilityReport
			);
		}

		public void OnDiscreteCollisionDetected(Collider hitCollider)
		{
			if (!_hasStarted) return;

			CurrentState.DiscreteCollisionDetected(
				hitCollider
			);
		}

		#endregion
	}
}
