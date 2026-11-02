using UnityEngine;

namespace Bezoro.Unity.Core.StateMachines.TickableBased;

/// <summary>
///     Base class for conditions evaluated during tickable state transitions.
/// </summary>
public abstract class BaseTickCondition
{
	private TickableStateMachineOwner _owner = null!;
	public void Init(TickableStateMachineOwner owner, bool inverted = false)
	{
		_owner   = owner;
		Inverted = inverted;
		OnInit();
	}

	public MonoBehaviour Owner => _owner;

	public bool Inverted { get; private set; }

	public bool Tick() =>
		Inverted ? !OnTick() : OnTick();

	public void BecomeRelevant() =>
		OnBecomeRelevant();

	public void CeaseRelevant() =>
		OnCeaseRelevant();

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
