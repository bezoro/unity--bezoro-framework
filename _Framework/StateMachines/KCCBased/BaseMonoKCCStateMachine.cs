using Bezoro.Core;
using UnityEngine;
using System.Collections.Generic;
using KinematicCharacterController;

namespace Bezoro.StateMachines
{
	[DisallowMultipleComponent]
	public abstract class BaseMonoKCCStateMachine : Tickable,
		IKCCStateMachine, ICharacterController
	{
		private void Awake()
		{
			Motor.CharacterController = this;
			RegisterStates();
			SetUpTransitions();
		}

		public IKCCState CurrentState => _currentState;

		public IKCCState LastState => _lastState;

		public bool Restarted => _restarted;

		[field: SerializeField]
		public DisableMethod DisableMethod { get; private set; }

		[field: SerializeField]
		public KinematicCharacterMotor Motor { get; private set; }

		private void Start()
		{
			_currentState = ResolveStartState();
			_currentState.Enter();
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			HandleRestarted();

			void HandleRestarted()
			{
				if (!_restarted)
					return;

				_restarted = false;
				_currentState = ResolveStartState();
				_currentState.Enter();
			}
		}

		public override void EarlyTick() =>
			_currentState.EarlyTick();

		public override void Tick() =>
			_currentState.Tick();

		public override void FixedTick() =>
			_currentState.FixedTick();

		public override void LateTick() =>
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

		protected override void OnDisable()
		{
			base.OnDisable();
			HandleDisableMethod();

			void HandleDisableMethod()
			{
				if (DisableMethod is DisableMethod.Restart)
					_restarted = true;
			}
		}

		public void DoTransition(IKCCState toState)
		{
			_currentState.Exit();
			_lastState = _currentState;
			_currentState = toState;
			_currentState.Enter();
		}

		public void SetMotor(KinematicCharacterMotor motor) =>
			Motor = motor;

		public List<IKCCState> GetCurrentHierarchy()
		{
			var currentStateHierarchy = new List<IKCCState> { _currentState };

			if (_currentState is not IKCCStateMachine rootSuperState)
				return currentStateHierarchy;

			var parentState = rootSuperState;

			while (true)
			{
				var currentState = parentState.CurrentState;

				currentStateHierarchy.Add(currentState);

				if (currentState is not IKCCStateMachine superState)
					return currentStateHierarchy;

				parentState = superState;
			}
		}

		public bool TryFindStateInCurrentHierarchy<T>(out T outState)
		where T : class, IState
		{
			var boolean = StateMachineExtensions.TryFindStateInCurrentHierarchy(this, out T state);
			outState = state;
			return boolean;
		}

		public bool TryFindStateInLastHierarchy<T>(out T outState)
		where T : class, IState
		{
			var boolean = StateMachineExtensions.TryFindStateInLastHierarchy(this, out T state);
			outState = state;
			return boolean;
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
		/// Called once in the lifetime of the object, in <b>Start</b>.
		/// Use it to decide which should be the <b>Start</b> <b>State</b>, then return it.
		/// </summary>
		protected abstract IKCCState ResolveStartState();

		#endregion

		protected T RegisterState<T>()
		where T : class, IKCCState, new()
		{
			var newState = new T();
			newState.Init(this, this, Motor);
			return newState;
		}

		private IKCCState _currentState;
		private IKCCState _lastState;
		private bool _restarted;
	}
}