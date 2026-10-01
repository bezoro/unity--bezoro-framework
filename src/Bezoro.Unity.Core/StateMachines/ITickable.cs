namespace Bezoro.Unity.Core.StateMachines;

/// <summary>
///     Defines an object updated by a tick loop.
/// </summary>
public interface ITickable
{
	public void EarlyTick();

	public void FixedTick();

	public void LateTick();

	public void Tick();
}
