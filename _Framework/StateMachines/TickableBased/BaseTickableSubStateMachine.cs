using UnityEngine;
using Bezoro.Core.Helpers;

namespace Bezoro.StateMachines
{
	public abstract class BaseTickableSubStateMachine :
		ITickableStateMachine, ITickableState
	{
		public void Init(TickableStateMachineOwner owner, ITickableStateMachine owningStateMachine)
		{
			_owner = owner;
			_owningStateMachine = owningStateMachine;
			RegisterStates();
			SetUpTransitions();
			OnInit();
		}

		public MonoBehaviour Owner => _owner;

		public ITickableState CurrentState => _currentState;

		public ITickableState LastState => _lastState;

		public TickTransition[] Transitions => _transitions;

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

		public void OnTransitionRaised(ITickableState toState) =>
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

		public TickTransition TransitionsTo(ITickableState toState)
		{
			var newTransition = new TickTransition();
			newTransition.Init(this, toState, _owner);
			ArrayHelpers.Add(ref _transitions, newTransition);
			return newTransition;
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
		/// Called once in the lifetime of the object, when it is created.
		/// Use it like <b>Awake</b> in a <b>MonoBehaviour</b>.
		/// </summary>
		protected abstract void OnInit();

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
		/// Called when the <b>SubStateMachine</b> is <b>Entered</b>.
		/// Use it to decide which should be the <b>Start</b> <b>State</b>, then return it.
		/// </summary>
		protected abstract ITickableState ResolveStartState();

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
		where T : class, ITickableState, new()
		{
			var newState = new T();
			newState.Init(_owner, this);
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

		private TickableStateMachineOwner _owner;
		private ITickableStateMachine _owningStateMachine;
		private ITickableState _currentState;
		private ITickableState _lastState;
		private TickTransition[] _transitions = new TickTransition[4];
	}
}