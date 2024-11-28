using Bezoro.Framework.Core;
using UnityEngine;

namespace Bezoro.Framework.Unity.StateMachines.TickableBased
{
	public abstract class BaseTickableState :
		ITickableState
	{
		public void Init( TickableStateMachineOwner owner, ITickableStateMachine owningStateMachine )
		{
			_owner              = owner;
			_owningStateMachine = owningStateMachine;
			OnInit();
		}

		public MonoBehaviour Owner => _owner;

		public TickTransition[] Transitions => _transitions;

		public void Enable()
		{
			OnEnable();
			AllTransitionsBecomeRelevant();
		}

		public void Enter()
		{
			OnEnter();
			AllTransitionsBecomeRelevant();
		}

		public void EarlyTick()
		{
			OnEarlyTick();
		}

		public void Tick()
		{
			OnTick();
		}

		public void FixedTick()
		{
			OnFixedTick();
		}

		public void LateTick()
		{
			OnLateTick();
			AllTransitionsTick();

			void AllTransitionsTick()
			{
				for ( var i = 0 ; i < _transitions.Length ; i++ )
				{
					if ( _transitions[i] == null )
					{
						continue;
					}

					_transitions[i].Tick();
				}
			}
		}

		public void OnTransitionRaised( ITickableState toState )
		{
			_owningStateMachine.DoTransition( toState );
		}

		public void Exit()
		{
			OnExit();
			AllTransitionsCeaseRelevant();
		}

		public void Disable()
		{
			OnDisable();
			AllTransitionsCeaseRelevant();
		}

		public void Destroy()
		{
			OnDestroy();
			AllTransitionsCeaseRelevant();
		}

		public TickTransition TransitionsTo( ITickableState toState )
		{
			var newTransition = new TickTransition();
			newTransition.Init( this, toState, _owner );
			Array_Helpers.Add( ref _transitions, newTransition );

			return newTransition;
		}

	#region Abstract Methods

		/// <summary>
		///    Called once in the lifetime of the object, when it is created.
		///    Use it like <b>Awake</b> in a <b>MonoBehaviour</b>.
		/// </summary>
		protected abstract void OnInit();

		/// <summary>
		///    Called when the <b>Owner</b> gets <i>Enabled</i> and was previously <i>Disabled</i>,
		///    while this object is still being executed.
		/// </summary>
		protected abstract void OnEnable();

		/// <summary>
		///    Called when the <b>State</b> is <i>Entered</i>.
		/// </summary>
		protected abstract void OnEnter();

		/// <summary>
		///    Called every frame, before <b>OnTick</b>.
		/// </summary>
		protected abstract void OnEarlyTick();

		/// <summary>
		///    Called every frame.
		/// </summary>
		protected abstract void OnTick();

		/// <summary>
		///    Called at the same rate as <b>FixedUpdate</b>.
		/// </summary>
		protected abstract void OnFixedTick();

		/// <summary>
		///    Called every frame after <b>OnTick</b>.
		/// </summary>
		protected abstract void OnLateTick();

		/// <summary>
		///    Called when the <b>State</b> is <i>Exited</i>.
		/// </summary>
		protected abstract void OnExit();

		/// <summary>
		///    Called when the <b>Owner</b> is <i>Disabled</i> while this object is still being executed.
		/// </summary>
		protected abstract void OnDisable();

		/// <summary>
		///    Called when the <b>Owner</b> is <i>Destroyed</i> while this object is still being executed.
		/// </summary>
		protected abstract void OnDestroy();

	#endregion

		private void AllTransitionsBecomeRelevant()
		{
			for ( var i = 0 ; i < _transitions.Length ; i++ )
			{
				if ( _transitions[i] == null )
				{
					continue;
				}

				_transitions[i].BecomeRelevant();
			}
		}

		private void AllTransitionsCeaseRelevant()
		{
			for ( var i = 0 ; i < _transitions.Length ; i++ )
			{
				if ( _transitions[i] == null )
				{
					continue;
				}

				_transitions[i].CeaseRelevant();
			}
		}

		private TickableStateMachineOwner _owner;
		private ITickableStateMachine     _owningStateMachine;
		private TickTransition[]          _transitions = new TickTransition[4];
	}
}
