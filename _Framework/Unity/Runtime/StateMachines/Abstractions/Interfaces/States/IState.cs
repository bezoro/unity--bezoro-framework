namespace Bezoro.Framework.Unity.StateMachines
{
	public interface IState
	{
		public void Enable();

		public void Enter();

		public void Exit();

		public void Disable();

		public void Destroy();
	}
}
