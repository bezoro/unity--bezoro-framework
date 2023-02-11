using System;
using UnityEngine;
using Bezoro.Core.Helpers;
using System.Collections.Generic;
using KinematicCharacterController;

namespace Bezoro.StateMachines
{
	public abstract class BaseKCCSubStateMachine :
		IKCCStateMachine, IKCCState
	{
		public void Init
		(KCCStateMachineOwner owner,
		 IKCCStateMachine owningStateMachine,
		 KinematicCharacterMotor motor)
		{
			_owner = owner;
			_owningStateMachine = owningStateMachine;
			_motor = motor;
			RegisterStates();
			SetUpTransitions();
			OnInit();
		}

		public MonoBehaviour Owner => _owner;

		public IKCCState CurrentState => _currentState;

		public IKCCState LastState => _lastState;

		public KCCTransition[] Transitions => _transitions;

		public KinematicCharacterMotor Motor => _motor;

		public List<Type> MonoBehaviourTypesToIgnoreCollisionsWith { get; } = new(12);

		public void Enable()
		{
			OnEnable();
			AllTransitionsBecomeRelevant();
			_currentState.Enable();
		}

		public void Enter()
		{
			OnEnter();
			AllTransitionsBecomeRelevant();
			_currentState = ResolveStartState();
			_currentState.Enter();
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
			AllTransitionsTick();

			void AllTransitionsTick()
			{
				for (var i = 0; i < _transitions.Length; i++)
				{
					if (_transitions[i] == null)
						continue;

					_transitions[i].Tick();
				}
			}
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

		public void OnTransitionRaised(IKCCState toState) =>
			_owningStateMachine.DoTransition(toState);

		public void Exit()
		{
			OnExit();
			_currentState.Exit();
			AllTransitionsCeaseRelevant();
		}

		public void Disable()
		{
			OnDisable();
			_currentState.Disable();
			AllTransitionsCeaseRelevant();
		}

		public void Destroy() =>
			OnDestroy();

		public KCCTransition TransitionsTo(IKCCState toState)
		{
			var newTransition = new KCCTransition();
			newTransition.Init(this, toState, _owner);
			ArrayHelpers.Add(ref _transitions, newTransition);
			return newTransition;
		}

		public void DoTransition(IKCCState toState)
		{
			_currentState.Exit();
			_lastState = _currentState;
			_currentState = toState;
			_currentState.Enter();
		}

		public bool IsSubStateMachine(out IKCCStateMachine outStateMachine)
		{
			outStateMachine = this;
			return true;
		}

		public void AddMonoBehaviourToIgnoreCollisionsWith<T>()
		where T : MonoBehaviour =>
			KCCStateExtensions.AddMonoBehaviourToIgnoreCollisionsWith<T>(this);

		#region Abstract Methods

		/// <summary>
		/// Called once in the lifetime of the object, when it is created.
		/// Use it to register all <b>States</b> needed for the <b>SubStateMachine</b> to work.
		/// /// Use it like 'Awake' in a MonoBehaviour.
		/// </summary>
		protected abstract void RegisterStates();

		/// <summary>
		/// Called once in the lifetime of the object, when it is created.
		/// Use it to set up all of the <b>States</b> <b>Transitions</b>.
		/// </summary>
		protected abstract void SetUpTransitions();

		/// <summary>
		/// Called once in the lifetime of the object, when it is created.
		/// Use it like <b>Awake</b> in a <b>MonoBehaviour</b>.
		/// </summary>
		protected abstract void OnInit();

		/// <summary>
		/// Called when the <b>SubStateMachine</b> is <b>Entered</b>.
		/// Use it to decide which should be the <b>Start</b> <b>State</b>, then return it.
		/// </summary>
		protected abstract IKCCState ResolveStartState();

		/// <summary>
		/// Called when the <b>Owner</b> is <i>Enabled</i> while this object is still being executed.
		/// </summary>
		protected abstract void OnEnable();

		/// <summary>
		/// Called when the <b>SubStateMachine</b> is <i>Entered</i>.
		/// </summary>
		protected abstract void OnEnter();

		/// <summary>
		/// Called every frame, before <b>OnTick</b>.
		/// </summary>
		protected abstract void OnEarlyTick();

		/// <summary>
		/// Called every frame.
		/// </summary>
		protected abstract void OnTick();

		/// <summary>
		/// Called at the same rate as <b>FixedUpdate</b>.
		/// </summary>
		protected abstract void OnFixedTick();

		/// <summary>
		/// Called every frame after <b>OnTick</b>.
		/// </summary>
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

		/// <summary>
		/// Called when the <b>SubStateMachine</b> is <i>Exited</i>.
		/// </summary>
		protected abstract void OnExit();

		/// <summary>
		/// Called when the <b>Owner</b> is <i>Disabled</i> while this object is still being executed.
		/// </summary>
		protected abstract void OnDisable();

		/// <summary>
		/// Called when the <b>Owner</b> is <i>Destroyed</i> while this object is still being executed.
		/// </summary>
		protected abstract void OnDestroy();

		#endregion

		protected T RegisterState<T>()
		where T : class, IKCCState, new()
		{
			var newState = new T();
			newState.Init(_owner, this, _motor);
			return newState;
		}

		private void AllTransitionsBecomeRelevant()
		{
			for (var i = 0; i < _transitions.Length; i++)
			{
				if (_transitions[i] == null)
					continue;

				_transitions[i].BecomeRelevant();
			}
		}

		private void AllTransitionsCeaseRelevant()
		{
			for (var i = 0; i < _transitions.Length; i++)
			{
				if (_transitions[i] == null)
					continue;

				_transitions[i].CeaseRelevant();
			}
		}

		private KCCStateMachineOwner _owner;
		private IKCCStateMachine _owningStateMachine;
		private IKCCState _currentState;
		private IKCCState _lastState;
		private KinematicCharacterMotor _motor;
		private KCCTransition[] _transitions = new KCCTransition[4];
	}
}