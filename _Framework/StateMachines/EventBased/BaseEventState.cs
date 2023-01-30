using System;
using UnityEngine;
using Bezoro.Core.Helpers;

namespace Bezoro.StateMachines
{
	public abstract class BaseEventState :
		IEventState
	{
		public void Init(MonoBehaviour masterStateMachine, IEventStateMachine owningStateMachine)
		{
			_masterStateMachine = masterStateMachine;
			_owningStateMachine = owningStateMachine;
			OnInit();
		}

		void IState.Init(MonoBehaviour masterStateMachine, IStateMachine owningStateMachine) =>
			Init(masterStateMachine, owningStateMachine as IEventStateMachine);

		public MonoBehaviour MasterStateMachine => _masterStateMachine;

		public ITransition[] Transitions => _transitions;

		public void Enter()
		{
			OnEnter();
			this.AllTransitionsBecomeRelevant();
		}

		public void OnTransitionRaised(IEventState toState) =>
			_owningStateMachine.DoTransition(toState);

		public void Exit()
		{
			this.AllTransitionsCeaseRelevant();
			OnExit();
		}

		public IEventTransition TransitionsTo(IEventState toState)
		{
			var newTransition = this.CreateTransition<EventTransition>(toState);
			ArrayHelpers.Add(ref _transitions, newTransition);
			return newTransition;
		}

		#region Abstract Methods

		protected abstract void OnInit();

		protected abstract void OnEnter();

		protected abstract void OnExit();

		#endregion

		private MonoBehaviour _masterStateMachine;
		private IEventStateMachine _owningStateMachine;
		private ITransition[] _transitions = new ITransition[4];
	}

	public sealed class NullEventState : IEventState
	{
		public void Init
			(MonoBehaviour masterStateMachine, IEventStateMachine owningStateMachine) { }

		void IState.Init(MonoBehaviour masterStateMachine, IStateMachine owningStateMachine) { }

		public MonoBehaviour MasterStateMachine { get; }

		public ITransition[] Transitions { get; } = Array.Empty<ITransition>();

		public void Enter() { }

		public void Exit() { }

		public void OnTransitionRaised(IEventState toState) { }
	}
}