using System;
using System.Collections;
using System.Collections.Generic;
using KinematicCharacterController;
using UnityEngine;
using UnityEngine.Assertions;

namespace Bezoro.StateMachines
{
	public abstract class BaseKCCSuperState<TAgent, TBlackBoard> :
		BaseKCCSuperState<TAgent>
	where TAgent : MonoBehaviour
	where TBlackBoard : BlackBoard
	{
		public override void Init
		(
			BaseKCCStateMachine stateMachine,
			IKCCSuperState parentState,
			KinematicCharacterMotor motor
		)
		{
			BlackBoard = stateMachine.TryGetBlackBoard<TBlackBoard>();
			Assert.IsNotNull(BlackBoard);

			base.Init(stateMachine, parentState, motor);
		}

		protected TBlackBoard BlackBoard { get; private set; }
	}

	public abstract class BaseKCCSuperState<TAgent> : BaseKCCSuperState
	where TAgent : MonoBehaviour
	{
		public override void Init
		(
			BaseKCCStateMachine stateMachine,
			IKCCSuperState parentState,
			KinematicCharacterMotor motor
		)
		{
			Agent = stateMachine.gameObject.GetComponent<TAgent>();
			base.Init(stateMachine, parentState, motor);
		}

		protected TAgent Agent { get; private set; }
	}

	public abstract class BaseKCCSuperState : IKCCSuperState
	{
		public IKCCState StartingState { get; private set; }

		public IKCCState LastState { get; private set; }

		public IKCCState CurrentState { get; private set; }

		public KinematicCharacterMotor Motor { get; private set; }

		public BaseKCCStateMachine StateMachine { get; private set; }

		public IKCCSuperState ParentState { get; private set; }

		public List<Type> MonoBehaviourTypesToIgnoreCollisionsWith { get; } = new(12);

		public virtual void Init
		(
			BaseKCCStateMachine stateMachine,
			IKCCSuperState parentState,
			KinematicCharacterMotor motor
		)
		{
			StateMachine = stateMachine;
			ParentState = parentState;
			Motor = motor;
			SetUp();

			Assert.IsNotNull(StartingState);
			Assert.IsNotNull(CurrentState);
		}

		public KCCTransition TransitionsTo(IKCCState state)
		{
			var transition = new KCCTransition
				(
				 this,
				 state,
				 StateMachine
				);

			_transitions.Add(transition);
			return transition;
		}

		public void Enter()
		{
			CurrentState = StartingState;

			OnEnter();

			foreach (var transition in _transitions)
				transition.BecomeRelevant();

			CurrentState.Enter();
		}

		public void EarlyTick()
		{
			OnEarlyTick();
			CurrentState.EarlyTick();
		}

		public void Tick()
		{
			OnTick();
			CurrentState.Tick();
		}

		public void FixedTick()
		{
			OnFixedTick();
			CurrentState.FixedTick();
		}

		public void LateTick()
		{
			OnLateTick();
			CurrentState.LateTick();
		}

		#region KCC Methods

		public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
		{
			OnUpdateRotation(ref currentRotation, deltaTime);
			CurrentState.UpdateRotation(ref currentRotation, deltaTime);
		}

		public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
		{
			OnUpdateVelocity(ref currentVelocity, deltaTime);
			CurrentState.UpdateVelocity(ref currentVelocity, deltaTime);
		}

		public void BeforeCharacterUpdate(float deltaTime)
		{
			OnBeforeCharacterUpdate(deltaTime);
			CurrentState.BeforeCharacterUpdate(deltaTime);
		}

		public void PostGroundingUpdate(float deltaTime)
		{
			OnPostGroundingUpdate(deltaTime);
			CurrentState.PostGroundingUpdate(deltaTime);
		}

		public void AfterCharacterUpdate(float deltaTime)
		{
			OnAfterCharacterUpdate(deltaTime);
			CurrentState.AfterCharacterUpdate(deltaTime);
			TryTransition();
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

			CurrentState.GroundHit
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

			CurrentState.MovementHit
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

			CurrentState.ProcessHitStabilityReport
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
			CurrentState.DiscreteCollisionDetected(hitCollider);
		}

		#endregion

		public void Exit()
		{
			CurrentState.Exit();

			foreach (var transition in _transitions)
				transition.CeaseRelevant();

			OnExit();
		}

		public void SwitchParentCurrentState(IKCCState toState)
		{
			if (ParentState is null)
			{
				StateMachine.SwitchCurrentState(toState);
				return;
			}

			ParentState.SwitchCurrentState(toState);
		}

		public void SwitchCurrentState(IKCCState stateToSwitchTo)
		{
			LastState = CurrentState;

			CurrentState.Exit();
			CurrentState = stateToSwitchTo;
			CurrentState.Enter();
		}

		protected void RegisterState<TState>(out TState createdState)
		where TState : class, IKCCState, new()
		{
			var newState = new TState();
			newState.Init(StateMachine, this, Motor);
			createdState = newState;

			StartingState ??= newState;
			CurrentState ??= StartingState;
		}

		protected void AddMonoBehaviourTypeToIgnoreCollisionsWith<TMonoBehaviour>()
		where TMonoBehaviour : MonoBehaviour
		{
			var type = typeof(TMonoBehaviour);
			MonoBehaviourTypesToIgnoreCollisionsWith.Add(type);
		}

		protected Coroutine StartCoroutine(IEnumerator routine) =>
			StateMachine.StartCoroutine(routine);

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

		protected abstract void OnExit();

		#endregion

		private void TryTransition()
		{
			for (var i = 0; i < _transitions.Count; i++)
			{
				var transition = _transitions[i];
				if (!transition.CheckConditions())
					continue;

				SwitchParentCurrentState(transition.ToState);
				return;
			}
		}

		private readonly List<KCCTransition> _transitions = new(4);
	}
}