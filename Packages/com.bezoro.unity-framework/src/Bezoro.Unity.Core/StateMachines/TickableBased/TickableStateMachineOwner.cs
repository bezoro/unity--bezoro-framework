using Bezoro.Unity.Core.StateMachines.Utilities;
using Bezoro.Unity.Core.Tickable;
using UnityEngine;

namespace Bezoro.Unity.Core.StateMachines.TickableBased
{
	[DisallowMultipleComponent]
	/// <summary>
	/// Runs a tickable state machine from a MonoBehaviour owner.
	/// </summary>
	public sealed class TickableStateMachineOwner : MonoBehaviour,
		IEarlyTickable,
		IRegularTickable,
		IFixedTickable,
		ILateTickable
	{
		private readonly StateMachineOwnerModel _model = new();

		#region Serialized Fields

		[SerializeField] private BaseTickableStateMachine stateMachine = null!;

		#endregion

		private bool _isTickRegistered;

		public BaseTickableStateMachine StateMachine => stateMachine;

		#region Unity Lifecycle

		private void Awake()
		{
			var source = stateMachine;
			if (!_model.HasRequiredStateMachine(source != null))
			{
				Debug.LogError($"{nameof(TickableStateMachineOwner)} requires a state machine.", this);
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
			RegisterTickable();
			var machine = stateMachine;
			if (_model.ShouldRunStateMachineAction(machine != null) && machine != null)
				machine.Enable();
		}

		private void OnDisable()
		{
			UnregisterTickable();
			var machine = stateMachine;
			if (_model.ShouldRunStateMachineAction(machine != null) && machine != null)
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

		#region Tickable

		public void EarlyTick()
		{
			var machine = stateMachine;
			if (_model.ShouldRunStateMachineAction(machine != null) && machine != null)
				machine.EarlyTick();
		}

		public void FixedTick()
		{
			var machine = stateMachine;
			if (_model.ShouldRunStateMachineAction(machine != null) && machine != null)
				machine.FixedTick();
		}

		public void LateTick()
		{
			var machine = stateMachine;
			if (_model.ShouldRunStateMachineAction(machine != null) && machine != null)
				machine.LateTick();
		}

		public void RegularTick()
		{
			var machine = stateMachine;
			if (_model.ShouldRunStateMachineAction(machine != null) && machine != null)
				machine.Tick();
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

		public void SetStateMachine(BaseTickableStateMachine stateMachine)
		{
			var source = stateMachine;
			if (!_model.HasRequiredStateMachine(source != null))
			{
				Debug.LogError($"{nameof(TickableStateMachineOwner)} cannot set a null state machine.", this);
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

		private static BaseTickableStateMachine CreateRuntimeStateMachine(BaseTickableStateMachine source)
			=> Instantiate(source);

		private void RegisterTickable()
		{
			if (!_model.ShouldRegisterTickable(_isTickRegistered)) return;

			_isTickRegistered = TickableRegistration.Register(this);
		}

		private void UnregisterTickable()
		{
			if (!_model.ShouldUnregisterTickable(_isTickRegistered)) return;

			TickableRegistration.Unregister(this);
			_isTickRegistered = false;
		}
	}
}
