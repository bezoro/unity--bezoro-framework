using UnityEngine;

namespace Bezoro.Unity.Core.StateMachines.EventBased;

/// <summary>
///     Base class for conditions evaluated by event-driven state transitions.
/// </summary>
public abstract class BaseEventCondition
{
	private EventStateMachineOwner _owner = null!;
	private EventTransition        _transition = null!;
	public void Init(EventStateMachineOwner owner, EventTransition transition)
	{
		_transition = transition;
		_owner      = owner;
		OnInit();
	}

	public MonoBehaviour Owner => _owner;

	public void BecomeRelevant() =>
		OnBecomeRelevant();

	public void CeaseRelevant() =>
		OnCeaseRelevant();

	protected void Raise() =>
		_transition.OnConditionRaised();

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

	/// <summary>
	///     Called when the <b>State</b> it belongs to is <i>Exited</i>, <i>Disabled</i> or <i>Destroyed</i>.
	/// </summary>
	protected abstract void OnCeaseRelevant();

	#endregion
}
