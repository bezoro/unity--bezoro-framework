using UnityEngine;
using Bezoro.Core.Helpers;

namespace Bezoro.StateMachines
{
	public abstract class BaseTickableSubStateMachine :
		ITickableStateMachine, ITickableState
	{
		public void Init(MonoBehaviour masterStateMachine, ITickableStateMachine owningStateMachine)
		{
			_masterStateMachine = masterStateMachine;
			_owningStateMachine = owningStateMachine;
			RegisterStates();
			SetUpTransitions();
		}

		public MonoBehaviour MasterStateMachine => _masterStateMachine;

		public ITickableState CurrentState => _currentState;

		public ITickableState LastState => _lastState;

		public void Enter()
		{
			OnEnter();

			for (var i = 0; i < _transitions.Length; i++)
			{
				if (_transitions[i] == null)
					continue;

				_transitions[i].BecomeRelevant();
			}

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

			for (var i = 0; i < _transitions.Length; i++)
			{
				if (_transitions[i] == null)
					continue;

				_transitions[i].Tick();
			}
		}

		public void OnTransitionTrue(ITickableState toState) =>
			_owningStateMachine.DoTransition(toState);

		public void Exit()
		{
			_currentState.Exit();
			for (var i = 0; i < _transitions.Length; i++)
			{
				if (_transitions[i] == null)
					continue;

				_transitions[i].CeaseRelevant();
			}

			OnExit();
		}

		public TickTransition TransitionsTo(ITickableState toState)
		{
			var newTransition = new TickTransition();
			newTransition.Init(this, toState, _masterStateMachine);
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

		protected abstract void RegisterStates();

		protected abstract void SetUpTransitions();

		protected abstract BaseTickableState ResolveStartState();

		protected abstract void OnEnter();

		protected abstract void OnEarlyTick();

		protected abstract void OnTick();

		protected abstract void OnFixedTick();

		protected abstract void OnLateTick();

		protected abstract void OnExit();

		#endregion

		protected T RegisterState<T>()
		where T : class, ITickableState, new()
		{
			var newState = new T();
			newState.Init(_masterStateMachine, this);
			return newState;
		}

		private MonoBehaviour _masterStateMachine;
		private ITickableStateMachine _owningStateMachine;
		private ITickableState _currentState;
		private ITickableState _lastState;
		private TickTransition[] _transitions = new TickTransition[4];
	}
}