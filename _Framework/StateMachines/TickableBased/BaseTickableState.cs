using UnityEngine;
using Bezoro.Core.Helpers;

namespace Bezoro.StateMachines
{
	public abstract class BaseTickableState : 
		ITickableState
	{
		public void Init
		(MonoBehaviour masterStateMachine,
		 ITickableStateMachine owningStateMachine)
		{
			_masterStateMachine = masterStateMachine;
			_owningStateMachine = owningStateMachine;
			OnInit();
		}

		void IState.Init
		(MonoBehaviour masterStateMachine,
		 IStateMachine owningStateMachine)
		{
			_masterStateMachine = masterStateMachine;
			_owningStateMachine = owningStateMachine;
			OnInit();
		}

		public MonoBehaviour MasterStateMachine => _masterStateMachine;

		public ITickTransition[] Transitions => _transitions;

		ITransition[] IState.Transitions => _transitions;

		public TickTransition TransitionsTo(ITickableState toState)
		{
			var newTransition = new TickTransition
				(this,
				 toState,
				 _masterStateMachine);

			ArrayHelpers.Add(ref _transitions, newTransition);
			return newTransition;
		}

		public void Enter()
		{
			OnEnter();
			this.AllTransitionsBecomeRelevant();
		}

		public void EarlyTick() =>
			OnEarlyTick();

		public void Tick() =>
			OnTick();

		public void FixedTick() =>
			OnFixedTick();

		public void LateTick()
		{
			OnLateTick();
			this.AllTransitionsTick();
		}

		public void Disable() =>
			OnDisable();

		public void OnTransitionTrue(ITickableState toState) =>
			_owningStateMachine.DoTransition(toState);

		public void Exit()
		{
			this.AllTransitionsCeaseRelevant();
			OnExit();
		}

		#region Abstract Methods

		protected abstract void OnInit();

		protected abstract void OnEnter();

		protected abstract void OnEarlyTick();

		protected abstract void OnTick();

		protected abstract void OnFixedTick();

		protected abstract void OnLateTick();

		protected abstract void OnDisable();

		protected abstract void OnExit();

		#endregion

		private MonoBehaviour _masterStateMachine;
		private IStateMachine _owningStateMachine;
		private ITickTransition[] _transitions = new ITickTransition[4];
	}

	public class NullTickableState : ITickableState
	{
		public void Init
		(MonoBehaviour masterStateMachine,
		 IStateMachine owningStateMachine) { }

		public void Init
		(MonoBehaviour masterStateMachine,
		 ITickableStateMachine owningStateMachine) { }

		public MonoBehaviour MasterStateMachine { get; }

		public ITickTransition[] Transitions { get; }

		ITransition[] IState.Transitions { get; }

		public void Enter() { }

		public void Exit() { }

		public void Disable() { }

		public void EarlyTick() { }

		public void Tick() { }

		public void FixedTick() { }

		public void LateTick() { }

		public void OnTransitionTrue(ITickableState toState) { }
	}
}