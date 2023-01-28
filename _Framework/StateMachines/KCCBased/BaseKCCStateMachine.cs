using Bezoro.Core;
using UnityEngine;
using System.Collections.Generic;
using KinematicCharacterController;

namespace Bezoro.StateMachines
{
	[DisallowMultipleComponent]
	public abstract class BaseKCCStateMachine : Tickable,
		IKCCStateMachine, ICharacterController
	{
		private void Awake()
		{
			Motor.CharacterController = this;
			RegisterStates();
			SetUpTransitions();
		}

		public IKCCState CurrentState => _currentState;

		ITickableState ITickableStateMachine.CurrentState => _currentState;

		IState IStateMachine.CurrentState => _currentState;

		public IKCCState LastState => _lastState;

		ITickableState ITickableStateMachine.LastState => _lastState;

		IState IStateMachine.LastState => _lastState;

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
			if (_restarted)
			{
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

				if (!_currentState.IsSubStateMachine(out IKCCStateMachine rootSubStateMachine))
					return false;

				var currentState = rootSubStateMachine.CurrentState;
				while (true)
				{
					if (CheckInState(currentState))
						return true;

					if (!currentState.IsSubStateMachine(out IKCCStateMachine subStateMachine))
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
			_currentState.GroundHit(hitCollider,
								    hitNormal,
								    hitPoint,
								    ref hitStabilityReport);

		public void OnMovementHit
		(Collider hitCollider,
		 Vector3 hitNormal,
		 Vector3 hitPoint,
		 ref HitStabilityReport hitStabilityReport) =>
			_currentState.MovementHit(hitCollider,
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
			if (DisableMethod is DisableMethod.Restart)
				_restarted = true;
		}

		public void SetMotor(KinematicCharacterMotor motor) =>
			Motor = motor;

		public List<IState> GetCurrentHierarchy()
		{
			var currentStateHierarchy = new List<IState> { _currentState };

			if (_currentState is not IStateMachine rootSuperState)
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
		
		public void DoTransition(IKCCState toState) =>
			this.DoTransition_Ext(toState);

		void ITickableStateMachine.DoTransition(ITickableState toState) =>
			this.DoTransition_Ext(toState);

		void IStateMachine.DoTransition(IState toState) =>
			this.DoTransition_Ext(toState);

		public void SetCurrentState(IKCCState state) =>
			_currentState = state;

		void ITickableStateMachine.SetCurrentState(ITickableState state) =>
			_currentState = state as IKCCState;

		void IStateMachine.SetCurrentState(IState state) =>
			_currentState = state as IKCCState;

		public void SetLastState(IKCCState state) =>
			_lastState = state;

		void ITickableStateMachine.SetLastState(ITickableState state) =>
			_lastState = state as IKCCState;

		void IStateMachine.SetLastState(IState state) =>
			_lastState = state as IKCCState;

		#region Abstract Methods

		protected abstract void RegisterStates();

		protected abstract void SetUpTransitions();

		protected abstract IKCCState ResolveStartState();

		#endregion

		protected T RegisterState<T>()
		where T : class, IKCCState, new() =>
			this.CreateState<T, IKCCStateMachine>(this);

		private IKCCState _currentState = new NullKCCState();
		private IKCCState _lastState = new NullKCCState();
		private bool _restarted;
	}
}