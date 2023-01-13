using Drawing;
using UnityEngine;
using UnityEngine.Assertions;
using System.Collections.Generic;
using KinematicCharacterController;

namespace Bezoro.StateMachines
{
	public abstract class BaseKCCStateMachine<TBlackBoard> :
		BaseKCCStateMachine
	where TBlackBoard : BlackBoard, new()
	{
		public override void Init
		(
			MonoBehaviour owner,
			KinematicCharacterMotor motor
		)
		{
			BlackBoard = new TBlackBoard();
			base.Init(owner, motor);
		}
	}

	public abstract class BaseKCCStateMachine
	{
		public MonoBehaviour Owner { get; private set; }

		public KinematicCharacterMotor Motor { get; private set; }

		public IKCCState LastState { get; private set; }

		public IKCCState CurrentState { get; private set; } =
			new NullKCCState();

		public BlackBoard BlackBoard { get; protected set; }

		public virtual void Init
		(
			MonoBehaviour owner,
			KinematicCharacterMotor motor
		)
		{
			Owner = owner;
			Motor = motor;
			Assert.IsNotNull(Motor);

			SetUp();
			Assert.IsNotNull(CurrentState);
		}

		public void Start() =>
			CurrentState.Enter();

		public void Tick()
		{
			CurrentState.Tick();

			#region Debug

		#if UNITY_EDITOR
			if (Application.isPlaying)
				DrawDebugInfo();

			void DrawDebugInfo()
			{
				DrawStateLabels();

				void DrawStateLabels()
				{
					var states = GetCurrentHierarchy();

					for (var i = states.Count - 1; i >= 0; i--)
					{
						var renderer = Owner.GetComponentInChildren<Renderer>();
						if (renderer is null)
							return;

						var bounds = renderer.bounds;

						var positionToDrawLabel = new Vector3
							(
							 0f,
							 bounds.size.y + i * 0.5f,
							 0f
							);

						states.Reverse();

						using (Draw.InLocalSpace(Owner.transform))
						{
							Draw.Label2D
								(
								 positionToDrawLabel,
								 $"{states[i].GetType().Name}",
								 24f,
								 LabelAlignment.Center,
								 Color.blue
								);
						}

						states.Reverse();
					}
				}
			}
		#endif

			#endregion
		}

		public void FixedTick() =>
			CurrentState.FixedTick();

		public void LateTick() =>
			CurrentState.LateTick();

		#region KCC Methods

		public void UpdateRotation(ref Quaternion currentRotation, float deltaTime) =>
			CurrentState.UpdateRotation(ref currentRotation, deltaTime);

		public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime) =>
			CurrentState.UpdateVelocity(ref currentVelocity, deltaTime);

		public void BeforeCharacterUpdate(float deltaTime) =>
			CurrentState.BeforeCharacterUpdate(deltaTime);

		public void PostGroundingUpdate(float deltaTime) =>
			CurrentState.PostGroundingUpdate(deltaTime);

		public void AfterCharacterUpdate(float deltaTime) =>
			CurrentState.AfterCharacterUpdate(deltaTime);

		public bool IsColliderValidForCollisions(Collider coll)
		{
			return !CheckForMonoBehaviourToIgnoreCollisionsWith();

			bool CheckForMonoBehaviourToIgnoreCollisionsWith()
			{
				if (CheckInState(CurrentState))
					return true;

				if (!IsSuperState(CurrentState, out var rootSuperState))
					return false;

				var parentState = rootSuperState;

				while (true)
				{
					var currentState = parentState.CurrentState;

					if (CheckInState(currentState))
						return true;

					if (!IsSuperState(currentState, out var superState))
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
			CurrentState.GroundHit
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
			CurrentState.MovementHit
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
			CurrentState.ProcessHitStabilityReport
				(
				 hitCollider,
				 hitNormal,
				 hitPoint,
				 atCharacterPosition,
				 atCharacterRotation,
				 ref hitStabilityReport
				);

		public void OnDiscreteCollisionDetected(Collider hitCollider) =>
			CurrentState.DiscreteCollisionDetected(hitCollider);

		#endregion

		public void SwitchCurrentState(IKCCState stateToSwitchTo)
		{
			LastState = CurrentState;

			CurrentState.Exit();
			CurrentState = stateToSwitchTo;
			CurrentState.Enter();
		}

		public bool TryFindStateInCurrentHierarchy<TState>(out IKCCState outState)
		where TState : IKCCState
		{
			if (CurrentState is TState)
			{
				outState = CurrentState;
				return true;
			}

			if (!IsSuperState(CurrentState, out var rootSuperState))
			{
				outState = null;
				return false;
			}

			var parentState = rootSuperState;

			while (true)
			{
				var currentState = parentState.CurrentState;

				if (currentState is TState)
				{
					outState = currentState;
					return true;
				}

				if (!IsSuperState(currentState, out var superState))
				{
					outState = null;
					return false;
				}

				parentState = superState;
			}
		}

		public bool TryFindStateInLastHierarchy<TState>(out IKCCState outState)
		where TState : IKCCState
		{
			if (LastState is TState)
			{
				outState = LastState;
				return true;
			}

			if (!IsSuperState(LastState, out var rootSuperState))
			{
				outState = null;
				return false;
			}

			var parentState = rootSuperState;

			while (true)
			{
				var lastState = parentState.LastState;

				if (lastState is TState)
				{
					outState = lastState;
					return true;
				}

				if (!IsSuperState(lastState, out var superState))
				{
					outState = null;
					return false;
				}

				parentState = superState;
			}
		}

		public List<IKCCState> GetCurrentHierarchy()
		{
			var currentStateHierarchy = new List<IKCCState> { CurrentState };

			if (CurrentState is not IKCCSuperState rootSuperState)
				return currentStateHierarchy;

			var parentState = rootSuperState;

			while (true)
			{
				var currentState = parentState.CurrentState;

				currentStateHierarchy.Add(currentState);

				if (currentState is not IKCCSuperState superState)
					return currentStateHierarchy;

				parentState = superState;
			}
		}

		protected void RegisterState<TState>(out TState createdState)
		where TState : class, IKCCState, new()
		{
			var newState = new TState();
			newState.Init(this, null, Motor);
			createdState = newState;

			if (CurrentState is null or NullKCCState)
				CurrentState = newState;
		}

		protected abstract void SetUp();

		private static bool IsSuperState
		(
			IKCCState state,
			out IKCCSuperState outSuperState
		)
		{
			if (state is IKCCSuperState superState)
			{
				outSuperState = superState;
				return true;
			}

			outSuperState = null;
			return false;
		}
	}

	public abstract class BlackBoard { }
}