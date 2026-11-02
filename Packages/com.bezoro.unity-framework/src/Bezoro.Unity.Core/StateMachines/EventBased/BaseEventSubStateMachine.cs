using Bezoro.Core.Helpers;
using UnityEngine;

namespace Bezoro.Unity.Core.StateMachines.EventBased;

/// <summary>
///     Embeds an event-based state machine inside a parent event state.
/// </summary>
public abstract class BaseEventSubStateMachine :
	IEventStateMachine,
	IEventState
{
	private bool                   _hasEntered;
	private EventStateMachineOwner _owner              = null!;
	private EventTransition[]      _transitions        = new EventTransition[4];
	private IEventStateMachine     _owningStateMachine = null!;

	public void Init(EventStateMachineOwner owner, IEventStateMachine owningStateMachine)
	{
		_owner              = owner;
		_owningStateMachine = owningStateMachine;
		RegisterStates();
		SetUpTransitions();
		OnInit();
	}

	public EventTransition[] Transitions => _transitions;

	public MonoBehaviour Owner => _owner;

	public IEventState CurrentState { get; private set; } = null!;
	public IEventState LastState    { get; private set; } = null!;

	public EventTransition TransitionsTo(IEventState toState)
	{
		var newTransition = new EventTransition();
		newTransition.Init(
			this,
			toState,
			_owner
		);

		ArrayHelper.Add(
			ref _transitions,
			newTransition
		);

		return newTransition;
	}

	protected T RegisterState<T>()
		where T : class, IEventState, new()
	{
		var newState = new T();

		newState.Init(
			_owner,
			this
		);

		return newState;
	}

	private void AllTransitionsBecomeRelevant()
	{
		for (var i = 0; i < _transitions.Length; i++)
		{
			if (_transitions[i] == null) continue;

			_transitions[i].BecomeRelevant();
		}
	}

	private void AllTransitionsCeaseRelevant()
	{
		for (var i = 0; i < _transitions.Length; i++)
		{
			if (_transitions[i] == null) continue;

			_transitions[i].CeaseRelevant();
		}
	}

	#region Interface Implementations

	public void Enable()
	{
		OnEnable();
		if (!_hasEntered) return;

		AllTransitionsBecomeRelevant();
		CurrentState.Enable();
	}

	public void Enter()
	{
		OnEnter();
		AllTransitionsBecomeRelevant();
		CurrentState = ResolveStartState();
		CurrentState.Enter();
		_hasEntered = true;
	}

	public void OnTransitionRaised(IEventState toState) =>
		_owningStateMachine.DoTransition(
			toState
		);

	public void Exit()
	{
		OnExit();
		if (!_hasEntered) return;

		AllTransitionsCeaseRelevant();
		CurrentState.Exit();
		_hasEntered = false;
	}

	public void Disable()
	{
		OnDisable();
		if (!_hasEntered) return;

		AllTransitionsCeaseRelevant();
		CurrentState.Disable();
	}

	public void Destroy()
	{
		OnDestroy();
		if (!_hasEntered) return;

		AllTransitionsCeaseRelevant();
		CurrentState.Destroy();
		_hasEntered = false;
	}

	public void DoTransition(IEventState toState)
	{
		if (!_hasEntered)
		{
			CurrentState = toState;
			CurrentState.Enter();
			_hasEntered = true;
			return;
		}

		CurrentState.Exit();
		LastState    = CurrentState;
		CurrentState = toState;
		CurrentState.Enter();
	}

	#endregion

	#region Abstract Methods

	/// <summary>
	///     Called once in the lifetime of the object, when it is created.
	///     Use it to register all <b>States</b> needed for the <b>SubStateMachine</b> to work.
	/// </summary>
	protected abstract void RegisterStates();

	/// <summary>
	///     Called once in the lifetime of the object, when it is created.
	///     Use it to set up all of the <b>States</b> <b>Transitions</b>.
	/// </summary>
	protected abstract void SetUpTransitions();

	/// <summary>
	///     Called once in the lifetime of the object, when it is created.
	///     Use it like <b>Awake</b> in a <b>MonoBehaviour</b>.
	/// </summary>
	protected abstract void OnInit();

	/// <summary>
	///     Called when the <b>SubStateMachine</b> is <b>Entered</b>.
	///     Use it to decide which should be the <b>Start</b> <b>State</b>, then return it.
	/// </summary>
	protected abstract IEventState ResolveStartState();

	/// <summary>
	///     Called when the <b>Owner</b> gets <i>Enabled</i> and was previously <i>Disabled</i>,
	///     while this object is still being executed.
	/// </summary>
	protected abstract void OnEnable();

	/// <summary>
	///     Called when the <b>SubStateMachine</b> is <i>Entered</i>.
	/// </summary>
	protected abstract void OnEnter();

	/// <summary>
	///     Called when the <b>SubStateMachine</b> is <i>Exited</i>.
	/// </summary>
	protected abstract void OnExit();

	/// <summary>
	///     Called when the <b>Owner</b> is <i>Disabled</i> while this object is still being executed.
	/// </summary>
	protected abstract void OnDisable();

	/// <summary>
	///     Called when the <b>Owner</b> is <i>Destroyed</i> while this object is still being executed.
	/// </summary>
	protected abstract void OnDestroy();

	#endregion
}
