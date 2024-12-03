using KCC;
using UnityEngine;

namespace Bezoro.Framework.Unity.StateMachines.KCCBased
{
	public abstract class BaseKCCStateMachine : ScriptableObject,
		IKCCStateMachine
	{
		[field: SerializeField] public DisableMethod DisableMethod { get; private set; }

		private KCCStateMachineOwner _owner;
		private bool                 _wasDisabled;

		public IKCCState CurrentState { get; private set; }

		public IKCCState LastState { get; private set; }

		public void DoTransition( IKCCState toState )
		{
			CurrentState.Exit();
			LastState    = CurrentState;
			CurrentState = toState;
			CurrentState.Enter();
		}

		public void Init( KCCStateMachineOwner owner )
		{
			_owner = owner;
			RegisterStates();
			SetUpTransitions();
			OnInit();
		}

		public void Start()
		{
			OnStart();
			CurrentState = ResolveStartState();
			CurrentState.Enter();
		}

		public void Enable()
		{
			if ( _wasDisabled && DisableMethod is DisableMethod.Restart )
			{
				OnEnabled();
				CurrentState.Enable();
				CurrentState = ResolveStartState();
				CurrentState.Enter();
				_wasDisabled = false;
			}
			else if ( _wasDisabled )
			{
				OnEnabled();
				CurrentState.Enable();
				_wasDisabled = false;
			}
		}

		public void EarlyTick()
		{
			CurrentState.EarlyTick();
		}

		public void Tick()
		{
			CurrentState.Tick();
		}

		public void FixedTick()
		{
			CurrentState.FixedTick();
		}

		public void LateTick()
		{
			CurrentState.LateTick();
		}

		public void Exit()
		{
			OnExit();
			CurrentState.Exit();
		}

		public void Disable()
		{
			OnDisabled();
			CurrentState.Disable();
			_wasDisabled = true;
		}

		public void Destroy()
		{
			OnDestroyed();
			CurrentState.Destroy();
		}

		protected T RegisterState<T>()
		where T : class, IKCCState, new()
		{
			var newState = new T();
			newState.Init(
				_owner,
				this,
				_owner.Motor );

			return newState;
		}

	#region KCC Methods

		public void UpdateRotation( ref Quaternion currentRotation, float deltaTime )
		{
			CurrentState.UpdateRotation(
				ref currentRotation,
				deltaTime );
		}

		public void UpdateVelocity( ref Vector3 currentVelocity, float deltaTime )
		{
			CurrentState.UpdateVelocity(
				ref currentVelocity,
				deltaTime );
		}

		public void BeforeCharacterUpdate( float deltaTime )
		{
			CurrentState.BeforeCharacterUpdate( deltaTime );
		}

		public void PostGroundingUpdate( float deltaTime )
		{
			CurrentState.PostGroundingUpdate( deltaTime );
		}

		public void AfterCharacterUpdate( float deltaTime )
		{
			CurrentState.AfterCharacterUpdate( deltaTime );
		}

		public bool IsColliderValidForCollisions( Collider coll )
		{
			return !CheckForMonoBehaviourToIgnoreCollisionsWith();

			bool CheckForMonoBehaviourToIgnoreCollisionsWith()
			{
				if ( CheckInState( CurrentState ) )
				{
					return true;
				}

				if ( !CurrentState.IsSubStateMachine( out var rootSubStateMachine ) )
				{
					return false;
				}

				var currentState = rootSubStateMachine.CurrentState;

				while ( true )
				{
					if ( CheckInState( currentState ) )
					{
						return true;
					}

					if ( !currentState.IsSubStateMachine( out var subStateMachine ) )
					{
						return false;
					}

					currentState = subStateMachine.CurrentState;
				}

				bool CheckInState( IKCCState state )
				{
					foreach ( var monoBehaviour in
							 state.MonoBehaviourTypesToIgnoreCollisionsWith )
					{
						if ( coll.TryGetComponent(
								monoBehaviour,
								out _ ) )
						{
							return true;
						}
					}

					return false;
				}
			}
		}

		public void OnGroundHit(
			Collider hitCollider,
			Vector3 hitNormal,
			Vector3 hitPoint,
			ref HitStabilityReport hitStabilityReport
		)
		{
			CurrentState.GroundHit(
				hitCollider,
				hitNormal,
				hitPoint,
				ref hitStabilityReport );
		}

		public void OnMovementHit(
			Collider hitCollider,
			Vector3 hitNormal,
			Vector3 hitPoint,
			ref HitStabilityReport hitStabilityReport
		)
		{
			CurrentState.MovementHit(
				hitCollider,
				hitNormal,
				hitPoint,
				ref hitStabilityReport );
		}

		public void ProcessHitStabilityReport(
			Collider hitCollider,
			Vector3 hitNormal,
			Vector3 hitPoint,
			Vector3 atCharacterPosition,
			Quaternion atCharacterRotation,
			ref HitStabilityReport hitStabilityReport
		)
		{
			CurrentState.ProcessHitStabilityReport(
				hitCollider,
				hitNormal,
				hitPoint,
				atCharacterPosition,
				atCharacterRotation,
				ref hitStabilityReport
			);
		}

		public void OnDiscreteCollisionDetected( Collider hitCollider )
		{
			CurrentState.DiscreteCollisionDetected( hitCollider );
		}

	#endregion

	#region Abstract Methods

		/// <summary>
		///     Called once in the lifetime of the object, in <b>Awake</b>.
		///     Use it to register all <b>States</b> needed for the <b>StateMachine</b> to work.
		/// </summary>
		protected abstract void RegisterStates();

		/// <summary>
		///     Called once in the lifetime of the object, in <b>Awake</b>.
		///     Use it to set up all of the <b>States</b> <b>Transitions</b>.
		/// </summary>
		protected abstract void SetUpTransitions();

		/// <summary>
		///     Called once in the lifetime of the object, when it is created.
		///     Use it like <b>Awake</b> in a <b>MonoBehaviour</b>.
		/// </summary>
		protected abstract void OnInit();

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
		///     Called once in the lifetime of the object when it is <i>Started</i>.
		/// </summary>
		protected abstract void OnStart();

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
		///     Called when the <b>Owner</b> gets <i>Disabled</i>, while this object is still being executed.
		/// </summary>
		protected abstract void OnDisabled();

		/// <summary>
		///     Called when the <b>Owner</b> is <i>Destroyed</i>, while this object is still being executed.
		/// </summary>
		protected abstract void OnDestroyed();

	#endregion
	}
}
