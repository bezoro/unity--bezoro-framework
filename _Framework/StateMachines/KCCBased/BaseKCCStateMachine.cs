using Drawing;
using Bezoro.Core;
using UnityEngine;
using UnityEngine.Assertions;
using System.Collections.Generic;
using KinematicCharacterController;
using UnityEditor;
using Debug = Sisus.Debugging.Debug;

namespace Bezoro.StateMachines
{
	public abstract class BaseKCCStateMachine<TBlackBoard> : BaseKCCStateMachine
	where TBlackBoard : BlackBoard
	{
		[field: SerializeField]
		public BlackBoard BlackBoard { get; private set; }

	#if UNITY_EDITOR
		protected override void OnValidate()
		{
			base.OnValidate();

			if (Application.isPlaying)
				return;

			EditorApplication.delayCall += SetBlackBoard;


			void SetBlackBoard()
			{
				BlackBoard ??= TryGetComponent<TBlackBoard>(out var blackBoard) ?
								   blackBoard :
								   gameObject.AddComponent<TBlackBoard>();
			}
		}
	#endif
	}

	[DisallowMultipleComponent]
	public abstract class BaseKCCStateMachine : Tickable,
		ICharacterController
	{
		[field: SerializeField]
		public KinematicCharacterMotor Motor { get; private set; }

		public IKCCState LastState { get; private set; }

		public IKCCState CurrentState { get; private set; } = new NullKCCState();

	#if UNITY_EDITOR
		protected virtual void OnValidate()
		{
			if (Application.isPlaying)
				return;

			EditorApplication.delayCall += SetMotor;

			void SetMotor()
			{
				Motor ??= TryGetComponent<KinematicCharacterMotor>(out var characterMotor) ?
							  characterMotor :
							  gameObject.AddComponent<KinematicCharacterMotor>();
			}
		}
	#endif

		private void Awake()
		{
			SetUp();
			Assert.IsNotNull(CurrentState);
		}

		private void Start() =>
			CurrentState.Enter();

		public override void EarlyTick() { }

		public override void Tick()
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
						var childRenderer = GetComponentInChildren<Renderer>();
						if (childRenderer is null)
							return;

						var bounds = childRenderer.bounds;

						var positionToDrawLabel = new Vector3
							(
							 0f,
							 bounds.size.y + i * 0.5f,
							 0f
							);

						states.Reverse();

						using (Draw.InLocalSpace(transform))
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

		public override void FixedTick() =>
			CurrentState.FixedTick();

		public override void LateTick() =>
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

		public TBlackBoard TryGetBlackBoard<TBlackBoard>()
		where TBlackBoard : BlackBoard
		{
			if (TryGetComponent<TBlackBoard>(out var bb))
				return bb;

			Debug.LogWarning($"BlackBoard not found in {gameObject.name}", gameObject);
			return null;
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

	[DisallowMultipleComponent]
	public abstract class BlackBoard : MonoBehaviour { }
}