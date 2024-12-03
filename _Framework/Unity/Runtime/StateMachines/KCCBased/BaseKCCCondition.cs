using UnityEngine;

namespace Bezoro.Framework.Unity.StateMachines.KCCBased
{
	public abstract class BaseKCCCondition
	{
		public MonoBehaviour StateMachine { get; private set; }

		public bool Inverted { get; private set; }

		public void Init( MonoBehaviour stateMachine, bool inverted = false )
		{
			StateMachine = stateMachine;
			Inverted     = inverted;
			OnInit();
		}

		public void BecomeRelevant()
		{
			OnBecomeRelevant();
		}

		public bool Tick()
		{
			return Inverted ? !OnTick() : OnTick();
		}

		public void CeaseRelevant()
		{
			OnCeaseRelevant();
		}

	#region Abstract Methods

		/// <summary>
		///     Called once in the lifetime of the object, when it is created.
		///     Use it like <b>Awake</b> in a <b>MonoBehaviour</b>.
		/// </summary>
		protected abstract void OnInit();

		/// <summary>
		///     Called when the <b>State</b> it belongs to is <i>Entered</i> or <i>Enabled</i>.
		/// </summary>
		protected abstract void OnBecomeRelevant();

		protected abstract bool OnTick();

		/// <summary>
		///     Called when the <b>State</b> it belongs to is <i>Exited</i>, <i>Disabled</i> or <i>Destroyed</i>.
		/// </summary>
		protected abstract void OnCeaseRelevant();

	#endregion
	}
}
