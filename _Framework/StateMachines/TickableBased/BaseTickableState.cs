using UnityEngine;
using Bezoro.Core.Helpers;

namespace Bezoro.StateMachines
{
	public abstract class BaseTickableState :
		ITickableState
	{
		public void Init(MonoBehaviour masterStateMachine, ITickableStateMachine owningStateMachine)
		{
			_masterStateMachine = masterStateMachine;
			_owningStateMachine = owningStateMachine;
			OnInit();
		}

		public MonoBehaviour MasterStateMachine => _masterStateMachine;

		public void Enter()
		{
			OnEnter();
			for (var i = 0; i < _transitions.Length; i++)
			{
				if (_transitions[i] == null)
					continue;

				_transitions[i].BecomeRelevant();
			}
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

		#region Abstract Methods

		protected abstract void OnInit();

		protected abstract void OnEnter();

		protected abstract void OnEarlyTick();

		protected abstract void OnTick();

		protected abstract void OnFixedTick();

		protected abstract void OnLateTick();

		protected abstract void OnExit();

		#endregion

		private MonoBehaviour _masterStateMachine;
		private ITickableStateMachine _owningStateMachine;
		private TickTransition[] _transitions = new TickTransition[4];
	}
}