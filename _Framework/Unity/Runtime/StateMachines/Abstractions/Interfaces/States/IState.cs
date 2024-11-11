namespace Bezoro.Framework.Unity.States
{
	public interface IState
	{
		void Enable();

		void Enter();

		void Exit();

		void Disable();

		void Destroy();
	}
}
