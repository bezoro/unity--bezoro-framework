using UnityEngine;
using Bezoro.Core.Helpers;

namespace Bezoro.StateMachines
{
	public abstract class BaseTickableSubStateMachine :
		ITickableSubStateMachine
	{
		public void Init(MonoBehaviour masterStateMachine, ITickableStateMachine owningStateMachine)
		{
			_masterStateMachine = masterStateMachine;
			_owningStateMachine = owningStateMachine;
			RegisterStates();
			SetUpTransitions();
		}

		void IState.Init(MonoBehaviour masterStateMachine, IStateMachine owningStateMachine) =>
			Init(masterStateMachine, owningStateMachine as ITickableStateMachine);

		public MonoBehaviour MasterStateMachine => _masterStateMachine;

		public ITickableState CurrentState => _currentState;

		IState IStateMachine.CurrentState => _currentState;

		public ITickableState LastState => _lastState;

		IState IStateMachine.LastState => _lastState;

		public ITickTransition[] Transitions => _transitions;

		ITransition[] IState.Transitions => _transitions;

		public ITickTransition TransitionsTo(ITickableState toState)
		{
			var newTransition = this.CreateTransition<TickTransition>(toState);
			ArrayHelpers.Add(ref _transitions, newTransition);
			return newTransition;
		}

		public void Enter()
		{
			OnEnter();
			this.AllTransitionsBecomeRelevant();
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
			this.AllTransitionsTick();
		}

		public void OnTransitionTrue(ITickableState toState) =>
			_owningStateMachine.DoTransition(toState);

		public void Exit()
		{
			_currentState.Exit();
			this.AllTransitionsCeaseRelevant();
			OnExit();
		}

		public void DoTransition(ITickableState toState) =>
			this.DoTransition_Ext(toState);

		void IStateMachine.DoTransition(IState toState) =>
			this.DoTransition_Ext(toState);

		public void SetLastState(ITickableState state) =>
			_lastState = state;

		void IStateMachine.SetLastState(IState state) =>
			_lastState = state as ITickableState;

		public void SetCurrentState(ITickableState state) =>
			_currentState = state;

		void IStateMachine.SetCurrentState(IState state) =>
			_currentState = state as ITickableState;

		#region Abstract Methods

		protected abstract void RegisterStates();

		protected abstract void SetUpTransitions();

		protected abstract ITickableState ResolveStartState();

		protected abstract void OnEnter();

		protected abstract void OnEarlyTick();

		protected abstract void OnTick();

		protected abstract void OnFixedTick();

		protected abstract void OnLateTick();

		protected abstract void OnExit();

		#endregion

		protected T RegisterState<T>()
		where T : class, ITickableState, new() =>
			this.CreateState<T, ITickableStateMachine>(_masterStateMachine);

		private ITickableState _startState = new NullTickableState();
		private ITickableState _currentState = new NullTickableState();
		private ITickableState _lastState = new NullTickableState();
		private ITickableStateMachine _owningStateMachine;
		private MonoBehaviour _masterStateMachine;
		private ITickTransition[] _transitions = new ITickTransition[4];
	}
}