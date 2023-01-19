using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using KinematicCharacterController;

namespace Bezoro.StateMachines
{
	public abstract class BaseKCCSubStateMachine : IKCCStateMachine, IKCCState
	{
		public IKCCState CurrentState => _currentState;

		public IKCCState LastState => _lastState;

		public KinematicCharacterMotor Motor => _motor;

		public List<Type> MonoBehaviourTypesToIgnoreCollisionsWith { get; } = new(12);

		public virtual void Init
		(
			BaseKCCStateMachine masterStateMachine,
			IKCCStateMachine owningStateMachine,
			KinematicCharacterMotor motor
		)
		{
			_masterStateMachine = masterStateMachine;
			_owningStateMachine = owningStateMachine;
			_motor = motor;
			SetUp();
		}

		public KCCTransition TransitionsTo(IKCCState state)
		{
			var transition = new KCCTransition(this, state, _masterStateMachine);
			_transitions.Add(transition);
			return transition;
		}

		public void Enter()
		{
			_currentState = _startState;

			OnEnter();

			for (var i = 0; i < _transitions.Count; i++)
				_transitions[i].BecomeRelevant();

			_currentState.Enter();
		}

		public void Disable()
		{
			OnDisable();

			_currentState.Disable();

			if (_masterStateMachine.DisableMethod is DisableMethod.Restart)
				_currentState = _startState;
		}

		public void EarlyTick()
		{
			OnEarlyTick();
			_currentState.EarlyTick();
		}

		public void Tick()
		{
			OnTick();
			_currentState.Tick();
		}

		public void FixedTick()
		{
			OnFixedTick();
			_currentState.FixedTick();
		}

		public void LateTick()
		{
			OnLateTick();
			_currentState.LateTick();
		}

		#region KCC Methods

		public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
		{
			OnUpdateRotation(ref currentRotation, deltaTime);
			_currentState.UpdateRotation(ref currentRotation, deltaTime);
		}

		public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
		{
			OnUpdateVelocity(ref currentVelocity, deltaTime);
			_currentState.UpdateVelocity(ref currentVelocity, deltaTime);
		}

		public void BeforeCharacterUpdate(float deltaTime)
		{
			OnBeforeCharacterUpdate(deltaTime);
			_currentState.BeforeCharacterUpdate(deltaTime);
		}

		public void PostGroundingUpdate(float deltaTime)
		{
			OnPostGroundingUpdate(deltaTime);
			_currentState.PostGroundingUpdate(deltaTime);
		}

		public void AfterCharacterUpdate(float deltaTime)
		{
			OnAfterCharacterUpdate(deltaTime);
			_currentState.AfterCharacterUpdate(deltaTime);
			TickTransitions();
		}

		public void GroundHit
		(
			Collider hitCollider,
			Vector3 hitNormal,
			Vector3 hitPoint,
			ref HitStabilityReport hitStabilityReport
		)
		{
			OnGroundHit
				(
				 hitCollider,
				 hitNormal,
				 hitPoint,
				 ref hitStabilityReport
				);

			_currentState.GroundHit
				(
				 hitCollider,
				 hitNormal,
				 hitPoint,
				 ref hitStabilityReport
				);
		}

		public void MovementHit
		(
			Collider hitCollider,
			Vector3 hitNormal,
			Vector3 hitPoint,
			ref HitStabilityReport hitStabilityReport
		)
		{
			OnMovementHit
				(
				 hitCollider,
				 hitNormal,
				 hitPoint,
				 ref hitStabilityReport
				);

			_currentState.MovementHit
				(
				 hitCollider,
				 hitNormal,
				 hitPoint,
				 ref hitStabilityReport
				);
		}

		public void ProcessHitStabilityReport
		(
			Collider hitCollider,
			Vector3 hitNormal,
			Vector3 hitPoint,
			Vector3 atCharacterPosition,
			Quaternion atCharacterRotation,
			ref HitStabilityReport hitStabilityReport
		)
		{
			OnProcessHitStabilityReport
				(
				 hitCollider,
				 hitNormal,
				 hitPoint,
				 atCharacterPosition,
				 atCharacterRotation,
				 ref hitStabilityReport
				);

			_currentState.ProcessHitStabilityReport
				(
				 hitCollider,
				 hitNormal,
				 hitPoint,
				 atCharacterPosition,
				 atCharacterRotation,
				 ref hitStabilityReport
				);
		}

