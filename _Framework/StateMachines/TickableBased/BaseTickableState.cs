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

		public TickTransition[] Transitions => _transitions;

		public void Enter()
		{
			OnEnter();
			AllTransitionsBecomeRelevant();

			void AllTransitionsBecomeRelevant()
			{
				for (var i = 0; i < _transitions.Length; i++)
				{
					if (_transitions[i] == null)
						continue;

					_transitions[i].BecomeRelevant();
				}
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
			AllTransitionsCeaseRelevant();
			OnExit();

			void AllTransitionsCeaseRelevant()
			{
				for (var i = 0; i < _transitions.Length; i++)
				{
					if (_transitions[i] == null)
						continue;

					_transitions[i].CeaseRelevant();
				}
			}
		}

		public TickTransition TransitionsTo(ITickableState toState)
		{
			var newTransition = new TickTransition();
			newTransition.Init(this, toState, _masterStateMachine);
			ArrayHelpers.Add(ref _transitions, newTransition);
			return newTransition;
		}

		#region Abstract Methods

		/// <summary>
		/// Called once in the lifetime of the object, when it is created.
		/// Use it like <b>Awake</b> in a <b>MonoBehaviour</b>.
		/// </summary>
		protected abstract void OnInit();

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

		#endregion

		private MonoBehaviour _masterStateMachine;
		private ITickableStateMachine _owningStateMachine;
		private TickTransition[] _transitions = new TickTransition[4];
	}
}