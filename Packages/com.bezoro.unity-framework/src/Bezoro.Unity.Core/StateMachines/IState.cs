namespace Bezoro.Unity.Core.StateMachines;

/// <summary>
///     Defines a state with enter, update, and exit lifecycle hooks.
/// </summary>
public interface IState
{
	public void Destroy();

	public void Disable();
	public void Enable();

	public void Enter();

	public void Exit();
}
