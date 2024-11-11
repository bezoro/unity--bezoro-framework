namespace Bezoro.Framework.Unity
{
	public interface ITickable
	{
		void EarlyTick();

		void Tick();

		void FixedTick();

		void LateTick();
	}
}
