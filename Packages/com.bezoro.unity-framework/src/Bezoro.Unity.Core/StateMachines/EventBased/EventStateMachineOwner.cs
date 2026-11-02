using Bezoro.Unity.Core.StateMachines.Utilities;
using UnityEngine;

namespace Bezoro.Unity.Core.StateMachines.EventBased
{
	[DisallowMultipleComponent]
	/// <summary>
	/// Runs an event-based state machine from a MonoBehaviour owner.
	/// </summary>
	public sealed class EventStateMachineOwner : MonoBehaviour
	{
		private readonly StateMachineOwnerModel _model = new();

		#region Serialized Fields

		[SerializeField] private BaseEventStateMachine stateMachine = null!;

		#endregion

		public BaseEventStateMachine StateMachine => stateMachine;

		#region Unity Lifecycle

		private void Awake()
		{
			var source = stateMachine;
			if (!_model.HasRequiredStateMachine(source != null))
			{
				Debug.LogError($"{nameof(EventStateMachineOwner)} requires a state machine.", this);
				enabled = !_model.ShouldDisableOwner(false);
				return;
			}
			if (source == null) return;

			stateMachine = CreateRuntimeStateMachine(source);
			stateMachine.Init(
				this
			);
		}

		private void Start()
		{
			var machine = stateMachine;
			if (!_model.ShouldRunStateMachineAction(machine != null)) return;
			if (machine == null) return;

			machine.Start();
		}

		private void OnEnable()
		{
			var machine = stateMachine;
			if (!_model.ShouldRunStateMachineAction(machine != null)) return;
			if (machine == null) return;

			machine.Enable();
		}

		private void OnDisable()
		{
			var machine = stateMachine;
			if (!_model.ShouldRunStateMachineAction(machine != null)) return;
			if (machine == null) return;

			machine.Disable();
		}

		private void OnDestroy()
		{
			var machine = stateMachine;
			if (!_model.ShouldRunStateMachineAction(machine != null)) return;
			if (machine == null) return;

			machine.Destroy();
			Destroy(machine);
		}

		#endregion

		public bool TryFindStateInCurrentHierarchy<T>(out T outState)
			where T : class, IState
		{
			if (!_model.ShouldFindState(stateMachine != null))
			{
				outState = null!;
				return false;
			}

			bool boolean = StateMachine.TryFindStateInCurrentHierarchy(
				out T state
			);

			outState = state;

			return boolean;
		}

		public bool TryFindStateInLastHierarchy<T>(out T outState)
			where T : class, IState
		{
			if (!_model.ShouldFindState(stateMachine != null))
			{
				outState = null!;
				return false;
			}

			bool boolean = StateMachine.TryFindStateInLastHierarchy(
				out T state
			);

			outState = state;

			return boolean;
		}

		public void SetStateMachine(BaseEventStateMachine stateMachine)
		{
			var source = stateMachine;
			if (!_model.HasRequiredStateMachine(source != null))
			{
				Debug.LogError($"{nameof(EventStateMachineOwner)} cannot set a null state machine.", this);
				return;
			}
			if (source == null) return;

			var existingStateMachine = this.stateMachine;
			if (_model.ShouldReplaceExistingStateMachine(existingStateMachine != null) && existingStateMachine != null)
			{
				existingStateMachine.Exit();
				existingStateMachine.Destroy();
				Destroy(existingStateMachine);
			}

			this.stateMachine = CreateRuntimeStateMachine(source);
			this.stateMachine.Init(
				this
			);

			this.stateMachine.Start();
		}

		private static BaseEventStateMachine CreateRuntimeStateMachine(BaseEventStateMachine source)
			=> Instantiate(source);
	}
}

namespace Bezoro.Unity.Core.StateMachines.Utilities
{
	public sealed class StateMachineOwnerModel
	{
		public bool HasRequiredStateMachine(bool hasStateMachine)
		{
			return hasStateMachine;
		}

		public bool ShouldDisableOwner(bool hasRequiredStateMachine)
		{
			return !hasRequiredStateMachine;
		}

		public bool ShouldRunStateMachineAction(bool hasStateMachine)
		{
			return hasStateMachine;
		}

		public bool ShouldFindState(bool hasStateMachine)
		{
			return hasStateMachine;
		}

		public bool ShouldReplaceExistingStateMachine(bool hasStateMachine)
		{
			return hasStateMachine;
		}

		public bool ShouldRegisterTickable(bool isTickRegistered)
		{
			return !isTickRegistered;
		}

		public bool ShouldUnregisterTickable(bool isTickRegistered)
		{
			return isTickRegistered;
		}
	}
}
