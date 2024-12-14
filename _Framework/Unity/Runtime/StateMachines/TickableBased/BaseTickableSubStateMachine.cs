using Bezoro.Framework.Core;
using UnityEngine;

namespace Bezoro.Framework.Unity.StateMachines.TickableBased
{
	public abstract class BaseTickableSubStateMachine :
		ITickableStateMachine,
		ITickableState
	{
		private TickableStateMachineOwner _owner;
		private ITickableStateMachine     _owningStateMachine;
		private TickTransition[]          _transitions = new TickTransition[4];

		public MonoBehaviour Owner => _owner;

		public void Init(TickableStateMachineOwner owner, ITickableStateMachine owningStateMachine)
		{
			_owner              = owner;
			_owningStateMachine = owningStateMachine;
			RegisterStates();
			SetUpTransitions();
			OnInit();
		}

		public void Enable()
		{
			OnEnable();
			AllTransitionsBecomeRelevant();
			CurrentState.Enable();
		}

		public void Enter()
		{
			OnEnter();
			AllTransitionsBecomeRelevant();
			CurrentState = ResolveStartState();
			CurrentState.Enter();
		}

		public void EarlyTick()
		{
			OnEarlyTick();
			CurrentState.EarlyTick();
		}

		public void Tick()
		{
			OnTick();
			CurrentState.Tick();
		}

		public void FixedTick()
		{
			OnFixedTick();
			CurrentState.FixedTick();
		}

		public void LateTick()
		{
			OnLateTick();
			CurrentState.LateTick();
			AllTransitionsTick();

			void AllTransitionsTick()
			{
				for (var i = 0 ; i < _transitions.Length ; i++)
				{
					if (_transitions[i] == null)
					{
						continue;
					}

					_transitions[i].Tick();
				}
			}
		}

		public void OnTransitionRaised(ITickableState toState)
		{
			_owningStateMachine.DoTransition(toState);
		}

		public void Exit()
		{
			OnExit();
			AllTransitionsCeaseRelevant();
			CurrentState.Exit();
		}

		public void Disable()
		{
			OnDisable();
			AllTransitionsCeaseRelevant();
			CurrentState.Disable();
		}

		public void Destroy()
		{
			OnDestroy();
			AllTransitionsCeaseRelevant();
			CurrentState.Destroy();
		}

		public void DoTransition(ITickableState toState)
		{
			CurrentState.Exit();
			LastState    = CurrentState;
			CurrentState = toState;
			CurrentState.Enter();
		}

		public TickTransition TransitionsTo(ITickableState toState)
		{
			var new_transition = new TickTransition();
			new_transition.Init(this, toState, _owner);
			Array_Helpers.Add(ref _transitions, new_transition);
			return new_transition;
		}

		public TickTransition[] Transitions => _transitions;

		public ITickableState CurrentState { get; private set; }

		public ITickableState LastState { get; private set; }

		private void AllTransitionsBecomeRelevant()
		{
			for (var i = 0 ; i < _transitions.Length ; i++)
			{
				if (_transitions[i] == null)
				{
					continue;
				}

				_transitions[i].BecomeRelevant();
			}
		}

		private void AllTransitionsCeaseRelevant()
		{
			for (var i = 0 ; i < _transitions.Length ; i++)
			{
				if (_transitions[i] == null)
				{
					continue;
				}

				_transitions[i].CeaseRelevant();
			}
		}

		protected T RegisterState<T>()
		where T : class, ITickableState, new()
		{
			var new_state = new T();
			new_state.Init(_owner, this);
			return new_state;
		}

	#region Abstract Methods

		/// <summary>
		///     Called once in the lifetime of the object, when it is created.
		///     Use it to register all <b>States</b> needed for the <b>SubStateMachine</b> to work.
		/// </summary>
		protected abstract void RegisterStates();

		/// <summary>
		///     Called once in the lifetime of the object, when it is created.
		///     Use it to set up all of the <b>States</b> <b>Transitions</b>.
		/// </summary>
		protected abstract void SetUpTransitions();

		/// <summary>
		///     Called once in the lifetime of the object, when it is created.
		///     Use it like <b>Awake</b> in a <b>MonoBehaviour</b>.
		/// </summary>
		protected abstract void OnInit();

		/// <summary>
		///     Called when the <b>SubStateMachine</b> is <b>Entered</b>.
		///     Use it to decide which should be the <b>Start</b> <b>State</b>, then return it.
		/// </summary>
		protected abstract ITickableState ResolveStartState();

		/// <summary>
		///     Called when the <b>Owner</b> gets <i>Enabled</i> and was previously <i>Disabled</i>,
		///     while this object is still being executed.
		/// </summary>
		protected abstract void OnEnable();

		/// <summary>
		///     Called when the <b>SubStateMachine</b> is <i>Entered</i>.
		/// </summary>
		protected abstract void OnEnter();

		/// <summary>
		///     Called every frame, before <b>OnTick</b>.
		/// </summary>
		protected abstract void OnEarlyTick();

		/// <summary>
		///     Called every frame.
		/// </summary>
		protected abstract void OnTick();

		/// <summary>
		///     Called at the same rate as <b>FixedUpdate</b>.
		/// </summary>
		protected abstract void OnFixedTick();

		/// <summary>
		///     Called every frame after <b>OnTick</b>.
		/// </summary>
		protected abstract void OnLateTick();

		/// <summary>
		///     Called when the <b>SubStateMachine</b> is <i>Exited</i>.
		/// </summary>
		protected abstract void OnExit();

		/// <summary>
		///     Called when the <b>Owner</b> is <i>Disabled</i> while this object is still being executed.
		/// </summary>
		protected abstract void OnDisable();

		/// <summary>
		///     Called when the <b>Owner</b> is <i>Destroyed</i> while this object is still being executed.
		/// </summary>
		protected abstract void OnDestroy();

	#endregion
	}
}
