using System;
using UnityEngine;
using Bezoro.Core.Helpers;
using System.Collections.Generic;
using KinematicCharacterController;

namespace Bezoro.StateMachines
{
	public abstract class BaseKCCSubStateMachine :
		IKCCSubStateMachine
	{
		public void Init
		(MonoBehaviour masterStateMachine,
		 IKCCStateMachine owningStateMachine,
		 KinematicCharacterMotor motor)
		{
			_masterStateMachine = masterStateMachine;
			_owningStateMachine = owningStateMachine;
			_motor = motor;
			SetUp();
		}

		void ITickableState.Init
		(MonoBehaviour masterStateMachine,
		 ITickableStateMachine owningStateMachine) =>
			Init(masterStateMachine, owningStateMachine as IKCCStateMachine, null);

		void IState.Init
		(MonoBehaviour masterStateMachine,
		 IStateMachine owningStateMachine) =>
			Init(masterStateMachine, owningStateMachine as IKCCStateMachine, null);

		public MonoBehaviour MasterStateMachine => _masterStateMachine;

		public IKCCState StartState => _startState;

		ITickableState ITickableStateMachine.StartState => _startState;

		IState IStateMachine.StartState => _startState;

		public IKCCState CurrentState => _currentState;

		ITickableState ITickableStateMachine.CurrentState => _currentState;

		IState IStateMachine.CurrentState => _currentState;

		public IKCCState LastState => _lastState;

		ITickableState ITickableStateMachine.LastState => _lastState;

		IState IStateMachine.LastState => _lastState;

		public IKCCTransition[] Transitions => _transitions;

		ITickTransition[] ITickableState.Transitions => _transitions;

		ITransition[] IState.Transitions => _transitions;

		public KinematicCharacterMotor Motor => _motor;

		public List<Type> MonoBehaviourTypesToIgnoreCollisionsWith { get; } = new(12);

		public IKCCTransition TransitionsTo(IKCCState toState)
		{
			var newTransition = this.CreateTransition<KCCTransition>(toState);
			ArrayHelpers.Add(ref _transitions, newTransition);
			return newTransition;
		}

		public void Enter()
		{
			_currentState = _startState;
			OnEnter();
			this.AllTransitionsBecomeRelevant();
			_currentState.Enter();
		}

		public void Disable()
		{
			OnDisable();
			this.Disable_Ext();
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
			this.AllTransitionsTick();
		}

		public void GroundHit
		(Collider hitCollider,
		 Vector3 hitNormal,
		 Vector3 hitPoint,
		 ref HitStabilityReport hitStabilityReport)
		{
			OnGroundHit(hitCollider,
					    hitNormal,
					    hitPoint,
					    ref hitStabilityReport);

			_currentState.GroundHit(hitCollider,
								    hitNormal,
								    hitPoint,
								    ref hitStabilityReport);
		}

		public void MovementHit
		(Collider hitCollider,
		 Vector3 hitNormal,
		 Vector3 hitPoint,
		 ref HitStabilityReport hitStabilityReport)
		{
			OnMovementHit(hitCollider,
						  hitNormal,
						  hitPoint,
						  ref hitStabilityReport);

			_currentState.MovementHit(hitCollider,
									  hitNormal,
									  hitPoint,
									  ref hitStabilityReport);
		}

		public void ProcessHitStabilityReport
		(Collider hitCollider,
		 Vector3 hitNormal,
		 Vector3 hitPoint,
		 Vector3 atCharacterPosition,
		 Quaternion atCharacterRotation,
		 ref HitStabilityReport hitStabilityReport)
		{
			OnProcessHitStabilityReport(hitCollider,
									    hitNormal,
									    hitPoint,
									    atCharacterPosition,
									    atCharacterRotation,
									    ref hitStabilityReport);

			_currentState.ProcessHitStabilityReport(hitCollider,
												    hitNormal,
												    hitPoint,
												    atCharacterPosition,
												    atCharacterRotation,
												    ref hitStabilityReport);
		}

		public void DiscreteCollisionDetected(Collider hitCollider)
		{
			OnDiscreteCollisionDetected(hitCollider);
			_currentState.DiscreteCollisionDetected(hitCollider);
		}

		#endregion

		public void OnTransitionTrue(ITickableState toState) =>
			_owningStateMachine.DoTransition(toState);


		public void Exit()
		{
			_currentState.Exit();
			this.AllTransitionsCeaseRelevant();
			OnExit();
		}

		public void DoTransition(IKCCState toState) =>
			this.DoTransition_Ext(toState);

		void ITickableStateMachine.DoTransition(ITickableState toState) =>
			this.DoTransition_Ext(toState);

		void IStateMachine.DoTransition(IState toState) =>
			this.DoTransition_Ext(toState);

		public void SetLastState(IKCCState state) =>
			_lastState = state;

		void ITickableStateMachine.SetLastState(ITickableState state) =>
			_lastState = state as IKCCState;

		void IStateMachine.SetLastState(IState state) =>
			_lastState = state as IKCCState;

		public void SetCurrentState(IKCCState state) =>
			_currentState = state;

		void ITickableStateMachine.SetCurrentState(ITickableState state) =>
			_currentState = state as IKCCState;

		void IStateMachine.SetCurrentState(IState state) =>
			_currentState = state as IKCCState;

		#region Abstract Methods

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
		(Collider hitCollider,
		 Vector3 hitNormal,
		 Vector3 hitPoint,
		 ref HitStabilityReport hitStabilityReport);

		protected abstract void OnMovementHit
		(Collider hitCollider,
		 Vector3 hitNormal,
		 Vector3 hitPoint,
		 ref HitStabilityReport hitStabilityReport);

		protected abstract void OnProcessHitStabilityReport
		(Collider hitCollider,
		 Vector3 hitNormal,
		 Vector3 hitPoint,
		 Vector3 atCharacterPosition,
		 Quaternion atCharacterRotation,
		 ref HitStabilityReport
			 hitStabilityReport);

		protected abstract void OnDiscreteCollisionDetected(Collider hitCollider);

		protected abstract void OnDisable();

		protected abstract void OnExit();

		#endregion

		protected void SetStartState(IKCCState state) =>
			_startState = state;

		protected void RegisterState<T>(out T outState)
		where T : class, IKCCState, new() =>
			outState = this.CreateState<T>(_masterStateMachine);

		private IKCCState _startState = new NullKCCState();
		private IKCCState _currentState = new NullKCCState();
		private IKCCState _lastState = new NullKCCState();
		private IKCCStateMachine _owningStateMachine;
		private MonoBehaviour _masterStateMachine;
		private KinematicCharacterMotor _motor;
		private IKCCTransition[] _transitions = new IKCCTransition[4];
	}
}