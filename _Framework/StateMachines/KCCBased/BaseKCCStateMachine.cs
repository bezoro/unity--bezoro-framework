using UnityEngine;
using KinematicCharacterController;

namespace Bezoro.StateMachines
{
	public abstract class BaseKCCStateMachine : ScriptableObject,
		IKCCStateMachine
	{
		public void Init(KCCStateMachineOwner owner)
		{
			_owner = owner;
			RegisterStates();
			SetUpTransitions();
			OnInit();
		}

		public IKCCState CurrentState => _currentState;

		public IKCCState LastState => _lastState;

		[field: SerializeField]
		public DisableMethod DisableMethod { get; private set; }

		public void Start()
		{
			OnStart();
			_currentState = ResolveStartState();
			_currentState.Enter();
		}

		public void Enable()
		{
			OnEnabled();
			if (_wasDisabled && DisableMethod is DisableMethod.Restart)
			{
				_currentState = ResolveStartState();
				_currentState.Enter();
				_wasDisabled = false;
			}
			else if (_wasDisabled)
			{
				_currentState.Enable();
				_wasDisabled = false;
			}
		}

		public void EarlyTick() =>
			_currentState.EarlyTick();

		public void Tick() =>
			_currentState.Tick();

		public void FixedTick() =>
			_currentState.FixedTick();

		public void LateTick() =>
			_currentState.LateTick();

		#region KCC Methods

		public void UpdateRotation(ref Quaternion currentRotation, float deltaTime) =>
			_currentState.UpdateRotation(ref currentRotation, deltaTime);

		public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime) =>
			_currentState.UpdateVelocity(ref currentVelocity, deltaTime);

		public void BeforeCharacterUpdate(float deltaTime) =>
			_currentState.BeforeCharacterUpdate(deltaTime);

		public void PostGroundingUpdate(float deltaTime) =>
			_currentState.PostGroundingUpdate(deltaTime);

		public void AfterCharacterUpdate(float deltaTime) =>
			_currentState.AfterCharacterUpdate(deltaTime);

		public bool IsColliderValidForCollisions(Collider coll)
		{
			return !CheckForMonoBehaviourToIgnoreCollisionsWith();

			bool CheckForMonoBehaviourToIgnoreCollisionsWith()
			{
				if (CheckInState(_currentState))
					return true;

				if (!_currentState.IsSubStateMachine(out var rootSubStateMachine))
					return false;

				var currentState = rootSubStateMachine.CurrentState;
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
			_currentState.GroundHit(hitCollider, hitNormal, hitPoint, ref hitStabilityReport);

		public void OnMovementHit
		(Collider hitCollider,
		 Vector3 hitNormal,
		 Vector3 hitPoint,
		 ref HitStabilityReport hitStabilityReport) =>
			_currentState.MovementHit(hitCollider, hitNormal, hitPoint, ref hitStabilityReport);

		public void ProcessHitStabilityReport
		(Collider hitCollider,
		 Vector3 hitNormal,
		 Vector3 hitPoint,
		 Vector3 atCharacterPosition,
		 Quaternion atCharacterRotation,
		 ref HitStabilityReport hitStabilityReport) =>
			_currentState.ProcessHitStabilityReport(hitCollider,
												    hitNormal,
												    hitPoint,
												    atCharacterPosition,
												    atCharacterRotation,
												    ref hitStabilityReport);

		public void OnDiscreteCollisionDetected(Collider hitCollider) =>
			_currentState.DiscreteCollisionDetected(hitCollider);

		#endregion
		
		public void Exit()
		{
			OnExit();
			_currentState.Exit();
		}

		public void Disable()
		{
			OnDisabled();
			_currentState.Disable();
			_wasDisabled = true;
		}

		public void Destroy()
		{
			OnDestroyed();
			_currentState.Destroy();
		}

		public void DoTransition(IKCCState toState)
		{
			_currentState.Exit();
			_lastState = _currentState;
			_currentState = toState;
			_currentState.Enter();
		}

		#region Abstract Methods

		/// <summary>
		/// Called once in the lifetime of the object, in <b>Awake</b>.
		/// Use it to register all <b>States</b> needed for the <b>StateMachine</b> to work.
		/// </summary>
		protected abstract void RegisterStates();

		/// <summary>
		/// Called once in the lifetime of the object, in <b>Awake</b>.
		/// Use it to set up all of the <b>States</b> <b>Transitions</b>.
		/// </summary>
		protected abstract void SetUpTransitions();

		/// <summary>
		/// Called once in the lifetime of the object, when it is created.
		/// Use it like <b>Awake</b> in a <b>MonoBehaviour</b>.
		/// </summary>
		protected abstract void OnInit();

		/// <summary>
		/// Called once in the lifetime of the object, in <b>Start</b>.
		/// Use it to decide which should be the <b>Start</b> <b>State</b>, then return it.
		/// </summary>
		protected abstract IKCCState ResolveStartState();

		/// <summary>
		/// Called once in the lifetime of the object when it is <i>Started</i>.
		/// </summary>
		protected abstract void OnStart();

		/// <summary>
		/// Called every time the Owner gets enabled, while its being executed.
		/// </summary>
		protected abstract void OnEnabled();

		/// <summary>
		/// Called once when the Owner switches State Machines.
		/// </summary>
		protected abstract void OnExit();

		/// <summary>
		/// Called every time the Owner gets disabled, while its being executed.
		/// </summary>
		protected abstract void OnDisabled();

		/// <summary>
		/// Called when the Owner gets destroyed, while its being executed.
		/// </summary>
		protected abstract void OnDestroyed();

		#endregion

		protected T RegisterState<T>()
		where T : class, IKCCState, new()
		{
			var newState = new T();
			newState.Init(_owner, this, _owner.Motor);
			return newState;
		}

		private KCCStateMachineOwner _owner;
		private IKCCState _currentState;
		private IKCCState _lastState;
		private bool _wasDisabled;
	}
}