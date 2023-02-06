namespace Bezoro.StateMachines
{
	public interface ITickable
	{
		void EarlyTick();

		void Tick();

		void FixedTick();

		void LateTick();
	}
}