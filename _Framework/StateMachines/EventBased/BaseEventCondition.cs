using UnityEngine;

namespace Bezoro.StateMachines
{
	public abstract class BaseEventCondition
	{
		public void Init(EventStateMachineOwner owner, EventTransition transition)
		{
			_transition = transition;
			_owner = owner;
			OnInit();
		}

		public MonoBehaviour Owner => _owner;

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

		private EventStateMachineOwner _owner;
		private EventTransition _transition;
	}
}