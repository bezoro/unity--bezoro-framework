using Bezoro.Core;
using UnityEngine;
using System.Collections.Generic;
using KinematicCharacterController;
using Debug = Sisus.Debugging.Debug;

namespace Bezoro.StateMachines
{
	[DisallowMultipleComponent]
	public abstract class BaseKCCStateMachine : Tickable,
		IKCCStateMachine, ICharacterController
	{
		[field: SerializeField]
		public KinematicCharacterMotor Motor { get; private set; }

		public IKCCState CurrentState => _currentState;

		public IKCCState LastState => _lastState;

		[field: SerializeField]
		public DisableMethod DisableMethod { get; private set; } = DisableMethod.Continue;

		public void SetMotor(KinematicCharacterMotor motor) =>
			Motor = motor;

		private void Awake() =>
			SetUp();

		protected abstract void SetUp();

		protected void SetStartState(IKCCState state) =>
			_startState = state;

		protected void RegisterState<T>(out T outState)
		where T : class, IKCCState, new() =>
			outState = CreateState<T>();

		private void Start()
		{
			_currentState = _startState;
			_currentState.Enter();
		}

		protected override void OnDisable()
		{
			base.OnDisable();

			_currentState.Disable();

			if (DisableMethod is DisableMethod.Restart)
				_currentState = _startState;
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

				if (!IsSubStateMachine(_currentState, out var rootSuperState))
					return false;

				var parentState = rootSuperState;

				while (true)
				{
					var currentState = parentState.CurrentState;

					if (CheckInState(currentState))
						return true;

					if (!IsSubStateMachine(currentState, out var superState))
						return false;

					parentState = superState;
				}

				bool CheckInState(IKCCState state)
				{
					foreach (var monoBehaviour in state.MonoBehaviourTypesToIgnoreCollisionsWith)
						if (coll.TryGetComponent(monoBehaviour, out _))
							return true;

					return false;
				}
			}
		}

		public void OnGroundHit
		(
			Collider hitCollider,
			Vector3 hitNormal,
			Vector3 hitPoint,
			ref HitStabilityReport hitStabilityReport
		) =>
			_currentState.GroundHit
				(
				 hitCollider,
				 hitNormal,
				 hitPoint,
				 ref hitStabilityReport
				);

		public void OnMovementHit
		(
			Collider hitCollider,
			Vector3 hitNormal,
			Vector3 hitPoint,
			ref HitStabilityReport hitStabilityReport
		) =>
			_currentState.MovementHit
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
			_currentState.ProcessHitStabilityReport
				(
				 hitCollider,
				 hitNormal,
				 hitPoint,
				 atCharacterPosition,
				 atCharacterRotation,
				 ref hitStabilityReport
				);

		public void OnDiscreteCollisionDetected(Collider hitCollider) =>
			_currentState.DiscreteCollisionDetected(hitCollider);

		#endregion

		public void DoTransition(IKCCState toState)
		{
			_lastState = _currentState;
			_currentState.Exit();
			_currentState = toState;
			_currentState.Enter();
		}

		public bool TryFindStateInCurrentHierarchy<T>(out IKCCState outState)
		where T : IKCCState
		{
			if (_currentState is T)
			{
				outState = _currentState;
				return true;
			}

			if (!IsSubStateMachine(_currentState, out var rootSuperState))
			{
				outState = null;
				return false;
			}

			var parentState = rootSuperState;

			while (true)
			{
				var currentState = parentState.CurrentState;

				if (currentState is T)
				{
					outState = currentState;
					return true;
				}

				if (!IsSubStateMachine(currentState, out var superState))
				{
					outState = null;
					return false;
				}

				parentState = superState;
			}
		}

		public bool TryFindStateInLastHierarchy<T>(out IKCCState outState)
		where T : IKCCState
		{
			if (_lastState is T)
			{
				outState = _lastState;
				return true;
			}

			if (!IsSubStateMachine(_lastState, out var rootSuperState))
			{
				outState = null;
				return false;
			}

			var parentState = rootSuperState;

			while (true)
			{
				var lastState = parentState.LastState;

				if (lastState is T)
				{
					outState = lastState;
					return true;
				}

				if (!IsSubStateMachine(lastState, out var superState))
				{
					outState = null;
					return false;
				}

				parentState = superState;
			}
		}

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

		private T CreateState<T>()
		where T : class, IKCCState, new()
		{
			var newState = new T();
			newState.Init(this, this, Motor);
			return newState;
		}

		private static bool IsSubStateMachine
		(
			IKCCState state,
			out IKCCStateMachine outSubStateMachine
		)
		{
			if (state is IKCCStateMachine stateMachine)
			{
				outSubStateMachine = stateMachine;
				return true;
			}

			outSubStateMachine = null;
			return false;
		}

		private IKCCState _startState = new NullKCCState();
		private IKCCState _currentState = new NullKCCState();
		private IKCCState _lastState = new NullKCCState();
	}
}