using UnityEngine;

namespace Bezoro.StateMachines
{
	public abstract class BaseTickableStateMachine : ScriptableObject,
		ITickableStateMachine
	{
		public void Init(TickableStateMachineOwner owner)
		{
			_owner = owner;
			RegisterStates();
			SetUpTransitions();
			OnInit();
		}

		public ITickableState CurrentState => _currentState;

		public ITickableState LastState => _lastState;

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
			if (_wasDisabled && DisableMethod is DisableMethod.Restart)
			{
				OnEnabled();
				_currentState.Enable();
				_currentState = ResolveStartState();
				_currentState.Enter();
				_wasDisabled = false;
			}
			else if (_wasDisabled)
			{
				OnEnabled();
				_currentState.Enable();
				_wasDisabled = false;
			}
		}

		public void EarlyTick() =>
			_currentState.EarlyTick();

		public void Tick() =>
			_currentState.Tick();

		public void FixedTick() =>
			_currentState.FixedTick();

		public void LateTick() =>
			_currentState.LateTick();

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

		public void DoTransition(ITickableState toState)
		{
			_currentState.Exit();
			_lastState = _currentState;
			_currentState = toState;
			_currentState.Enter();
		}

		#region Abstract Methods

		/// <summary>
		/// Called once in the lifetime of the object, in <b>Awake</b>.
		/// Use it to register all <b>States</b> needed for the <b>StateMachine</b> to work.
		/// </summary>
		protected abstract void RegisterStates();

		/// <summary>
		/// Called once in the lifetime of the object, in <b>Awake</b>.
		/// Use it to set up all of the <b>States</b> <b>Transitions</b>.
		/// </summary>
		protected abstract void SetUpTransitions();

		/// <summary>
		/// Called once in the lifetime of the object, when it is created.
		/// Use it like <b>Awake</b> in a <b>MonoBehaviour</b>.
		/// </summary>
		protected abstract void OnInit();

		/// <summary>
		/// Called once in the lifetime of the object, in <b>Start</b>.
		/// Use it to decide which should be the <b>Start</b> <b>State</b>, then return it.
		/// </summary>
		protected abstract ITickableState ResolveStartState();

		/// <summary>
		/// Called once in the lifetime of the object when it is <i>Started</i>.
		/// </summary>
		protected abstract void OnStart();

		/// <summary>
		/// Called every time the <b>Owner</b> gets <i>Enabled</i> and this object was previously <i>Disabled</i>, while its being executed.
		/// </summary>
		protected abstract void OnEnabled();

		/// <summary>
		/// Called once when the Owner switches State Machines.
		/// </summary>
		protected abstract void OnExit();

		/// <summary>
		/// Called every time the Owner gets disabled, while its being executed.
		/// </summary>
		protected abstract void OnDisabled();

		/// <summary>
		/// Called when the Owner gets destroyed, while its being executed.
		/// </summary>
		protected abstract void OnDestroyed();

		#endregion

		protected T RegisterState<T>()
		where T : class, ITickableState, new()
		{
			var newState = new T();
			newState.Init(_owner, this);
			return newState;
		}

		private TickableStateMachineOwner _owner;
		private ITickableState _currentState;
		private ITickableState _lastState;
		private bool _wasDisabled;
	}
}