		public void DiscreteCollisionDetected(Collider hitCollider)
		{
			OnDiscreteCollisionDetected(hitCollider);
			_currentState.DiscreteCollisionDetected(hitCollider);
		}

		#endregion

		public void OnTransitionTrue(IKCCState toState) =>
			_owningStateMachine.DoTransition(toState);

		public void Exit()
		{
			_currentState.Exit();

			for (var i = 0; i < _transitions.Count; i++)
				_transitions[i].CeaseRelevant();

			OnExit();
		}

		public void DoTransition(IKCCState toState)
		{
			_lastState = _currentState;
			_currentState.Exit();
			_currentState = toState;
			_currentState.Enter();
		}

		#region Protected Abstract Methods

		protected abstract void SetUp();

		protected abstract void OnEnter();

		protected abstract void OnEarlyTick();

		protected abstract void OnTick();

		protected abstract void OnFixedTick();

		protected abstract void OnLateTick();

		protected abstract void OnUpdateRotation(ref Quaternion currentRotation, float deltaTime);

		protected abstract void OnUpdateVelocity(ref Vector3 currentVelocity, float deltaTime);

		protected abstract void OnBeforeCharacterUpdate(float deltaTime);

		protected abstract void OnPostGroundingUpdate(float deltaTime);

		protected abstract void OnAfterCharacterUpdate(float deltaTime);

		protected abstract void OnGroundHit
		(
			Collider hitCollider,
			Vector3 hitNormal,
			Vector3 hitPoint,
			ref HitStabilityReport hitStabilityReport
		);

		protected abstract void OnMovementHit
		(
			Collider hitCollider,
			Vector3 hitNormal,
			Vector3 hitPoint,
			ref HitStabilityReport hitStabilityReport
		);

		protected abstract void OnProcessHitStabilityReport
		(
			Collider hitCollider,
			Vector3 hitNormal,
			Vector3 hitPoint,
			Vector3 atCharacterPosition,
			Quaternion atCharacterRotation,
			ref HitStabilityReport
				hitStabilityReport
		);

		protected abstract void OnDiscreteCollisionDetected(Collider hitCollider);

		protected abstract void OnDisable();

		protected abstract void OnExit();

		#endregion

		protected void AddMonoBehaviourTypeToIgnoreCollisionsWith<TMonoBehaviour>()
		where TMonoBehaviour : MonoBehaviour
		{
			var type = typeof(TMonoBehaviour);
			MonoBehaviourTypesToIgnoreCollisionsWith.Add(type);
		}

		protected Coroutine StartCoroutine(IEnumerator routine) =>
			_masterStateMachine.StartCoroutine(routine);

		protected void StopCoroutine(Coroutine routine) =>
			_masterStateMachine.StopCoroutine(routine);

		protected bool TryGetComponent<T>(out T component)
		where T : Component
		{
			if (_masterStateMachine.TryGetComponent<T>(out var c))
			{
				component = c;
				return true;
			}

			component = null;
			return false;
		}

		protected void SetStartState(IKCCState state) =>
			_startState = state;

		protected void RegisterState<T>(out T outState)
		where T : class, IKCCState, new() =>
			outState = CreateState<T>();

		private T CreateState<T>()
		where T : class, IKCCState, new()
		{
			var newState = new T();
			newState.Init(_masterStateMachine, this, _motor);
			return newState;
		}

		private void TickTransitions()
		{
			for (var i = 0; i < _transitions.Count; i++)
				_transitions[i].Tick();
		}

		private IKCCState _startState = new NullKCCState();
		private IKCCState _currentState = new NullKCCState();
		private IKCCState _lastState = new NullKCCState();
		private IKCCStateMachine _owningStateMachine;
		private BaseKCCStateMachine _masterStateMachine;
		private KinematicCharacterMotor _motor;

		private readonly List<KCCTransition> _transitions = new(4);
	}
}