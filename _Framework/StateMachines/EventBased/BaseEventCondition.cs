using UnityEngine;

namespace Bezoro.StateMachines
{
	public abstract class BaseEventCondition
	{
		public void Init(EventTransition transition, MonoBehaviour stateMachine)
		{
			_transition = transition;
			_stateMachine = stateMachine;
			OnInit();
		}

		public MonoBehaviour StateMachine => _stateMachine;

		public void BecomeRelevant() =>
			OnBecomeRelevant();

		protected void Raise() =>
			_transition.OnConditionRaised();

		public void CeaseRelevant() =>
			OnCeaseRelevant();

		#region Abstract Methods

		/// <summary>
		/// Called once in the lifetime of the object, when it is created.
		/// Use it like <b>Awake</b> in a <b>MonoBehaviour</b>.
		/// </summary>
		protected abstract void OnInit();

		/// <summary>
		/// Called when the <b>State</b> it belongs to is <i>Entered</i>.
		/// </summary>
		protected abstract void OnBecomeRelevant();

		/// <summary>
		/// Called when the <b>State</b> it belongs to is <i>Exited</i>.
		/// </summary>
		protected abstract void OnCeaseRelevant();

		#endregion

		private EventTransition _transition;
		private MonoBehaviour _stateMachine;
	}
}