using UnityEngine;

namespace Bezoro.Framework.Unity.StateMachines.EventBased
{
	public abstract class BaseEventStateMachine : ScriptableObject,
		IEventStateMachine
	{
		public void Init( EventStateMachineOwner owner )
		{
			_owner = owner;
			RegisterStates();
			SetUpTransitions();
			OnInit();
		}

		public IEventState CurrentState => _currentState;

		public IEventState LastState => _lastState;

		[field: SerializeField]
		public DisableMethod DisableMethod { get; private set; }

		public void Start()
		{
			OnStart();
			_currentState = ResolveStartState();
			_currentState.Enter();
		}

		public void Enable()
		{
			if ( _wasDisabled && DisableMethod is DisableMethod.Restart )
			{
				OnEnabled();
				_currentState.Enable();
				_currentState = ResolveStartState();
				_currentState.Enter();
				_wasDisabled = false;
			}
			else if ( _wasDisabled )
			{
				OnEnabled();
				_currentState.Enable();
				_wasDisabled = false;
			}
		}

		public void DoTransition( IEventState toState )
		{
			_currentState.Exit();
			_lastState    = _currentState;
			_currentState = toState;
			_currentState.Enter();
		}

		public void Exit()
		{
			OnExit();
			_currentState.Exit();
		}

		public void Disable()
		{
			OnDisabled();
			_currentState.Disable();
			_wasDisabled = true;
		}

		public void Destroy()
		{
			OnDestroyed();
			_currentState.Destroy();
		}

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
		protected abstract IEventState ResolveStartState();

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

		protected T RegisterState<T>()
		where T : class, IEventState, new()
		{
			var newState = new T();
			newState.Init( _owner, this );

			return newState;
		}

		private EventStateMachineOwner _owner;
		private IEventState _currentState;
		private IEventState _lastState;
		private bool _wasDisabled;
	}
}
