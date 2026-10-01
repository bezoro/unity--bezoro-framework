using UnityEngine;

namespace Bezoro.Unity.Core.StateMachines.EventBased
{
	/// <summary>
	///     Manages event-based states, transitions, and nested sub-state machines.
	/// </summary>
	public abstract class BaseEventStateMachine : ScriptableObject,
		IEventStateMachine
	{
		#region Serialized Fields

		[field: SerializeField] public DisableMethod DisableMethod { get; private set; }

		#endregion

		private bool _hasStarted;
		private bool _wasDisabled;

		private EventStateMachineOwner _owner = null!;
		public void Init(EventStateMachineOwner owner)
		{
			_owner = owner;
			RegisterStates();
			SetUpTransitions();
			OnInit();
		}

		public IEventState CurrentState { get; private set; } = null!;
		public IEventState LastState { get; private set; } = null!;
		public void Destroy()
		{
			OnDestroyed();
			if (!_hasStarted) return;

			CurrentState.Destroy();
		}

		public void Disable()
		{
			OnDisabled();
			if (!_hasStarted) return;

			CurrentState.Disable();
			_wasDisabled = true;
		}

		#region Interface Implementations

		public void DoTransition(IEventState toState)
		{
			CurrentState.Exit();
			LastState    = CurrentState;
			CurrentState = toState;
			CurrentState.Enter();
		}

		#endregion

		public void Enable()
		{
			if (!_wasDisabled) return;
			if (!_hasStarted) return;

			OnEnabled();

			if (_wasDisabled && DisableMethod is DisableMethod.Restart)
			{
				CurrentState.Exit();
				LastState    = CurrentState;
				CurrentState = ResolveStartState();
				CurrentState.Enter();
			}
			else
			{
				CurrentState.Enable();
			}

			_wasDisabled = false;
		}

		public void Exit()
		{
			OnExit();
			if (!_hasStarted) return;

			CurrentState.Exit();
		}

		public void Start()
		{
			if (_hasStarted) return;

			OnStart();
			CurrentState = ResolveStartState();
			CurrentState.Enter();
			_hasStarted = true;
		}

		protected T RegisterState<T>()
			where T : class, IEventState, new()
		{
			var newState = new T();

			newState.Init(
				_owner,
				this
			);

			return newState;
		}

		#region Abstract Methods

		/// <summary>
		///     Called when the <b>Owner</b> is <i>Destroyed</i>, while this object is still being executed.
		/// </summary>
		protected abstract void OnDestroyed();

		/// <summary>
		///     Called when the <b>Owner</b> gets <i>Disabled</i>, while this object is still being executed.
		/// </summary>
		protected abstract void OnDisabled();

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
		///     Called once in the lifetime of the object, when it is created.
		///     Use it like <b>Awake</b> in a <b>MonoBehaviour</b>.
		/// </summary>
		protected abstract void OnInit();

		/// <summary>
		///     Called once in the lifetime of the object when it is <i>Started</i>.
		/// </summary>
		protected abstract void OnStart();

		/// <summary>
		///     Called once in the lifetime of the object, in <b>Awake</b>.
		///     Use it to register all <b>States</b> needed for the <b>StateMachine</b> to work.
		/// </summary>
		protected abstract void RegisterStates();

		/// <summary>
		///     If <b>DisableMethod</b> is <b>Continue</b>:
		///     <para>Called once in the lifetime of the object, in <b>Start</b>.</para>
		///     If <b>DisableMethod</b> is <b>Restart</b>:
		///     <para>Called once in <b>Start</b> and on <b>Enable</b>.</para>
		///     <para></para>
		///     Use it to decide which should be the <b>Start</b> <b>State</b>, then return it.
		/// </summary>
		protected abstract IEventState ResolveStartState();

		/// <summary>
		///     Called once in the lifetime of the object, in <b>Awake</b>.
		///     Use it to set up all of the <b>States</b> <b>Transitions</b>.
		/// </summary>
		protected abstract void SetUpTransitions();

		#endregion
	}
}
