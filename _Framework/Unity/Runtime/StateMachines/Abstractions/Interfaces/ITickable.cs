namespace Bezoro.Framework.Unity.StateMachines
{
	public interface ITickable
	{
		public void EarlyTick();

		public void Tick();

		public void FixedTick();

		public void LateTick();
	}
}
