using UnityEngine;

namespace Bezoro.StateMachines
{
	public abstract class BaseTickCondition
	{
		public void Init(TickableStateMachineOwner owner, bool inverted = false)
		{
			_owner = owner;
			_inverted = inverted;
			OnInit();
		}

		public MonoBehaviour Owner => _owner;

		public bool Inverted => _inverted;

		public void BecomeRelevant() =>
			OnBecomeRelevant();

		public bool Tick() =>
			_inverted ? !OnTick() : OnTick();

		public void CeaseRelevant() =>
			OnCeaseRelevant();

		#region Abstract Methods

		/// <summary>
		/// Called once in the lifetime of the object, when it is created.
		/// Use it like <b>Awake</b> in a <b>MonoBehaviour</b>.
		/// </summary>
		protected abstract void OnInit();

		/// <summary>
		/// Called when the <b>State</b> it belongs to is <i>Entered</i> or <i>Enabled</i>.
		/// </summary>
		protected abstract void OnBecomeRelevant();

		protected abstract bool OnTick();

		/// <summary>
		/// Called when the <b>State</b> it belongs to is <i>Exited</i>, <i>Disabled</i> or <i>Destroyed</i>.
		/// </summary>
		protected abstract void OnCeaseRelevant();

		#endregion

		private TickableStateMachineOwner _owner;
		private bool _inverted;
	}
}