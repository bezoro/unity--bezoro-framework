using UnityEngine;
using Bezoro.Core.Helpers;

namespace Bezoro.StateMachines
{
	public abstract class BaseTickableSubStateMachine :
		ITickableSubStateMachine
	{
		public void Init
		(MonoBehaviour masterStateMachine,
		 ITickableStateMachine owningStateMachine)
		{
			_masterStateMachine = masterStateMachine;
			_owningStateMachine = owningStateMachine;
			SetUp();
		}

		public void Init
		(MonoBehaviour masterStateMachine,
		 IStateMachine owningStateMachine)
		{
			_masterStateMachine = masterStateMachine;
			_owningStateMachine = owningStateMachine as ITickableStateMachine;
			SetUp();
		}

		public MonoBehaviour MasterStateMachine => _masterStateMachine;

		public ITickableState StartState => _startState;

		public ITickableState CurrentState => _currentState;

		public ITickableState LastState => _lastState;

		public ITickTransition[] Transitions => _transitions;

		IState IStateMachine.StartState => _startState;

		IState IStateMachine.CurrentState => _currentState;

		IState IStateMachine.LastState => _lastState;

		ITransition[] IState.Transitions => _transitions;

		public ITickTransition TransitionsTo(ITickableState toState)
		{
			var newTransition =
				this.CreateTransition<TickTransition, ITickableState, ITickableState>(toState);

			ArrayHelpers.Add(ref _transitions, newTransition);
			return newTransition;
		}

		public void Enter()
		{
			_currentState = _startState;
			OnEnter();
			this.AllTransitionsBecomeRelevant();
			_currentState.Enter();
		}

		public void Disable()
		{
			OnDisable();
			this.Disable_Ext();
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

		protected abstract void SetUp();

		protected abstract void OnEnter();

		protected abstract void OnEarlyTick();

		protected abstract void OnTick();

		protected abstract void OnFixedTick();

		protected abstract void OnLateTick();

		protected abstract void OnDisable();

		protected abstract void OnExit();

		#endregion

		protected void SetStartState(ITickableState state) =>
			_startState = state;

		protected void RegisterState<T>(out T outState)
		where T : class, ITickableState, new() =>
			outState = this.CreateState<T>(_masterStateMachine);

		private ITickableState _startState = new NullTickableState();
		private ITickableState _currentState = new NullTickableState();
		private ITickableState _lastState = new NullTickableState();
		private ITickableStateMachine _owningStateMachine;
		private MonoBehaviour _masterStateMachine;
		private ITickTransition[] _transitions = new ITickTransition[4];
	}
}